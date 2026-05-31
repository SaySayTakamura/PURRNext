using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Noppes.E621;
using SQLite;

// Defines the database entry for a POOL
// MongoDB
public class PoolEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _ID { get; set; }

    public Pool? pool { get; set; }
}
// Defines the database entry for a POOL
// SQLite
public class SQLPoolEntry
{
    [PrimaryKey, AutoIncrement]
    public int EID {get; set;}

    [Indexed]
    public string? PoolData {get; set;}
    public string? PoolHash {get; set;}
    public int? PoolID {get; set;}

//Redundant type? values are null
/*
    public SQLPoolEntry()
    {
        PoolData = null;
        PoolHash = null;
        PoolID = null;
    }
*/

    public Pool? Reconstruct()
    {
        if(PoolData != null)
        {
            return JsonConvert.DeserializeObject<Pool>(PoolData);
        }
        return null;
    }

}

public interface  IPoolRepository
{
    Task<Pool?> GetPoolByIDAsync(string ID);
    Task<Pool?> GetPoolByIDAsync(int ID);
    Task<bool> SavePoolAsync(Pool pool);
}