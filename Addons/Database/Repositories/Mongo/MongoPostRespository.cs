using MongoDB.Driver;
using Noppes.E621;

public class MongoPostRepository : IPostRepository
{
    private readonly IMongoCollection<PostEntry> _collection;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbClient">
    /// The database client connection, passing a single connection to several repositories helps to streamline the process
    /// </param>
    public MongoPostRepository(MongoClient dbClient)
    {
        var database = dbClient.GetDatabase("Index");
        var colNames = database.ListCollectionNames().ToList();
        if(colNames.Contains("Posts"))
        {
            _collection = database.GetCollection<PostEntry>("Posts");
        }
        else
        {
            Console.WriteLine("Post Entry Collection does not exists, creating collection");
            database.CreateCollection("Posts");
            Console.WriteLine("Done! Assigning Collection!");
            _collection = database.GetCollection<PostEntry>("Posts");
            Console.WriteLine("Done!");
        }
    }

    //TODO
    //Follow CRUD'S Architecture

    //Read
    public async Task<Post?> GetPostByIDAsync(int ID)
    {
        var Entry = await _collection.Find(x=> x.Post.Id == ID).FirstOrDefaultAsync();
        if(Entry != null)
        {
            return Entry.Post;
        }
        return null;
    }

    //Create
    public async Task<bool> SavePostAsync(Post post, string filePath)
    {
        /*
        var pid = post.Id;
        var result = await GetPostByIDAsync(pid);

        if(result == null)
        {
            PostEntry entry = new PostEntry
            {
                PostPath = filePath,
                Post = post
            };

            await _collection.InsertOneAsync(entry);
            return true;
        }
        */
        try
        {
            PostEntry entry = new PostEntry
            {
                PostPath = filePath,
                Post = post
            };
            await _collection.InsertOneAsync(entry);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] - {ex.ToString()}");
        }
        return false;
    }

    //Update
    public async Task<bool> UpdatePost(int ID, Post newPost)
    {
        throw new NotImplementedException();
    }

    //Delete
    public async Task<bool> RemovePost(string ID)
    {
        throw new NotImplementedException();
    }
}