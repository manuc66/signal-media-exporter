using System.Text.Json;
using manuc66.SignalMediaExporter.Core.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace manuc66.SignalMediaExporter.Core;

public class MessageRepository(SQLiteConnection sqLiteConnection) : IDisposable
{
    public long GetMessageWithAttachmentCount()
    {
        SQLiteCommand sqLiteCommand = sqLiteConnection.CreateCommand("select count(id) from messages where hasAttachments=1 order by id");
        return sqLiteCommand.ExecuteScalar<long>();
    }


    public List<Message> GetMessageWithAttachments()
    {
        SQLiteCommand sqLiteCommand = sqLiteConnection.CreateCommand("select id, json from messages where hasAttachments=1 order by id");
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

        SQLiteConnection connection = DatabaseConnectionProvider.OpenSignalDatabase(dbLocationPath, data?.Key ?? string.Empty);
        
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

    public void Dispose()
    {
        sqLiteConnection.Dispose();
    }
}