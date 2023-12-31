using SQLite;

namespace manuc66.SignalMediaExporter.Core;

public static class DatabaseConnectionProvider
{
    public static SQLiteConnection OpenSignalDatabase(string databasePath, string passphrase)
    {
        SQLitePCL.Batteries_V2.Init();
        byte[] key = Convert.FromHexString(passphrase);

        SQLiteConnectionString sqLiteConnectionString = new(databasePath, SQLiteOpenFlags.ReadOnly, false, key: key);
        SQLiteConnection connection = new(sqLiteConnectionString);
        connection.Trace = true;
        return connection;
    }
}