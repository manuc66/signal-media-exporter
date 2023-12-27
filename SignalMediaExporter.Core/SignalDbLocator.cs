using System.Runtime.InteropServices;

namespace manuc66.SignalMediaExporter.Core;

public static class SignalDbLocator
{
    public static (string signalDirectory, string dbLocationPath) GetDbLocationDetails()
    {
        string dbLocationPath0 = GetDbLocationPath();

        string signalDirectory0 = Path.GetDirectoryName(Path.GetDirectoryName(dbLocationPath0)) ??
                                  throw new Exception("Impossible to traverse file directory to find the logger of the Signal config file");
        return (signalDirectory0, dbLocationPath0);
    }

    private static string GetDbLocationPath()
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