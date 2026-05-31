using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PURRNext.LOG;
using SQLite;

public class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _ID {get; set;}
    public EntryForm? Log {get; set;}
}

public class SQLLogEntry
{
    [PrimaryKey, AutoIncrement]
    public int EID {get; set;}

    [Indexed]
    public string? LogData {get; set;}

    public EntryForm? Reconstruct()
    {
        if(LogData != null)
        {
            return JsonConvert.DeserializeObject<EntryForm>(LogData);
        }
        return null;
    }
}

public interface ILogRepository
{
    Task LogToDatabase(EntryForm form);
    Task<List<EntryForm>> GetLogs(bool success);
    Task<List<EntryForm>> GetLogs(string date);
    Task<List<EntryForm>> GetLogs(Predicate<EntryForm> deleg);
}