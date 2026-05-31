using MongoDB.Driver;
using Noppes.E621;

public class MongoPoolRepository : IPoolRepository
{
    private readonly IMongoCollection<PoolEntry> _collection;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbClient">
    /// The database client connection, passing a single connection to several repositories helps to streamline the process
    /// </param>
    public MongoPoolRepository(MongoClient dbClient)
    {
        var database = dbClient.GetDatabase("Index");
        var colNames = database.ListCollectionNames().ToList();
        if(colNames.Contains("Pools"))
        {
            _collection = database.GetCollection<PoolEntry>("Pools");
        }
        else
        {
            Console.WriteLine("Pool Entry Collection does not exists, creating collection");
            database.CreateCollection("Pools");
            Console.WriteLine("Done! Assigning Collection!");
            _collection = database.GetCollection<PoolEntry>("Pools");
            Console.WriteLine("Done!");
        }
    }

    public async Task<Pool?> GetPoolByIDAsync(string ID)
    {
        var Entry = await _collection.Find(p => p.pool.Id.ToString() == ID).FirstOrDefaultAsync();
        if(Entry != null)
        {
            return Entry.pool;
        }
        return null;
    }

    public async Task<Pool?> GetPoolByIDAsync(int ID)
    {
        var Entry = await _collection.Find(p => p.pool.Id == ID).FirstOrDefaultAsync();
        if(Entry != null)
        {
            return Entry.pool;
        }
        return null;
    }

    public async Task<bool> SavePoolAsync(Pool pool)
    {
        var pid = pool.Id;
        var result = await GetPoolByIDAsync(pid.ToString());

        if(result == null)
        {
            PoolEntry entry = new PoolEntry
            {
                pool = pool
            };

            await _collection.InsertOneAsync(entry);
            return true;
        }
        return false;
    }
}