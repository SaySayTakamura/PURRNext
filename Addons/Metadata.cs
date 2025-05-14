using Newtonsoft.Json;
using Noppes.E621;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PURRNext.PostMD
{
    public class PoolData
    {
        public int ID;
        public string Name;
        public string Description;
        public int PostCount;
        public string Path;
    }

    public class Metadata
    {
        public int ID;
        public int? UploaderID;
        public int? ApproverID;
        public int CommentCount;
        public int FavoriteCount;
        public string Extension;
        public PostFlags Flags;
        public Relationships Relationships;
        public DateTimeOffset CreatedAt;
        public DateTimeOffset? UpdatedAt;
        public string? Description;
        public PostRating? Rating;
        public TagCollection Tags;
        public List<string> Sources;
        public Score Score;
        public List<PoolData> Pools;
        public string PostPath;
        public string IsRefferFile;

        public Metadata() 
        { 
            Sources = new List<string>();
            Pools = new List<PoolData>();
        }
    };

    public class MetadataManager
    {
        public List<Metadata> MDL;
        private string Path;
        public void WithPath(string path)
        {
            Path = path;
            using (FileStream fs = File.Create(Path))
            {
                byte[] info = new UTF8Encoding(true).GetBytes($"[\n]");
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
                fs.Close(); //Remove in case of regret
            }
        }
        public MetadataManager() { MDL = new List<Metadata>(); }
        public void ExportMetadata()
        {
            try
            {
                //open file stream
                using (StreamWriter file = File.CreateText(Path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, MDL);
                    file.Close();//Remove in case of regret
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}
