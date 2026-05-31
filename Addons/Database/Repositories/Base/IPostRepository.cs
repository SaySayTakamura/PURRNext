using Noppes.E621;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SQLite;
using Newtonsoft.Json;

// Defines the database entry for a POST
// MongoDB
public class PostEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ID {get; set;}
    public string? PostPath {get; set;}
    public Post? Post {get; set; }

    //No need
    // Mongo DB returns the whole object (PostEntry) as null if it isn't found in the db
    //The post doesn't need to be null
    /*
    public PostEntry()
    {
        post = null;
    }
    */
}
//Defines the database entry for a POST
//SQLite
public class SQLPostEntry
{
    [PrimaryKey, AutoIncrement]
    public int EID {get; set;}

    [Indexed]
    public string? PostData {get; set;}
    public string? PostHash {get; set;}
    public string? PostPath {get; set;}
    public int? PostID {get; set;}

    public SQLPostEntry()
    {
        PostData = null;
        PostHash = null;
        PostID = null;
        PostPath = null;
    }

    public Post? Reconstruct()
    {
        if(PostData != null)
        {
            return JsonConvert.DeserializeObject<Post>(PostData);
        }
        return null;
    }
}
//Recommended by Chat-GPT
//Study Material from: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface
public interface IPostRepository
{
    Task<Post?> GetPostByIDAsync(string ID);
    Task<Post?> GetPostByIDAsync(int ID);
    Task<bool> SavePostAsync(Post post, string filePath = "");
}