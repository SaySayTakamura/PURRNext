
using Newtonsoft.Json;

namespace PURRNext.Configs
{
    public class DockerCofigs
    {
        //Amount of idle time before the next run of the application
        public int ActivityInterval = 5;

        //Limit of tags to fetch from the main file at time
        //Like this even on a big file we won't overload e621 servers
        public int SearchLimit = 4;

    }
    public class Configuration
    {
        //Configuration file scheme version
        public string Version {get; set;} = "1.9.6";
        //Store videos in a separate folder
        public bool VideoOnFolders {get; set;} = true;
        //Store SWF files in a separate folder
        public bool FlashOnFolders {get; set;} = true;
        //Skips login prompt and pull credentials from storage file
        public bool AutoLogin {get; set;}= false;
        //Max Posts to fetch per page on a single call
        public int MaxPostsPerPage {get; set;} = 75;
        //The amount of posts to fetch in a single Async call. (FetchPostsAsync)
        public int MaxPostsPerCall {get; set;} = 320;
        //Start a search with a list of "searches" inside a file.
        public bool TagsFromFile {get; set;} = false;
        //Which database to be used with the app, MONGO or SQL
        public string DatabaseDriver {get; set;} = "SQL";
        public string DatabasePath {get; set;} = "./db.sqlite";
        
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
                Configuration c = cfg;

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