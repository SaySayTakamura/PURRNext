
using System.Security.Cryptography;
using System.Text;
using Noppes.E621;
using SQLite;

public class SQLPostRepository : IPostRepository
{
    private readonly SQLiteAsyncConnection _db;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbConnection"></param>
    public SQLPostRepository(SQLiteAsyncConnection dbConnection)
    {
        _db = dbConnection;
        SetupTables();
    }
//Database Utils
#region 
    public void SetupTables()
    {
        var postResult = _db.CreateTableAsync<SQLPostEntry>();
        postResult.Wait();
        if(postResult.Result == CreateTableResult.Created)
        {
            Console.WriteLine("SQL Posts Table was created!");
        }
        else
        {
            Console.WriteLine("SQL Posts Table has been migrated!");
        }
    }
#endregion

//CRUD Operations
#region 
    public async Task<Post?> GetPostByIDAsync(string id)
    {
        var postData = await _db.Table<SQLPostEntry>().Where(x => x.PostID.ToString() == id).FirstOrDefaultAsync();
        if(postData != null)
        {
            return postData.Reconstruct();
        }
        return null;
    }
    public async Task<Post?> GetPostByIDAsync(int id)
    {
        var postData = await _db.Table<SQLPostEntry>().Where(x => x.PostID == id).FirstOrDefaultAsync();
        if(postData != null)
        {
            return postData.Reconstruct();
        }
        return null;
    }

    public async Task<bool> SavePostAsync(Post post, string filePath)
    {
        if(post != null)
        {
            //Sets options for the serializers
            System.Text.Json.JsonSerializerOptions opts = new()
            {
                IncludeFields = true
            };

            var postData = System.Text.Json.JsonSerializer.Serialize(post, options: opts);
            var HashEngine = SHA256.Create();
            var postHash = Convert.ToHexString(HashEngine.ComputeHash(Encoding.UTF8.GetBytes(postData)));

            SQLPostEntry entry = new ()
            {
                PostData = postData,
                PostHash = postHash,
                PostPath = filePath,
                PostID = post.Id
            };
            await _db.InsertAsync(entry);
            return true;
        }
        return false;
    }
#endregion
}