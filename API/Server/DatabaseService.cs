using APIServer.Models;
using MongoDB.Driver;

namespace APIServer.Services
{
    //PURR DATABASE SERVICES (PDBS)

    public class DatabaseService
    {
        /*
            We will retrieve things for each section such as:

            Index Entry
            Archive Entry
            Log Entry

            And so on.
        */
        private readonly IMongoCollection<IndexEntry> _EntryCollection;

        public DatabaseService()
        {
            Console.WriteLine("Initializing MONGODB Server Connection");

            var mongoClient = new MongoClient("mongodb://client:secret@mongo:4547");

            var mongoDatabase = mongoClient.GetDatabase("Index");

            var Existence = mongoDatabase.ListCollectionNames().ToList().Contains("Entries");
            if (Existence == true)
            {
                Console.WriteLine("Entry Collections exists, proceeding");
                _EntryCollection = mongoDatabase.GetCollection<IndexEntry>("Entries");
            }
            else
            {
                Console.WriteLine("Entry Collections does not exists, creating collection");
                mongoDatabase.CreateCollectionAsync("Entries").Wait();
                Console.WriteLine("Done! Assigning Collection!");
                _EntryCollection = mongoDatabase.GetCollection<IndexEntry>("Entries");
                Console.WriteLine("Done!");

            }
        }
        public async Task<List<IndexEntry>> GetAsync()
        {
            return await _EntryCollection.Find(_ => true).ToListAsync();
        }
        public async Task<IndexEntry?> GetAsync(string id)
        {
            return await _EntryCollection.Find(x => x.ID == id).FirstOrDefaultAsync();
        }
        public async Task CreateAsync(IndexEntry entry)
        {
            await _EntryCollection.InsertOneAsync(entry);
        }
        public async Task UpdateAsync(string id, IndexEntry new_entry)
        {
            await _EntryCollection.ReplaceOneAsync(x => x.ID == id, new_entry);
        }
        public async Task DeleteAsync(string id)
        {
            await _EntryCollection.DeleteOneAsync(x => x.ID == id);
        }
    }
}