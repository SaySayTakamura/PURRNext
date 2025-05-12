using Noppes.E621;
using System.Text;
using PURRNext.Configs;
using PURRNext.TUpdater;
using PURRNext.LOG;
using Newtonsoft.Json;
using PURRNext.TagFile;

namespace PURRNext
{
    internal class Program
    {
        //Global Logger Instance
        static Logger global_logger = new Logger();
        static Configuration global_config = new Configuration();

        //Directories variables
        static string WorkDir = "";
        static string SessionDir = "";
        static string SessionOutput = "";
        static string LogFile = "";
        static string UpdatesFile = "";
        static string ConfigFile = "";
        static string BlacklistFile = "";

        //Extra directories for the Docker implementation

        static string DataDir = "";
        static string ContentDir = "";
        static string TagsFile = "";

        static bool LoadedConfigFile = false;

        //True if logged onto E621
        static bool Logged = false;

        //Global blacklist, loaded from file
        static List<string> GlobalBlacklist = new List<string>();

        //Loads blacklist file
        static List<string> LoadBlackListText()
        {
            var result = new List<string>();
            var lines = File.ReadAllLines(BlacklistFile);
            for(int i = 0; i < lines.Count(); i++)
            {
                result.Add(lines[i]);
            }
            return result;
        }
        static string ComposeBlacklistString(List<string> tags)
        {
            StringBuilder builder = new StringBuilder();

            var result = String.Empty;
            for(var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                var modified_tag = $"-{tag}";
                Console.WriteLine($"Result Tag - {modified_tag}");

                builder.Append($"{modified_tag} ");
            }
            var s = builder.ToString();
            if(s.ElementAt(s.Length-1) == ' ')
            {
                Console.WriteLine("Removing blank space");
                s = s.Remove(s.Length-1);
                result = s;
            }
            else
            {
                Console.WriteLine("No blank space found.... weird");
                result = s;
            }
            return result;
        }

