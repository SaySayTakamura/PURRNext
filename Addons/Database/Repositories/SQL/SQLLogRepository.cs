
using Newtonsoft.Json;
using PURRNext.LOG;
using SQLite;

public class SQLLogRepository : ILogRepository
{
    private readonly SQLiteAsyncConnection _db;
    public SQLLogRepository(SQLiteAsyncConnection dbConnection)
    {
        _db = dbConnection;
        SetupTables();
    }

    public void SetupTables()
    {
        //Move this to the Pools Repository
        var logResult = _db.CreateTableAsync<SQLLogEntry>();
        logResult.Wait();
        if(logResult.Result == CreateTableResult.Created)
        {
            Console.WriteLine("SQL Logs Table was created!");
        }
        else
        {
            Console.WriteLine("SQL Logs Table has been migrated!");
        }
    }

    public async Task LogToDatabase(EntryForm form)
    {

        var logData = JsonConvert.SerializeObject(form);

        try
        {
            SQLLogEntry entry = new()
            {
                LogData = logData
            };
            await _db.InsertAsync(entry);
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