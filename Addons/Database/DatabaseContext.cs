
using MongoDB.Driver;
using Noppes.E621;
using PURRNext.LOG;
using SQLite;

public class DatabaseContext
{
    private readonly IPostRepository postRepository;
    private readonly IPoolRepository poolRepository;
    private readonly ILogRepository logRepository;
    private readonly string dbDriver;

    /// <summary>
    /// Creates a Database Context for a specific database.
    /// </summary>
    /// <param name="driver"> Choose between MONGO or SQL, mongodb is more recommended for Docker deploys of PURR </param>
    /// <param name="dbPath"> The database URL/Path, use a URL to connect to deployable/external databases (Such as MongoDB) or a Path for local databases (such as SQLite</param>
    public DatabaseContext(string driver, string dbPath = "")
    {
        var driv = driver.Trim().ToLower();
        if(driv == "mongo")
        {
            dbDriver = "mongo";
            //Creates a single connection to the database
            //Instead of creating one connection to each repository
            //Creates a single connection that several repositories can use
            //var mongo_client = new MongoClient("mongodb://repoUser:996854@mongodb:4547");
            var mongo_client = new MongoClient(dbPath);
            postRepository = new MongoPostRepository(mongo_client);
            poolRepository = new MongoPoolRepository(mongo_client);
            logRepository = new MongoLogRepository(mongo_client);
        }
        else if(driv == "sql")
        {
            dbDriver = "sqlite";
            var connectionString = new SQLiteConnectionString(dbPath, false, key: "996854");
            var connection = new SQLiteAsyncConnection(connectionString);
            postRepository = new SQLPostRepository(connection);
            poolRepository = new SQLPoolRepository(connection);
            logRepository = new SQLLogRepository(connection);
        }
        else
        {
            Console.WriteLine("Invalid Driver Selection");
            Environment.Exit(0);
            Console.ReadLine();
        }
    }
#region POSTS
    public async Task<bool> PostExists(string id)
    {
        var postData = await postRepository.GetPostByIDAsync(id);
        if(postData != null)
        {
            return true;
        }
        return false;
    }
    public async Task<bool> PostExists(int id)
    {
        var postData = await postRepository.GetPostByIDAsync(id);
        if(postData != null)
        {
            return true;
        }
        return false;
    }

    public async Task<bool> AddPost(Post post, string path)
    {
        return await postRepository.SavePostAsync(post, path);
    }
#endregion

#region UTILS
    public async Task Log(EntryForm form)
    {
        await logRepository.LogToDatabase(form);
    }
#endregion
}