        //Further tests required
        //Use relative directory for Docker volumes instead of Environment.CurrentDirectory
        //Until proven itself useful
        static void SetupDirectories()
        {
            Console.WriteLine("Setting up directories\n");
            var working_directory = Environment.CurrentDirectory;
            var date = DateTime.Now;
            var session_dir = $"{working_directory}/Sessions";
            var session_output = $"{session_dir}/Session - {date.ToString("dd-MM-yyyy")}";
            var log_file = $"{session_dir}/log.json";
            var updates_file = $"{working_directory}/updates.json";
            var config_file = $"{working_directory}/config.json";
            var blacklist_file = $"{working_directory}/blacklist.txt";

            Console.WriteLine($"Working Directory - {working_directory}\n");
            WorkDir = working_directory;

            //Sets up the SESSIONS directory to store each day's session
            if(!Directory.Exists(session_dir))
            {
                Console.WriteLine("No Sessions Directory, creating one....");

                Directory.CreateDirectory(session_dir);
                SessionDir = session_dir;

                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Sessions Directory exists, no need for a new one!");
                if(SessionDir == "")
                {
                    SessionDir = session_dir;
                }
                else
                {
                    Console.WriteLine("No need to overwrite SessionDir variable");
                }
            }
            //Sets up the SESSION XX-YY-ZZ directory for the current day
            if(!Directory.Exists(session_output))
            {
                Console.WriteLine("No Session Output Directory, creating one....");
                
                Directory.CreateDirectory(session_output);
                SessionOutput = session_output;

                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Session Output Directory exists, no need for a new one!");
                if(SessionOutput == "")
                {
                    SessionOutput = session_output;
                }
                else
                {
                    Console.WriteLine("No need to overwrite SessionOutput variable");
                }
            }
            if(!File.Exists(blacklist_file))
            {
                Console.WriteLine("Blacklist file does not exist, creating one!");
                using (FileStream fs = File.Create(blacklist_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                BlacklistFile = blacklist_file;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Blacklist file exist, great!");
                if(BlacklistFile == "")
                {
                    BlacklistFile = blacklist_file;
                }
                else
                {
                    Console.WriteLine("No need to overwrite UpdatesFile variable");
                }
            }
            //Creates a Updates.JSON file if one doesn't exists.
            if(!File.Exists(updates_file))
            {
                Console.WriteLine("Updates file does not exist, creating one!");
                using (FileStream fs = File.Create(updates_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"[\n]");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                UpdatesFile = updates_file;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Updates file exist, great!");
                if(UpdatesFile == "")
                {
                    UpdatesFile = updates_file;
                }
                else
                {
                    Console.WriteLine("No need to overwrite UpdatesFile variable");
                }
            }
            //Creates a Log.JSON file if one doesn't exists.
            if(!File.Exists(log_file))
            {
                Console.WriteLine("Log file does not exist, creating one!");
                using (FileStream fs = File.Create(log_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"[\n]");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                LogFile = log_file;
                global_logger.WithLogPath(log_file);
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Log file exist, holy molly!");
                if(LogFile == "")
                {
                    LogFile = log_file;
                    global_logger.WithLogPath(log_file);
                }
                else
                {
                    Console.WriteLine("No need to overwrite LogFile variable");
                }
            }
            //Set up a Configuration Writer with a default Configuration file
            //If the config file does not exist, creates one
            if(!File.Exists(config_file))
            {
                //Declares a default configuration file and writer
                Configuration cfg = new Configuration();
                ConfigurationWriter c_wrtr = new ConfigurationWriter();

                Console.WriteLine("Configuration file does not exist, creating one!");
                c_wrtr.config_path = config_file;
                c_wrtr.SaveConfigFile(cfg);

                ConfigFile = config_file;

                global_config = cfg;
                LoadedConfigFile = true;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Configuration file exist, heavens!");
                var writer = new ConfigurationWriter();
                if(ConfigFile == "")
                {
                    ConfigFile = config_file;
                    if(LoadedConfigFile == false)
                    {
                        Console.WriteLine("Loading configuration file!\n");

                        //Loads the config file
                        writer.config_path = config_file;
                        global_config = writer.LoadConfigFile();

                        //Display the values of the settings file
                        Console.WriteLine($"- Configuration File Version - {global_config.Version}");
                        Console.WriteLine($"- Save videos on their own folder - {global_config.VideoOnFolders}");
                        Console.WriteLine($"- Maximum posts to retrieve by API Call - {global_config.MaxPostsPerCall}");
                        Console.WriteLine($"- Maximum posts to retrieve by Pagination - {global_config.MaxPostsPerPage}");
                        Console.WriteLine($"- Last modified at - {File.GetLastWriteTime(ConfigFile).ToString("G")}\n");

                        LoadedConfigFile = true;
                    }
                }
                else
                {
                    //Should never happen but only to be safe
                    Console.WriteLine("No need to overwrite ConfigFile variable");
                }
                
            }
        }
        static void SetupDockerDirectories()
        {
            Console.WriteLine("Setting up docker directories set\n");
            var working_directory = Environment.CurrentDirectory;
            var date = DateTime.Now;

            //Content Folders
            var content_dir = $"{working_directory}/Content";
            var session_dir = $"{content_dir}/Sessions";
            var session_output = $"{session_dir}/Session - {date.ToString("dd-MM-yyyy")}";

            //File Paths
            var data_path = $"{working_directory}/Data";
            var log_file = $"{data_path}/log.json";
            var updates_file = $"{data_path}/updates.json";
            var config_file = $"{data_path}/config.json";
            var blacklist_file = $"{data_path}/blacklist.txt";
            var tags_file = $"{data_path}/tags.txt";

            Console.WriteLine($"Working Directory - {working_directory}\n");
            WorkDir = working_directory;
            DataDir = data_path;
            ContentDir = content_dir;

            //Setting up folders
            //Sets up the SESSIONS directory to store each day's session
            if(!Directory.Exists(session_dir))
            {
                Console.WriteLine("No Sessions Directory, creating one....");

                Directory.CreateDirectory(session_dir);
                SessionDir = session_dir;

                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Sessions Directory exists, no need for a new one!");
                if(SessionDir == "")
                {
                    SessionDir = session_dir;
                }
                else
                {
                    Console.WriteLine("No need to overwrite SessionDir variable");
                }
            }

            //Sets up the SESSION XX-YY-ZZ directory for the current day
            if(!Directory.Exists(session_output))
            {
                Console.WriteLine("No Session Output Directory, creating one....");
                
                Directory.CreateDirectory(session_output);
                SessionOutput = session_output;

                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Session Output Directory exists, no need for a new one!");
                if(SessionOutput == "")
                {
                    SessionOutput = session_output;
                }
                else
                {
                    Console.WriteLine("No need to overwrite SessionOutput variable");
                }
            }

            //Setting up files
            //Creates a Blacklist.txt file if one doesn't exists.
            if(!File.Exists(blacklist_file))
            {
                Console.WriteLine("Blacklist file does not exist, creating one!");
                using (FileStream fs = File.Create(blacklist_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                BlacklistFile = blacklist_file;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Blacklist file exist, great!");
                if(BlacklistFile == "")
                {
                    BlacklistFile = blacklist_file;
                }
                else
                {
                    Console.WriteLine("No need to overwrite UpdatesFile variable");
                }
            }
        
            //Creates a Tags.txt file if one doesn't exists.
            if(!File.Exists(tags_file))
            {
                Console.WriteLine("Tags file does not exist, creating one!");
                using (FileStream fs = File.Create(tags_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                TagsFile = tags_file;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Tags file exist, great!");
                if(TagsFile == "")
                {
                    TagsFile = tags_file;
                }
                else
                {
                    Console.WriteLine("No need to overwrite TagsFile variable");
                }
            }

            //Creates a Updates.JSON file if one doesn't exists.
            if(!File.Exists(updates_file))
            {
                Console.WriteLine("Updates file does not exist, creating one!");
                using (FileStream fs = File.Create(updates_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"[\n]");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                UpdatesFile = updates_file;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Updates file exist, great!");
                if(UpdatesFile == "")
                {
                    UpdatesFile = updates_file;
                }
                else
                {
                    Console.WriteLine("No need to overwrite UpdatesFile variable");
                }
            }
        
            //Creates a Log.JSON file if one doesn't exists.
            if(!File.Exists(log_file))
            {
                Console.WriteLine("Log file does not exist, creating one!");
                using (FileStream fs = File.Create(log_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"[\n]");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                LogFile = log_file;
                global_logger.WithLogPath(log_file);
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Log file exist, holy molly!");
                if(LogFile == "")
                {
                    LogFile = log_file;
                    global_logger.WithLogPath(log_file);
                }
                else
                {
                    Console.WriteLine("No need to overwrite LogFile variable");
                }
            }
        
            //Set up a Configuration Writer with a default Configuration file
            //If the config file does not exist, creates one
            if(!File.Exists(config_file))
            {
                //Declares a default configuration file and writer
                Configuration cfg = new Configuration();
                ConfigurationWriter c_wrtr = new ConfigurationWriter();

                Console.WriteLine("Configuration file does not exist, creating one!");
                c_wrtr.config_path = config_file;
                c_wrtr.SaveConfigFile(cfg);

                ConfigFile = config_file;

                global_config = cfg;
                LoadedConfigFile = true;
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Configuration file exist, heavens!");
                var writer = new ConfigurationWriter();
                if(ConfigFile == "")
                {
                    ConfigFile = config_file;
                    if(LoadedConfigFile == false)
                    {
                        Console.WriteLine("Loading configuration file!\n");

                        //Loads the config file
                        writer.config_path = config_file;
                        global_config = writer.LoadConfigFile();

                        //Display the values of the settings file
                        Console.WriteLine($"- Configuration File Version - {global_config.Version}");
                        Console.WriteLine($"- Save videos on their own folder - {global_config.VideoOnFolders}");
                        Console.WriteLine($"- Maximum posts to retrieve by API Call - {global_config.MaxPostsPerCall}");
                        Console.WriteLine($"- Maximum posts to retrieve by Pagination - {global_config.MaxPostsPerPage}");
                        Console.WriteLine($"- Last modified at - {File.GetLastWriteTime(ConfigFile).ToString("G")}\n");

                        LoadedConfigFile = true;
                    }
                }
                else
                {
                    //Should never happen but only to be safe
                    Console.WriteLine("No need to overwrite ConfigFile variable");
                }
                
            }
        }

        //Print the available topics for the user to choose from
        static void PrintTopics()
        {
            //Intro
            Console.WriteLine("How do you do Fellow Degenerate?");
            Console.WriteLine("What are you searching today? - Pools? Posts? we have it my friend. - Type below which one\n");

            //Main Topics
            
            Console.WriteLine("Here are some of the topics you can choose from:");
            Console.WriteLine("1 - Posts");
            Console.WriteLine(" -   Downloads posts within a tag with a determined limit (which you can set on your config file)");
            Console.WriteLine("2 - Paginated Posts");
            Console.WriteLine(" -   Downloads posts within a tag for the amount of pages you want (which you can set on the next step)");
        }

        static void Main(string[] args)
        {
            start:
            Console.WriteLine("\n--------------------------------------------");
            Console.WriteLine("-------------------------------------------------- --    -\n");

            Console.WriteLine("     PURR NEXT - An E621 CLI BACKEND (WIP)");
            Console.WriteLine("     VERSION - 0.0.1");
            Console.WriteLine("     Author: Edgar Takamura");

            Console.WriteLine("\n-------------------------------------------------- --    -");
            Console.WriteLine("--------------------------------------------\n");     

            Console.WriteLine($"[System] - Available Cores: {Environment.ProcessorCount}");

            //Main Runtime
            var topic = "";
            var mode = (args.Length > 0) ? args[0] : "CLI-APP";

            Console.WriteLine($"[System] - Application is running on the '{mode}' mode\n");


            if (mode == "BATCH")
            {
                SetupDirectories();
                topic = args[1];
            }
            else if (mode == "CLI-APP")
            {
                SetupDirectories();
                PrintTopics();
                topic = Console.ReadLine();
                if(topic == "1")
                {
                    var result_tags = "";
                    if(global_config.TagsFromFile == true)
                    {
                        Console.WriteLine("You've set to use a file for storing your tags, do you want to use it for your search? - [Y/N]");
                        var response = Console.ReadLine();

                        if(response == "Y" || response == "y")
                        {
                            Console.WriteLine("Okie dokie proceeding then");
                            goto limit1;
                        }
                        else if(response == "N" || response == "n")
                        {
                            Console.WriteLine("Okay then!");
                            goto tg1;
                        }
                        else if(response == "YES" || response == "yes")
                        {
                            Console.WriteLine("Okie dokie proceeding then");
                            goto limit1;
                        }
                        else if(response == "NO" || response == "no")
                        {
                            Console.WriteLine("Okay then!");
                            goto tg1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tags from File feature not enable, proceeding");
                    }

                    tg1:
                        Console.WriteLine("Just type your tags below and press enter to begin your search!");
                        var tags = Console.ReadLine();

                        Console.WriteLine("Checking for blank spaces");
                        if(tags.Length != 0)
                        {
                            if(tags.ElementAt(tags.Length-1) == ' ')
                            {
                                Console.WriteLine("Blank space found on string end, removing");
                                tags = tags.Remove(tags.Length-1);
                            }
                            else
                            {
                                Console.WriteLine("No blank space, congrats");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Tags can't be empty, try again");
                            Environment.Exit(0);
                            
                        }
                    
                    Console.WriteLine($"Those are your tags: {tags}");
                    Console.WriteLine("Do you confirm?");
                    var confirm = Console.ReadLine();

                    if(confirm == "Y" || confirm == "y")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                        result_tags = tags;
                    }
                    else if(confirm == "N" || confirm == "n")
                    {
                        Console.WriteLine("Okay then!");
                        goto tg1;
                    }
                    else if(confirm == "YES" || confirm == "yes")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                        result_tags = tags;
                    }
                    else if(confirm == "NO" || confirm == "no")
                    {
                        Console.WriteLine("Okay then!");
                        goto tg1;
                    }

                    limit1:
                    Console.WriteLine("The app will be using a limit for fetching posts!");
                    Console.WriteLine("You can change this limit whenever you want by changing the 'config.json' fike");

                    Console.WriteLine("- STAAARTING -");

                    Console.WriteLine("Composing tags blacklist");
                    var BL = LoadBlackListText();
                    var backup_list_string = "";
                    if(BL.Count != 0)
                    {
                        for(int en = 0; en < BL.Count; en++)
                        {
                            if(result_tags.Contains(BL[en]))
                            {
                                int current = en;
                                Console.WriteLine("Tags conflict with blacklist");
                                Console.WriteLine($"Removing - {BL[current]}");
                                result_tags = result_tags.Replace(BL[current], "");
                            }
                        }
                        
                        //Compose final string with both search tags and blacklist tags
                        var blacklist_string = ComposeBlacklistString(BL);
                        result_tags = $"{result_tags} {blacklist_string}";
                        Console.WriteLine($"Final TAG string - {result_tags}");
                        backup_list_string = blacklist_string;
                    }
                    else
                    {
                        Console.WriteLine("No tags found on the loaded blacklist file");
                    }

                    var e621Client = new E621ClientBuilder()
                    .WithUserAgent("PURRNext - An E621 CLI BACKEND", "0.01", "EdgarTakamura", "Bluesky")
                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                    .Build();

                    Fetcher fetcher = new Fetcher(e621Client, result_tags);
                    fetcher.AssignLoggerInstance(global_logger);
                    fetcher.AssignConfigurationFile(global_config);
                    fetcher.AssignOutputDir(SessionOutput);
                    fetcher.AssignBlacklistString(backup_list_string);
                    fetcher.Start();
                }
                else if(topic == "2")
                {
                    var result_tags = "";
                    if(global_config.TagsFromFile == true)
                    {
                        Console.WriteLine("You've set to use a file for storing your tags, do you want to use it for your search? - [Y/N]");
                        var response = Console.ReadLine();

                        if(response == "Y" || response == "y")
                        {
                            Console.WriteLine("Okie dokie proceeding then");
                            goto limit1;
                        }
                        else if(response == "N" || response == "n")
                        {
                            Console.WriteLine("Okay then!");
                            goto tg1;
                        }
                        else if(response == "YES" || response == "yes")
                        {
                            Console.WriteLine("Okie dokie proceeding then");
                            goto limit1;
                        }
                        else if(response == "NO" || response == "no")
                        {
                            Console.WriteLine("Okay then!");
                            goto tg1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tags from File feature not enable, proceeding");
                    }

                    tg1:
                        Console.WriteLine("Just type your tags below and press enter to begin your search!");
                        var tags = Console.ReadLine();

                        Console.WriteLine("Checking for blank spaces");
                        if(tags.Length != 0)
                        {
                            if(tags.ElementAt(tags.Length-1) == ' ')
                            {
                                Console.WriteLine("Blank space found on string end, removing");
                                tags = tags.Remove(tags.Length-1);
                            }
                            else
                            {
                                Console.WriteLine("No blank space, congrats");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Tags can't be empty, try again");
                            Environment.Exit(0);
                            
                        }
                    
                    Console.WriteLine($"Those are your tags: {tags}");
                    Console.WriteLine("Do you confirm?");
                    var confirm = Console.ReadLine();

                    if(confirm == "Y" || confirm == "y")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                        result_tags = tags;
                    }
                    else if(confirm == "N" || confirm == "n")
                    {
                        Console.WriteLine("Okay then!");
                        goto tg1;
                    }
                    else if(confirm == "YES" || confirm == "yes")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                        result_tags = tags;
                    }
                    else if(confirm == "NO" || confirm == "no")
                    {
                        Console.WriteLine("Okay then!");
                        goto tg1;
                    }


                    limit1:
                    Console.WriteLine("The app will be using a limit for fetching posts!");
                    Console.WriteLine("You can change this limit whenever you want by changing the 'config.json' fike");

                    Console.WriteLine("BUT WAIT A SECOND!!");

                    p_select:
                    Console.WriteLine("This mode accepts pagination, how many pages you want to search on?");
                    var pages = Int32.Parse(Console.ReadLine());

                    if(pages >= 20)
                    {
                        Console.WriteLine("Woah, that's a lot of posts >w>");
                    }
                    else if(pages < 20)
                    {
                        Console.WriteLine("Wooow, lotsa posts to download +w+");
                    }
                    Console.WriteLine("Do you confirm?");
                    var confirm_pages = Console.ReadLine();

                    if(confirm_pages == "Y" || confirm_pages == "y")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                    }
                    else if(confirm_pages == "N" || confirm_pages == "n")
                    {
                        Console.WriteLine("Okay then!");
                        goto p_select;
                    }
                    else if(confirm_pages == "YES" || confirm_pages == "yes")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                    }
                    else if(confirm_pages == "NO" || confirm_pages == "no")
                    {
                        Console.WriteLine("Okay then!");
                        goto p_select;
                    }

                    Console.WriteLine("- STAAARTING -");

                    Console.WriteLine("Composing tags blacklist");
                    var BL = LoadBlackListText();
                    var backup_list_string = "";
                    if(BL.Count != 0)
                    {
                        for(int en = 0; en < BL.Count; en++)
                        {
                            if(result_tags.Contains(BL[en]))
                            {
                                int current = en;
                                Console.WriteLine("Tags conflict with blacklist");
                                Console.WriteLine($"Removing - {BL[current]}");
                                result_tags = result_tags.Replace(BL[current], "");
                            }
                        }
                        
                        //Compose final string with both search tags and blacklist tags
                        var blacklist_string = ComposeBlacklistString(BL);
                        result_tags = $"{result_tags} {blacklist_string}";
                        Console.WriteLine($"Final TAG string - {result_tags}");
                        backup_list_string = blacklist_string;
                    }
                    else
                    {
                        Console.WriteLine("No tags found on the loaded blacklist file");
                    }

                    var e621Client = new E621ClientBuilder()
                    .WithUserAgent("PURRNext - An E621 CLI BACKEND", "0.01", "EdgarTakamura", "Bluesky")
                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                    .Build();

                    Fetcher fetcher = new Fetcher(e621Client, result_tags, pages);
                    fetcher.AssignLoggerInstance(global_logger);
                    fetcher.AssignConfigurationFile(global_config);
                    fetcher.AssignOutputDir(SessionOutput);
                    fetcher.AssignBlacklistString(backup_list_string);
                    fetcher.Start();
                }


            }
            else if (mode == "UPDATE-TAGS")
            {   
                SetupDirectories();
                Console.WriteLine("Updating tags");
                TagUpdater tg = new TagUpdater();
                tg.WithListPath(UpdatesFile);
                tg.LoadTagList();
                var r = tg.UpdateTagsAsync(global_logger);
                r.Wait();
                
                if(r.IsCompleted)
                {
                    /* if(r.Result == true)
                    {
                        Console.WriteLine("Task Completed Successfully");
                        goto end;
                    }
                    else
                    {
                        Console.WriteLine("Task didn't completed due an error check the log file to know what happened");
                        goto end;
                    } */
                }
                
                //Discarded Code
                /*
                var j = LoadJson();
                var us = await UpdateAsync(j);
                if(us == true)
                {
                    Console.WriteLine("Update ended its run");
                }
                */
            }
            else if(mode == "Docker")
            {   

                //Intro
                Console.WriteLine("\n--------------------------------------------");
                Console.WriteLine("-------------------------------------------------- --    -\n");

                Console.WriteLine("     PURR NEXT - An E621 CLI BACKEND (WIP)");
                Console.WriteLine("     VERSION - 0.0.1");
                Console.WriteLine("     Author: Edgar Takamura");

                Console.WriteLine("\n-------------------------------------------------- --    -");
                Console.WriteLine("--------------------------------------------\n");    
                
                Console.WriteLine($"[System] - Available Cores: {Environment.ProcessorCount}");
                
                //Initialize directories
                Console.WriteLine("INITIALIZING DOCKER DIRECTORIES");
                SetupDockerDirectories();

                try
                {
                    

                Console.WriteLine("Initializing main thread!");
                Task main = new Task(
                async ()=>
                {
                    var done = false;
                    while(!done)
                    {
                        //Loads the Tags file and check for the amount of returned tags
                        var Searches = TagImporter.ImportTags(TagsFile);

                        if(Searches.Count != 0)
                        {
                            List<Task> Tasks = new List<Task>();
                            for(int i = 0; i < Searches.Count; i++)
                            {
                                
                                var tag_string = Searches[i].Tag;
                                var tag_pages = Searches[i].Pages;
                                var tag_amount = Searches[i].Amount;

                                Console.WriteLine("Composing tags blacklist");

                                var BL = LoadBlackListText();
                                var backup_list_string = "";
                                
                                if(BL.Count != 0)
                                {
                                    for(int en = 0; en < BL.Count; en++)
                                    {
                                        if(tag_string .Contains(BL[en]))
                                        {
                                            int current = en;
                                            Console.WriteLine("Tags conflict with blacklist");
                                            Console.WriteLine($"Removing - {BL[current]}");
                                            tag_string = tag_string.Replace(BL[current], "");
                                        }
                                    }
                                    
                                    //Compose final string with both search tags and blacklist tags
                                    var blacklist_string = ComposeBlacklistString(BL);
                                    tag_string = $"{tag_string} {blacklist_string}";
                                    Console.WriteLine($"Final TAG string - {tag_string}");
                                    backup_list_string = blacklist_string;
                                }
                                else
                                {
                                    Console.WriteLine("No tags found on the loaded blacklist file");
                                }

                                var t = Task.Run(
                                async ()=>
                                {
                                    var e621Client = new E621ClientBuilder()
                                    .WithUserAgent("PURRNext - An E621 CLI BACKEND", "0.01", "EdgarTakamura", "Bluesky")
                                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                                    .Build();

                                    Fetcher fetcher = new Fetcher(e621Client, tag_string, tag_pages);
                                    fetcher.AssignLoggerInstance(global_logger);
                                    fetcher.AssignConfigurationFile(global_config);
                                    fetcher.AssignOutputDir(SessionOutput);
                                    fetcher.AssignBlacklistString(backup_list_string);
                                    fetcher.Start();
                                });
                                
                                Tasks.Add(t);
                            }
                            Task.WhenAll(Tasks).Wait();

                            Console.WriteLine("Sleeping -w-");
                            //Sleeps the thread, waiting for the next batch of searches;
                            Thread.Sleep(TimeSpan.FromMinutes(120));
                        }
                        else
                        {
                            Console.WriteLine("No searches to be done!");
                            Console.WriteLine("Sleeping...");
                            //Sleeps the thread if there is no tags to search;
                            Thread.Sleep(TimeSpan.FromMinutes(120));
                        }
                    }
                });
                Thread mt = new Thread(main.Start);
                mt.Name = "Main Thread";
                mt.Start();

                Console.WriteLine("Initializing updater thread!");
                Task updater = new Task(
                async ()=>
                {
                    var done = false;
                    while(!done)
                    {
                        var update_task = Task.Run(async ()=>
                        {
                            TagUpdater updtr = new TagUpdater();
                            updtr.WithListPath(UpdatesFile);
                            updtr.LoadTagList();
                            var result = await updtr.UpdateTagsAsync(global_logger);
                            if(result == true)
                            {
                                Console.WriteLine("All tags have been updated");
                            }
                            else
                            {
                                Console.WriteLine("Something happened within the Updater, check the logs for a detailed report");
                            }
                        });
                        Task.WhenAll(update_task).Wait();
                        Thread.Sleep(TimeSpan.FromMinutes(120));
                    }
                });
                Thread ut = new Thread(updater.Start);
                ut.Name = "Updater Thread";
                ut.Start();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"An error has occurred\nError - {e}");
                }
                //Console.WriteLine();
                //Thread.Sleep(TimeSpan.FromMinutes(25));

                //Parallel.Invoke
                //Discarded Code
                /*
                Console.WriteLine("Running on docker mode");
                //Parse Env Variables here
                var workPath = Environment.GetEnvironmentVariable("WORKING_DIRECTORY"); //Parse the path to where download the files
                var TimerInterval = Environment.GetEnvironmentVariable("TIMER_INTERVAL"); //The amount of time before the next run on the queue

                 if(Directory.Exists("/p_storage"))
                {
                    Console.WriteLine("Volume is ready for use");
                }

                if(!File.Exists($"{workPath}/Sessions/index.entry.json"))
                {
                    Console.WriteLine("Index file not found.... Setting up......");
                } */
                //SetupDockerDirectories();
				//Taken from:
				//https://learn.microsoft.com/en-us/dotnet/standard/threading/creating-threads-and-passing-data-at-start-time
                //Thread t = new(new ThreadStart(Fetcher.Start));

            }
            else if(mode == "GUI")
            {
                Console.WriteLine("Running on GUI mode!");
            }

            /* var e621Client = new E621ClientBuilder()
            .WithUserAgent("PURRNext- An E621 CLI BACKEND", "0.01", "EdgarTakamura", "Bluesky")
            .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
            .WithRequestInterval(E621Constants.MinimumRequestInterval)
            .Build();

            var ids = FetchPostsIDsAsync(e621Client, "rough_sex");
            ids.Wait();

            if (ids.IsCompleted)
            {
                for (int i = 0; i < ids.Result.Count; i++)
                {
                    var id = ids.Result[i];
                    Console.WriteLine($"ID - {id}");
                }
            } */

        /*     var e621Client = new E621ClientBuilder()
            .WithUserAgent("PURRNext - An E621 CLI BACKEND", "0.01", "EdgarTakamura", "Bluesky")
            .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
            .WithRequestInterval(E621Constants.MinimumRequestInterval)
            .Build();
            var ods = FetchPostsPages(e621Client, "umpherio", 60);
            ods.Wait();

            if(ods.IsCompleted)
            {
                Console.WriteLine("Task completed");
                Console.WriteLine($"Amount - {ods.Result["Posts"].Count}");
                Console.WriteLine($"Latest Post - {ods.Result["Latest"]}");
            }
            

            Console.WriteLine("Work in Progress");

            end:
            Console.WriteLine("Quit?");
            var b = Console.ReadLine();
            if (b == "yes")
            {
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("fuck off dude");
                Environment.Exit(0);
            } */

            Console.WriteLine("What do you want to do now?");
            var b = Console.ReadLine();
            if(b.ToLower() == "restart")
            {
                goto start;
            }
            else if(b.ToLower() == "quit")
            {
                Console.WriteLine("End of Runtime!");
                Console.WriteLine("See you next time!");
                Console.ReadLine();
                Environment.Exit(0);
            }           
        }
    }
}
