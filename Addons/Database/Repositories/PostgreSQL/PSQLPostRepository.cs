
using Marten;
using Noppes.E621;

public class PSQLPostRepository : IPostRepository
{

    private readonly IDocumentStore _store;

    //Connection string follows this format:
    //Format: var store = DocumentStore.For("host=localhost;database=marten_testing;password=mypassword;username=someuser");
    //Taken from: https://github.com/JasperFx/marten/blob/master/docs/getting-started.md
    public PSQLPostRepository(string connectionString)
    {
        _store = DocumentStore.For(opts =>
        {
            opts.Connection(connectionString);
        });
    }

    //Read
    public async Task<Post?> GetPostByIDAsync(int ID)
    {
        await using var session = _store.LightweightSession();
        var entry = await session.Query<PostEntry>().FirstOrDefaultAsync(x => x.Post.Id == ID);
        if(entry != null)
        {
            return entry.Post;
        }
        return null;

    }
    //Create
    public async Task<bool> SavePostAsync(Post post, string filePath)
    {
        try
        {
            await using var session = _store.LightweightSession();
            PostEntry entry = new PostEntry
            {
                PostPath = filePath,
                Post = post
            };
            session.Store(entry);
            await session.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
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
