// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;

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

if (!File.Exists(dbLocationPath))
{
    throw new NotSupportedException($"Signal configuration file not found: {signalConfigFilePath}");
}

Console.WriteLine("Database location: " +  dbLocationPath);
Console.WriteLine("Configuration location: " +  dbLocationPath);