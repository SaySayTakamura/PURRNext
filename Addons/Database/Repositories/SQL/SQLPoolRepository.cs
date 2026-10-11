
using System.Security.Cryptography;
using System.Text;
using Noppes.E621;
using SQLite;

public class SQLPoolRepository : IPoolRepository
{
    private readonly SQLiteAsyncConnection _db;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConnection"></param>
    public SQLPoolRepository(SQLiteAsyncConnection dbConnection)
    {
        _db = dbConnection;
        SetupTables();
    }
//Database Utils
#region 
    public void SetupTables()
    {
        //Move this to the Pools Repository
        var poolResult = _db.CreateTableAsync<SQLPoolEntry>();
        poolResult.Wait();
        if(poolResult.Result == CreateTableResult.Created)
        {
            Console.WriteLine("SQL Pools Table was created!");
        }
        else
        {
            Console.WriteLine("SQL Pools Table has been migrated!");
        }
    }
#endregion

    //TODO
    //Follow CRUD'S Architecture

    //Read
    public async Task<Pool?> GetPoolByIDAsync(int id)
    {
        var postData = await _db.Table<SQLPoolEntry>().Where(x => x.PoolID == id).FirstOrDefaultAsync();
        return postData.Reconstruct();
    }
    
    //Create
    public async Task<bool> SavePoolAsync(Pool pool)
    {
        if(pool != null)
        {
            //Sets options for the serializers
            System.Text.Json.JsonSerializerOptions opts = new()
            {
                IncludeFields = true
            };

            var poolData = System.Text.Json.JsonSerializer.Serialize(pool, options: opts);
            var HashEngine = SHA256.Create();
            var poolHash = Convert.ToHexString(HashEngine.ComputeHash(Encoding.UTF8.GetBytes(poolData)));

            SQLPoolEntry entry = new ()
            {
                PoolData = poolData,
                PoolHash = poolHash,
                PoolID = pool.Id
            };
            await _db.InsertAsync(entry);
            return true;
        }
        return false;
    }

    //TODO: Update

    //TODO: Delete
}