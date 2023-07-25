using manuc66.SignalMediaExporter.CLI.Models;
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
}