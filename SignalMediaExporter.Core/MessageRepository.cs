using System.Text.Json;
using manuc66.SignalMediaExporter.CLI.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace SignalMediaExporter.Core;

public class MessageRepository
{
    private readonly SQLiteConnection _sqLiteConnection;
    public MessageRepository(SQLiteConnection sqLiteConnection)
    {
        _sqLiteConnection = sqLiteConnection;
    }

    public long GetMessageWithAttachmentCount()
    {
        SQLiteCommand sqLiteCommand = _sqLiteConnection.CreateCommand("select count(id) from messages where hasAttachments=1 order by id");
        return sqLiteCommand.ExecuteScalar<long>();
    }


    public List<Message> GetMessageWithAttachments()
    {
        SQLiteCommand sqLiteCommand = _sqLiteConnection.CreateCommand("select id, json from messages where hasAttachments=1 order by id");
        List<Message> messages = sqLiteCommand.ExecuteQuery<Message>();
        return messages;
    }

    public static MessageRepository CreateMessageRepository(ILogger logger, string signalDirectory, string dbLocationPath)
    {
        string signalConfigFilePath = GetSignalConfigFilePath(signalDirectory);

        logger.LogInformation("Database: " + dbLocationPath);
        logger.LogInformation("Configuration: " + signalDirectory);

        string jsonData = File.ReadAllText(signalConfigFilePath);

        SignalConfig? data = JsonSerializer.Deserialize<SignalConfig>(jsonData);

        logger.LogInformation("Key: " + data?.Key);

        using SQLiteConnection connection = DatabaseConnectionProvider.OpenSignalDatabase(dbLocationPath, data?.Key ?? string.Empty);
        
        MessageRepository messageRepository = new(connection);
        return messageRepository;
    }
   private static string GetSignalConfigFilePath(string signalDirectory)
    {
        string signalConfigFilePath = Path.Combine(signalDirectory, "config.json");

        if (!File.Exists(signalConfigFilePath))
        {
            throw new NotSupportedException($"Signal configuration file not found: {signalConfigFilePath}");
        }

        return signalConfigFilePath;
    }
}