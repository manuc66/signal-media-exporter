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

string configDirectoryPath = Path.GetDirectoryName(Path.GetDirectoryName(dbLocationPath)) ??
                        throw new Exception("Impossible to traverse file directory to find the location of the Signal config file");
string signalConfigFilePath = Path.Combine(configDirectoryPath, "config.json");

if (!File.Exists(signalConfigFilePath))
{
    throw new NotSupportedException($"Signal configuration file not found: {signalConfigFilePath}");
}

Console.WriteLine("Database location: " +  dbLocationPath);
Console.WriteLine("Configuration location: " +  configDirectoryPath);


string jsonData = File.ReadAllText(signalConfigFilePath);

// Deserialize the JSON data into the MyData class
SignalConfig? data = JsonSerializer.Deserialize<SignalConfig>(jsonData);


Console.WriteLine("Key: " + data?.Key);
using SQLiteConnection connection = OpenSignalDatabase(dbLocationPath, data.Key);

SQLiteCommand sqLiteCommand = connection.CreateCommand("select id, json from messages where hasAttachments=1");
List<Message> executeQuery = sqLiteCommand.ExecuteQuery<Message>();

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

class Message {
    private string id { get; set; }
    private string json { get; set; }
}

public class SignalConfig
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}