using System.CommandLine;
using System.Runtime.InteropServices;
using System.Text.Json;
using manuc66.SignalMediaExporter.CLI.Models;
using Microsoft.Extensions.Logging;
using SignalMediaExporter.Core;
using SQLite;

namespace manuc66.SignalMediaExporter.CLI;

public static class Program
{
    static async Task<int> Main(string[] args)
    {
        Option<DirectoryInfo?> destinationOption = new(
            name: "--destination",
            description: "The destination folder")
        {
            IsRequired = true,
            Arity = ArgumentArity.ExactlyOne
        };

        RootCommand rootCommand = new("Export Signal message attachment to a directory");
        rootCommand.AddOption(destinationOption);

        rootCommand.SetHandler((file, logger) => { ExportAttachments(logger, file!); },
            destinationOption, new LoggerBinder());

        return await rootCommand.InvokeAsync(args);
    }

    private static void ExportAttachments(ILogger logger, DirectoryInfo exportLocation)
    {
        string dbLocationPath = GetDbLocationPath();

        string signalDirectory = Path.GetDirectoryName(Path.GetDirectoryName(dbLocationPath)) ??
                                 throw new Exception("Impossible to traverse file directory to find the logger of the Signal config file");

        string signalConfigFilePath = GetSignalConfigFilePath(signalDirectory);

        logger.LogInformation("Database logger: " + dbLocationPath);
        logger.LogInformation("Configuration logger: " + signalDirectory);

        string jsonData = File.ReadAllText(signalConfigFilePath);

        SignalConfig? data = JsonSerializer.Deserialize<SignalConfig>(jsonData);

        logger.LogInformation("Key: " + data?.Key);

        using SQLiteConnection connection = DatabaseConnectionProvider.OpenSignalDatabase(dbLocationPath, data?.Key ?? string.Empty);
        MessageRepository messageRepository = new(connection);

        long messageWithAttachmentCount = messageRepository.GetMessageWithAttachmentCount();
        logger.LogInformation($"Number of message with attachment to process: {messageWithAttachmentCount}");

        List<Message> messageWithAttachments = messageRepository.GetMessageWithAttachments();

        AttachmentExporter attachmentExporter = new();
        foreach (Message message in messageWithAttachments)
        {
            attachmentExporter.SaveMessageAttachments(logger, message, signalDirectory, exportLocation.FullName);
        }

        logger.LogInformation("Total messages with attachment: " + messageWithAttachments.Count);
    }


    static string GetSignalConfigFilePath(string signalDirectory)
    {
        string signalConfigFilePath = Path.Combine(signalDirectory, "config.json");

        if (!File.Exists(signalConfigFilePath))
        {
            throw new NotSupportedException($"Signal configuration file not found: {signalConfigFilePath}");
        }

        return signalConfigFilePath;
    }

    static string GetDbLocationPath()
    {
        string dbLocationPath;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            dbLocationPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Roaming\\Signal\\sql\\db.sqlite";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            dbLocationPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.config/Signal/sql/db.sqlite";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            dbLocationPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Library/Application Support/Signal/sql/db.sqlite";
        }
        else
        {
            throw new NotSupportedException("Unsupported operating system");
        }

        dbLocationPath = Path.GetFullPath(dbLocationPath);

        if (!File.Exists(dbLocationPath))
        {
            throw new NotSupportedException("Signal db not found at: " + dbLocationPath);
        }

        return dbLocationPath;
    }




}