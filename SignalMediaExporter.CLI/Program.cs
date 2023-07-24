using System.CommandLine;
using System.Runtime.InteropServices;
using System.Text.Json;
using HeyRed.Mime;
using manuc66.SignalMediaExporter.CLI.Models;
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

        RootCommand rootCommand = new RootCommand("Export Signal message attachment to a directory");
        rootCommand.AddOption(destinationOption);

        rootCommand.SetHandler((file) => { ExportAttachments(file!); },
            destinationOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static void ExportAttachments(DirectoryInfo exportLocation)
    {
        string dbLocationPath = GetDbLocationPath();

        string signalDirectory = Path.GetDirectoryName(Path.GetDirectoryName(dbLocationPath)) ??
                                 throw new Exception("Impossible to traverse file directory to find the location of the Signal config file");

        string signalConfigFilePath = GetSignalConfigFilePath(signalDirectory);

        Console.WriteLine("Database location: " + dbLocationPath);
        Console.WriteLine("Configuration location: " + signalDirectory);

        string jsonData = File.ReadAllText(signalConfigFilePath);

        SignalConfig? data = JsonSerializer.Deserialize<SignalConfig>(jsonData);

        Console.WriteLine("Key: " + data?.Key);

        using SQLiteConnection connection = OpenSignalDatabase(dbLocationPath, data?.Key ?? string.Empty);

        List<Message> messageWithAttachments = GetMessageWithAttachments(connection);

        foreach (Message message in messageWithAttachments)
        {
            SaveMessage(message, signalDirectory, exportLocation.FullName);
        }

        Console.WriteLine("Total messages with attachment: " + messageWithAttachments.Count);
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

    static SQLiteConnection OpenSignalDatabase(string databasePath, string passphrase)
    {
        SQLitePCL.Batteries_V2.Init();
        byte[] key = Convert.FromHexString(passphrase);

        SQLiteConnectionString sqLiteConnectionString = new(databasePath, SQLiteOpenFlags.ReadOnly, false, key: key);
        SQLiteConnection connection = new(sqLiteConnectionString);

        return connection;
    }

    static string? GetFileName(Attachment attachment)
    {
        char[] invalidFileNameChars = Path.GetInvalidPathChars().Union(new[] { '?' }).ToArray();

        if (attachment.path == null)
        {
            return null;
        }

        string fileName;
        if (!string.IsNullOrEmpty(attachment.fileName)
            && !attachment.fileName.Any(x => invalidFileNameChars.Any(z => x == z)))
        {
            fileName = attachment.fileName;
        }
        else
        {
            fileName = Path.GetFileName(attachment.path);
        }

        if (String.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            fileName += "." + MimeTypesMap.GetExtension(attachment.contentType);
        }

        return fileName;
    }

    static bool Save(Attachment attachments, string signalDirectory, string exportLocation, long receivedAt)
    {
        string? fileName = GetFileName(attachments);
        if (string.IsNullOrEmpty(attachments.path) || fileName == null)
        {
            return false;
        }

        string currentFileLocation = Path.Combine(signalDirectory, "attachments.noindex", attachments.path);

        string destFilepath = Path.Combine(exportLocation, fileName);
        if (Path.Exists(destFilepath))
        {
            destFilepath = Path.Combine(exportLocation, Guid.NewGuid() + fileName);
        }

        DateTime when = new DateTime(1970, 1, 1).AddMilliseconds(receivedAt).ToLocalTime();


        Console.WriteLine($"{fileName}: {currentFileLocation} @{when:yyyy-MM-dd} -> {destFilepath}");
        File.Copy(currentFileLocation, destFilepath);
        FileInfo fileInfo = new(destFilepath)
        {
            LastWriteTime = when,
            CreationTime = when
        };
        fileInfo.LastWriteTime = when;
        return true;
    }

    static bool SaveMessage(Message message, string signalDirectory, string exportLocation)
    {
        if (message.json == null)
        {
            return false;
        }

        MessageContent? rootObject = JsonSerializer.Deserialize<MessageContent>(message.json);
        if (!(rootObject?.attachments?.Length > 0))
        {
            return false;
        }

        long receivedAt = rootObject.received_at;

        bool saved = false;
        foreach (Attachment attachment in rootObject.attachments)
        {
            saved |= Save(attachment, signalDirectory, exportLocation, receivedAt);
        }

        return saved;

    }

    static List<Message> GetMessageWithAttachments(SQLiteConnection sqLiteConnection)
    {
        SQLiteCommand sqLiteCommand = sqLiteConnection.CreateCommand("select id, json from messages where hasAttachments=1");
        List<Message> messages = sqLiteCommand.ExecuteQuery<Message>();
        return messages;
    }
}