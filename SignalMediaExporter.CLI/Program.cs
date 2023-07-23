// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using SQLiteConnection connection = OpenSignalDatabase(dbLocationPath, data.Key);

SQLiteCommand sqLiteCommand = connection.CreateCommand("select id, json from messages where hasAttachments=1");
List<Message> executeQuery = sqLiteCommand.ExecuteQuery<Message>();

char[] invalidFileNameChars = Path.GetInvalidPathChars().Union(new char[] {'?'}).ToArray();
foreach (Message message in executeQuery)
{
    RootObject? rootObject = JsonSerializer.Deserialize<RootObject>(message.json);
    if (rootObject?.attachments?.Length > 0)
    {
        foreach (Attachments attachment in rootObject.attachments)
        {
            if (!string.IsNullOrEmpty(attachment.path))
            {
                string location = Path.Combine(signalDirectory, "attachments.noindex", attachment.path);

                string fileName = !string.IsNullOrEmpty(attachment.fileName) ? attachment.fileName : Path.GetFileName(location);

                if (fileName.Any( x =>
                    {
                        return invalidFileNameChars.Any(z => x == z);
                    }))
                {
                    fileName = Path.GetFileName(location);
                }


                string destFileName = Path.Combine(exportLocation, fileName);
                if (Path.Exists(destFileName))
                {
                    destFileName = Path.Combine(exportLocation, Guid.NewGuid() + fileName);
                }
                
                
                var when = (new DateTime(1970, 1, 1)).AddMilliseconds(rootObject.received_at).ToLocalTime();
                
                
                Console.WriteLine($"{fileName}: {location} @{when:yyyy-MM-dd} -> {destFileName}");
                File.Copy(location, destFileName);
                FileInfo fileInfo = new FileInfo(destFileName)
                {
                    LastWriteTime = when,
                    CreationTime = when
                };
            }
        }
    }
}

Console.WriteLine("Total messages with attachments: " + executeQuery.Count);

static SQLiteConnection OpenSignalDatabase(string databasePath, string passphrase)
{
    SQLitePCL.Batteries_V2.Init();


    byte[] key = Convert.FromHexString(passphrase);


    // Create a new SQLiteConnection with the key
    SQLiteConnectionString sqLiteConnectionString = new SQLiteConnectionString(databasePath, SQLiteOpenFlags.ReadOnly, false, key: key);
    SQLiteConnection connection = new SQLiteConnection(sqLiteConnectionString);

    return connection;
}

public class RootObject
{
    public long timestamp { get; set; }
    public Attachments[] attachments { get; set; }
    public string source { get; set; }
    public int sourceDevice { get; set; }
    public long sent_at { get; set; }
    public long received_at { get; set; }
    public string conversationId { get; set; }
    public bool unidentifiedDeliveryReceived { get; set; }
    public string type { get; set; }
    public int schemaVersion { get; set; }
    public string id { get; set; }
    public string body { get; set; }
    public object[] contact { get; set; }
    public long decrypted_at { get; set; }
    public object[] errors { get; set; }
    public int flags { get; set; }
    public int hasAttachments { get; set; }
    public int hasVisualMediaAttachments { get; set; }
    public bool isViewOnce { get; set; }
    public object[] preview { get; set; }
    public int requiredProtocolVersion { get; set; }
    public int supportedVersionAtReceive { get; set; }
    public object quote { get; set; }
    public object sticker { get; set; }
    public int readStatus { get; set; }
    public int seenStatus { get; set; }
}

public class Attachments
{
    public object caption { get; set; }
    public string contentType { get; set; }
    public string? fileName { get; set; }
    public object flags { get; set; }
    public int? height { get; set; }
    public string id { get; set; }
    public int size { get; set; }
    public int? width { get; set; }
    public string path { get; set; }
    public Thumbnail thumbnail { get; set; }
}

public class Thumbnail
{
    public string path { get; set; }
    public string contentType { get; set; }
    public int width { get; set; }
    public int height { get; set; }
}


class Message
{
    string id { get; set; }
    public string json { get; set; }
}

public class SignalConfig
{
    [JsonPropertyName("key")] public string? Key { get; set; }
}