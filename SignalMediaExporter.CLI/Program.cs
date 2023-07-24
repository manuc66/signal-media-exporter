// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;
using System.Text.Json;
using HeyRed.Mime;
using SQLite;

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

string signalDirectory = Path.GetDirectoryName(Path.GetDirectoryName(dbLocationPath)) ??
                         throw new Exception("Impossible to traverse file directory to find the location of the Signal config file");
string signalConfigFilePath = Path.Combine(signalDirectory, "config.json");

if (!File.Exists(signalConfigFilePath))
{
    throw new NotSupportedException($"Signal configuration file not found: {signalConfigFilePath}");
}

Console.WriteLine("Database location: " + dbLocationPath);
Console.WriteLine("Configuration location: " + signalDirectory);

const string exportLocation = ;


string jsonData = File.ReadAllText(signalConfigFilePath);

// Deserialize the JSON data into the MyData class
SignalConfig? data = JsonSerializer.Deserialize<SignalConfig>(jsonData);


Console.WriteLine("Key: " + data?.Key);
using SQLiteConnection connection = OpenSignalDatabase(dbLocationPath, data?.Key ?? string.Empty);

SQLiteCommand sqLiteCommand = connection.CreateCommand("select id, json from messages where hasAttachments=1");
List<Message> executeQuery = sqLiteCommand.ExecuteQuery<Message>();


foreach (Message message in executeQuery)
{
    SaveMessage(message, signalDirectory, exportLocation);
}

Console.WriteLine("Total messages with attachment: " + executeQuery.Count);


static SQLiteConnection OpenSignalDatabase(string databasePath, string passphrase)
{
    SQLitePCL.Batteries_V2.Init();


    byte[] key = Convert.FromHexString(passphrase);


    // Create a new SQLiteConnection with the key
    SQLiteConnectionString sqLiteConnectionString = new SQLiteConnectionString(databasePath, SQLiteOpenFlags.ReadOnly, false, key: key);
    SQLiteConnection connection = new SQLiteConnection(sqLiteConnectionString);

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

static void Save(Attachment attachments, string signalDirectory, string exportLocation1, long receivedAt)
{
    string? fileName = GetFileName(attachments);
    if (!string.IsNullOrEmpty(attachments.path) && fileName != null)
    {
        string currentFileLocation = Path.Combine(signalDirectory, "attachment.noindex", attachments.path);

        string destFilepath = Path.Combine(exportLocation1, fileName);
        if (Path.Exists(destFilepath))
        {
            destFilepath = Path.Combine(exportLocation1, Guid.NewGuid() + fileName);
        }

        DateTime when = new DateTime(1970, 1, 1).AddMilliseconds(receivedAt).ToLocalTime();


        Console.WriteLine($"{fileName}: {currentFileLocation} @{when:yyyy-MM-dd} -> {destFilepath}");
        File.Copy(currentFileLocation, destFilepath);
        FileInfo fileInfo = new FileInfo(destFilepath)
        {
            LastWriteTime = when,
            CreationTime = when
        };
    }
}

static void SaveMessage(Message message1, string signalDirectory, string exportLocation)
{
    if (message1?.json == null)
    {
        return;
    }

    MessageContent? rootObject = JsonSerializer.Deserialize<MessageContent>(message1.json);
    if (rootObject?.attachments?.Length > 0)
    {
        long receivedAt = rootObject.received_at;
        foreach (Attachment attachment in rootObject.attachments)
        {
            Save(attachment, signalDirectory, exportLocation, receivedAt);
        }
    }
}