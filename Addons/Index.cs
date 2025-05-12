using Newtonsoft.Json;
using PURRNext.LOG;

namespace PURRNext.Index
{
    public class IndexEntry
    {
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Path { get; set; }
    }
    public class Index
    {
        public List<IndexEntry> Entries;
        public Index() { Entries = new List<IndexEntry>(); Entries = ImportDefault(); }

        public void AddEntry(string tag, DateTime createdAt,DateTime lastUpdated, string path)
        { 
            Entries.Add(new IndexEntry { Tag = tag, CreatedAt = createdAt, LastUpdated = lastUpdated, Path = path });
        }
        public void AddEntry(IndexEntry entry)
        {  
            Entries.Add(entry); 
        }
        public void DeleteEntry(string tag)
        {
            bool deleted = false;
            for (int i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].Tag == tag)
                {
                    Console.WriteLine("Entry found, Deleting");
                    Entries.RemoveAt(i);
                }
            }
            if(deleted == true)
            {
                Console.WriteLine("Entry deleted!");
            }
            else
            {
                Console.WriteLine("Entry not found!");
            }
        }
        public void Export()
        {

        }
        public List<IndexEntry> ImportDefault()
        {
            List<IndexEntry> result = new List<IndexEntry>();

            using (StreamReader r = new StreamReader("index.json"))
            {
                string json = r.ReadToEnd();
                List<IndexEntry> items = JsonConvert.DeserializeObject<List<IndexEntry>>(json);
                result = items;
                r.Close();//Remove in case of regret
            }

            return result;
        }
    }
}
