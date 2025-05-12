
using Newtonsoft.Json;

namespace PURRNext.Configs
{
    public class Configuration
    {
        public string Version = "1.0.0";
        public bool VideoOnFolders = false;
        //Max Posts to fetch per page on a single call
        public int MaxPostsPerPage = 75;

        //The amount of posts to fetch in a single Async call. (FetchPostsAsync)
        public int MaxPostsPerCall = 320;
        public bool TagsFromFile = false;
    }
    public class ConfigurationWriter
    {
        //Path to the configuration file
        public string config_path;

        public Configuration LoadConfigFile()
        {
            Configuration load = new Configuration();

            using (StreamReader r = new StreamReader(config_path))
            {
                string json = r.ReadToEnd();
                Configuration entry = JsonConvert.DeserializeObject<Configuration>(json);
                load = entry;
                r.Close();//Remove in case of regret
            }
            return load;
        }
        public void SaveConfigFile(Configuration cfg)
        {
            if(cfg != null)
            {
                Configuration c = new Configuration();
                c.Version = cfg.Version;
                c.VideoOnFolders = cfg.VideoOnFolders;

                using (StreamWriter file = File.CreateText(config_path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, c);
                    file.Close();//Remove in case of regret
                }

            }

        }
    }
}