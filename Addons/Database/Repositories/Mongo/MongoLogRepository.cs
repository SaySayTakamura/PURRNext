using MongoDB.Driver;
using PURRNext.LOG;

public class MongoLogRepository : ILogRepository
{
    private readonly IMongoCollection<LogEntry> _collection;
    public MongoLogRepository(MongoClient dbClient)
    {
        var database = dbClient.GetDatabase("Data");
        var colNames = database.ListCollectionNames().ToList();
        if(colNames.Contains("Logs"))
        {
            _collection = database.GetCollection<LogEntry>("Logs");
        }
        else
        {
            Console.WriteLine("Log Entry Collection does not exists, creating collection");
            database.CreateCollection("Logs");
            Console.WriteLine("Done! Assigning Collection!");
            _collection = database.GetCollection<LogEntry>("Logs");
            Console.WriteLine("Done!");
        }
    }

    public async Task LogToDatabase(EntryForm form)
    {
        try
        {
            LogEntry entry = new()
            {
                Log = form
            };
            await _collection.InsertOneAsync(entry);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error has occured while sending the log to the Database\nException: {e}");
            throw new Exception(e.Message);
        }

    }
    public Task<List<EntryForm>> GetLogs(bool success)
    {
        throw new NotImplementedException();
    }

    public Task<List<EntryForm>> GetLogs(string date)
    {
        throw new NotImplementedException();
    }

    public Task<List<EntryForm>> GetLogs(Predicate<EntryForm> deleg)
    {
        throw new NotImplementedException();
    }

}