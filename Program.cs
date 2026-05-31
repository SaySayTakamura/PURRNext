using Noppes.E621;
using System.Text;
using PURRNext.Configs;
using PURRNext.TUpdater;
using PURRNext.LOG;
using Newtonsoft.Json;
using PURRNext.TagFile;
using PURRNext.LoginData;
using PURRNext.Crypto.Hash;
using PURRNext.Crypto;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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

        //Global blacklist, loaded from file
        static List<string> GlobalBlacklist = new List<string>();

        static void DockerMain()
        {
            while(true)
            {
                //Loads the Tags file and check for the amount of returned tags
                var Searches = TagImporter.ImportTags(TagsFile);

                if(Searches.Count != 0)
                {
                    List<Task> Tasks = new List<Task>();
                    for(int i = 0; i < Searches.Count; i++)
                    {
                        
                        var tag_string = Searches[i].Search;
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

                            if(File.Exists($"{DataDir}/Serializer.json"))
                            {
                                Console.WriteLine("Login file detected!");

                                var stream = File.ReadAllBytes("PURRNext.dll");
                                var rash_pass = Hashing.SetHash(Encoding.ASCII.GetString(stream), false);

                                using (StreamReader r = new StreamReader($"{DataDir}/Serializer.json"))
                                {

                                    string json = r.ReadToEnd();
                                    var data = JsonConvert.DeserializeObject<LoginInputData>(json);
                                    var u = StringCipher.Decrypt(data.Username, rash_pass);
                                    var p = StringCipher.Decrypt(data.Password, rash_pass);

                                    Console.WriteLine("Logging in!");

                                    var log = await e621Client.LogInAsync(u, p);

                                    if(log == true)
                                    {
                                        Console.WriteLine("Successfuly Logged!!");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Something Went Wrong");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("No Login file detected, proceeding without logging in!");
                            }

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
                    Console.WriteLine("Cleaning UP Tags");
                    TagImporter.ClearTags(TagsFile);

                    //Sleeps the thread, waiting for the next batch of searches;
                    Console.WriteLine("Sleeping -w-");
                    Task.Delay(TimeSpan.FromMinutes(5)).Wait();
                }
                else
                {
                    Console.WriteLine("No searches to be done!");
                    Console.WriteLine("Sleeping -w-");
                    //Sleeps the thread if there is no tags to search;
                    //Thread.Sleep(TimeSpan.FromMinutes(120));
                    Task.Delay(TimeSpan.FromMinutes(5)).Wait();
                }
            }
        }

        static void DockerUpdater()
        {

        }

        //Loads blacklist file
        static List<string> LoadBlackListText()
        {
            var result = new List<string>();
            var lines = File.ReadAllLines(BlacklistFile);
            for(int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i].Trim();
                result.Add(line);
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

            //Builds the string and trims it.
            var s = builder.ToString();
            result = s.Trim();

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
                        Console.WriteLine($"- Auto Login - {global_config.AutoLogin}");
                        Console.WriteLine($"- Save videos on their own folder - {global_config.VideoOnFolders}");
                        Console.WriteLine($"- Save Flash Posts on their own folder - {global_config.FlashOnFolders}");
                        Console.WriteLine($"- Maximum posts to retrieve by API Call - {global_config.MaxPostsPerCall}");
                        Console.WriteLine($"- Maximum posts to retrieve by Pagination - {global_config.MaxPostsPerPage}");
                        Console.WriteLine($"- Database Driver - {global_config.DatabaseDriver}");
                        Console.WriteLine($"- Database Driver - {global_config.DatabasePath}");
                        Console.WriteLine($"- Last modified at - {File.GetLastWriteTime(ConfigFile).ToString("G")}\n");

                        Configuration base_cfg = new Configuration();

                        if (global_config.Version != base_cfg.Version)
                        {
                            Console.WriteLine($"Outdated config file, preparing for update\nCurrent Version: {global_config.Version}\nLatest: {base_cfg.Version}");
                            var nCFG = base_cfg;
                            nCFG.AutoLogin = global_config.AutoLogin;
                            nCFG.VideoOnFolders = global_config.VideoOnFolders;
                            nCFG.FlashOnFolders = global_config.FlashOnFolders;
                            nCFG.MaxPostsPerCall = global_config.MaxPostsPerCall;
                            nCFG.MaxPostsPerPage = global_config.MaxPostsPerPage;
                            nCFG.DatabaseDriver = global_config.DatabaseDriver;
                            nCFG.DatabasePath = global_config.DatabasePath;

                            global_config = nCFG;
                            writer.SaveConfigFile(nCFG);
                            Console.WriteLine($"Configuration file update to version - {base_cfg.Version}");

                        }

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
            var sample_login_input_file = $"{data_path}/sample-login-input.txt";

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

            //Creates a sample-login-input.txt file if one doesn't exists.
             if(!File.Exists(sample_login_input_file))
            {
                Console.WriteLine("Sample login file does not exist, creating one!");
                using (FileStream fs = File.Create(sample_login_input_file))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"U=YOUR USERNAME HERE\nP=YOUR PASSOWRD HERE");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    fs.Close(); //Remove in case of regret
                }
                Console.WriteLine("Done!");
            }
            else
            {
                Console.WriteLine("Sample login file exist, great!");
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

                //Check DB Type
                //if(Environment.GetCommandLineArgs().Contains("")

                //Take this into account
                //Link: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/main-command-line
                var arg_list = Environment.GetCommandLineArgs().ToList();
                var db = arg_list.Find(x=> x.Contains("--db="));
                if(db != null)
                {
                    Console.WriteLine("Intercepted database command line argument");
                    db = db.Trim().ToLower();
                    if(db == "mongo")
                    {
                        Console.WriteLine("Selected DB - MongoDB");
                        cfg.DatabaseDriver = "MONGO";
                        cfg.DatabasePath = "mongodb://repoUser:996854@db:27017";
                        Console.WriteLine("Assigned mongodb instance path and Driver\nInstance URL: db:27017\nNote that you need to change 'db' your host address so you can access it from other sources");
                    }else if(db == "sql")
                    {
                        Console.WriteLine("Selected DB - SQLite");
                        cfg.DatabaseDriver = "SQL";
                        cfg.DatabasePath = $"{data_path}/db.sqlite";
                        Console.WriteLine("Assigned mongodb instance path and Driver\nInstance URL: db:27017\nNote that you need to change 'db' your host address so you can access it from other sources");
                    }
                }

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
            Console.WriteLine("     VERSION - 0.8.5");
            Console.WriteLine("     Author: Edgar Takamura");

            Console.WriteLine("\n-------------------------------------------------- --    -");
            Console.WriteLine("--------------------------------------------\n");     

            Console.WriteLine($"[System] - Available Cores: {Environment.ProcessorCount}");

            /* var Greet = "u=hello";
            if(Greet.Contains("U=", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("found the user");
                var result = Greet.Replace("U=", "", StringComparison.CurrentCultureIgnoreCase);
                Console.WriteLine(result);
            } */



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
//Label - Topic 1: Prompt Checkpoint 0
T1_PROMPT_CHECKPOINT_0:
                        Console.WriteLine("You've set to use a file for storing your tags, do you want to use it for your search? - [Y/N]");
                        
                        var response = Console.ReadLine();
                        response = response.ToLower();
                        response = response.Trim();

                        //Checks the response
                        if(response == "y" || response == "yes")
                        {
                            Console.WriteLine("Okie dokie proceeding then");
                            goto T1_LIMIT_CHECKPOINT;
                        }
                        else if(response == "n" || response == "no")
                        {
                            Console.WriteLine("Okay then!");
                            //Goto directine not needed, please uncomment this if something goes wrong.
                            //goto fl_tg1;
                        }
                        else
                        {
                            Console.WriteLine("Invalid response, try again");
                            goto T1_PROMPT_CHECKPOINT_0;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tags from File feature not enabled, proceeding");
                    }

//Label - Topic 1: Tag Checkpoint
T1_TAG_CHECKPOINT:
                    Console.WriteLine("Just type your tags below and press enter to begin your search!");
                    var tags = Console.ReadLine();
                    //Trims the whole string so it doesn't have any Whitespaces on either ends
                    //The string should be trimmed to avoid empty-spaced strings which results
                    //on dumping the e6's front page on the root of the sessions directory.
                    tags = tags.Trim();


                    if(tags.Length != 0)
                    {
                        
                        //Splits the whole string, remove inbetween whitespaces and trim the entries
                        var tag_list = tags.Split(' ', options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                        //Joins the string back into a single string
                        tags = string.Join(" ", tag_list);
                    }
                    else
                    {
                        Console.WriteLine("Tags can't be empty, try again");
                        Environment.Exit(0);
                        Console.ReadLine();
                    }
//Label - Topic 1: Prompt Checkpoint 1
T1_PROMPT_CHECKPOINT_1:
                    Console.WriteLine($"Those are your tags: {tags}");
                    Console.WriteLine("Do you confirm?");
                    var confirm = Console.ReadLine();

                    //Sets the whole string to lowercase to generalize the result and make code more simple
                    confirm = confirm.ToLower(); 

                    //Checks the response, the user can quit from here as well
                    if(confirm == "y" || confirm == "yes")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                        result_tags = tags;
                    }
                    else if(confirm == "n" || confirm == "no")
                    {
                        Console.WriteLine("Okay then, let's try again!");
                        goto T1_TAG_CHECKPOINT;
                    }
                    else
                    {
                        Console.WriteLine("Invalid response, try again");
                        goto T1_PROMPT_CHECKPOINT_1;
                    }
//Label - Topic 1: Limit Checkpoint
T1_LIMIT_CHECKPOINT:
                    Console.WriteLine("The app will be using a limit for fetching posts!");
                    Console.WriteLine("You can change this limit whenever you want by changing the 'config.json' file.");

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
                    .WithUserAgent("PURRNext - An E621 CLI BACKEND", "0.05b", "EdgarTakamura", "Bluesky")
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
//Label - Topic 2: Prompt Checkpoint 0
T2_PROMPT_CHECKPOINT_0:
                        Console.WriteLine("You've set to use a file for storing your tags, do you want to use it for your search? - [Y/N]");
                        var response = Console.ReadLine();
                        response = response.ToLower();
                        response = response.Trim();
                        //Checks the response
                        if(response == "y" || response == "yes")
                        {
                            Console.WriteLine("Okie dokie proceeding then");
                            goto T2_LIMIT_CHECKPOINT;
                        }
                        else if(response == "n" || response == "no")
                        {
                            Console.WriteLine("Okay then!");
                            //Goto directine not needed, please uncomment this if something goes wrong.
                            //goto fl_tg2;
                        }
                        else
                        {
                            Console.WriteLine("Invalid response, try again");
                            goto T2_PROMPT_CHECKPOINT_0;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tags from File feature not enabled, proceeding");
                    }
//Label - Topic 2: Tag Checkpoint
T2_TAG_CHECKPOINT:
                    Console.WriteLine("Just type your tags below and press enter to begin your search!");
                    var tags = Console.ReadLine();

                    /*  
                        Trims the whole string so it doesn't have any Whitespaces on either ends
                        The string should be trimmed before checking to avoid empty-spaced strings which results
                        on dumping the e6's front page on the root of the sessions directory.
                    */
                    tags = tags.Trim();

                    if(tags.Length != 0)
                    {

                        //Splits the whole string, remove inbetween whitespaces and trim the entries
                        var tag_list = tags.Split(' ', options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                        //Joins the string back into a single string
                        tags = string.Join(" ", tag_list);
                    }
                    else
                    {
                        Console.WriteLine("Tags can't be empty, try again");
                        //Use for testing
                        //Environment.Exit(0);
                    }

/*
    I could use "T2_TAG_CHECKPOINT_0" for this, but i felt that going back all the way and being forced to 
    REWRITE your tags again would be tiresome/unecessary, so we only go back to THIS prompt and if the user really wants
    to REWRITE his tags, he can simply say "No" or "N"          
*/
//Label - Topic 2: Prompt Checkpoint 1
T2_PROMPT_CHECKPOINT_1:
                    Console.WriteLine($"Those are your tags: {tags}");
                    Console.WriteLine("Do you confirm?");
                    var confirm = Console.ReadLine();

                    //Sets the whole string to lowercase to generalize the result and make code more simple
                    confirm = confirm.ToLower(); 

                    //Checks the response, the user can quit from here as well
                    if(confirm == "y" || confirm == "yes")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                        result_tags = tags;
                    }
                    else if(confirm == "n" || confirm == "no")
                    {
                        Console.WriteLine("Okay then, let's try again!");
                        goto T2_TAG_CHECKPOINT;
                    }
                    else
                    {
                        Console.WriteLine("Invalid response, try again");
                        goto T2_PROMPT_CHECKPOINT_1;
                    }
//Label - Topic 2: Limit Checkpoint 1
T2_LIMIT_CHECKPOINT:
                    Console.WriteLine("The app will be using a limit for fetching posts!");
                    Console.WriteLine("You can change this limit whenever you want by changing the 'config.json' file.");

//Label - Topic 2: Pagination Checkpoint
T2_PAGE_SELECT_CHECKPOINT:
                    Console.WriteLine("This mode accepts pagination, how many pages you want to search on?");
                    var pages = Int32.Parse(Console.ReadLine());

                    //Aesthetic Text
                    //Remove as needed
                    if(pages >= 20)
                    {
                        Console.WriteLine("Woah, that's a lot of posts >w>");
                    }
                    else if(pages < 20)
                    {
                        Console.WriteLine("Wooow, lotsa posts to download +w+");
                    }
                    else if(pages <= 0)
                    {
                        Console.WriteLine("The page parameter must be more than 0");
                        Console.WriteLine("Reverting...");
                        goto T2_PAGE_SELECT_CHECKPOINT;
                    }

//Label - Topic 2: Prompt Checkpoint 1
T2_PROMPT_CHECKPOINT_2:
                    Console.WriteLine("Do you confirm?");
                    var confirm_pages = Console.ReadLine();

                    //Sets the whole string to lowercase to generalize the result and make code more simple
                    confirm_pages.ToLower();

                    if(confirm_pages == "y" || confirm_pages == "yes")
                    {
                        Console.WriteLine("Okie dokie proceeding then");
                    }
                    else if(confirm_pages == "n" || confirm_pages == "no")
                    {
                        Console.WriteLine("Okay then!");
                        goto T2_PAGE_SELECT_CHECKPOINT;
                    }
                    else
                    {
                        Console.WriteLine("Invalid response, try again");
                        goto T2_PROMPT_CHECKPOINT_2;
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

                    Console.WriteLine("Before proceeding...");

                    if(!File.Exists("./Serializer.json"))
                    {
//Label - Topic 2: Login Prompt Checkpoint
T2_PROMPT_CHECKPOINT_3:
                        Console.WriteLine("Do you want to login with your account?");
                        var login_request = Console.ReadLine();
                        LoginInputData LoginData;

                        //Sets the whole string to lowercase to generalize the result and make code more simple
                        login_request = login_request.ToLower(); 

                        //Checks the response, the user can quit from here as well
                        if(login_request == "y" || login_request == "yes")
                        {
                            Console.WriteLine("Great!");
                            Console.WriteLine("First, please input your username");
//Label - Topic 2: Username Input Checkpoint 1
T2_USERNAME_INPUT_CHECKPOINT:
                            Console.WriteLine("Username: "); 
                            var username = Console.ReadLine();
                            username = username.Trim();

                            if(username.Length == 0)
                            {
                                Console.WriteLine("Invalid username, please try again!");
                                goto T2_USERNAME_INPUT_CHECKPOINT;
                            }

                            Console.WriteLine($"Now input the API key for the user - {username}");
//Label - Topic 2: Password Input Checkpoint
T2_PASSWORD_INPUT_CHECKPOINT:
                            var apk = "";
                            while (true)
                            {
                                var key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.Enter)
                                {
                                    Console.Write("\n");

                                    break;
                                }
                                if (key.Key != ConsoleKey.Backspace)
                                {
                                    Console.Write("*");
                                    apk += key.KeyChar;
                                }
                                else
                                {
                                    if (apk.Length != 0)
                                    {
                                        Console.Write("\b \b");
                                        apk = apk.Remove(apk.Length - 1);
                                    }
                                }
                            }
                            apk = apk.Trim();

                            if(apk.Length == 0)
                            {
                                Console.WriteLine("Invalid password/API Key, please try again!");
                                goto T2_PASSWORD_INPUT_CHECKPOINT;
                            }
//Label - Topic 2: Credentials Save Prompt Checkpoint 
T2_PROMPT_CHECKPOINT_4:
                            Console.WriteLine("Great, would you like to save your credentials?");
                            Console.WriteLine("Everything will be encrypted to avoid issues");
                            Console.WriteLine("Note that this also enables auto-login.");

                            var save_creds = Console.ReadLine();
                            
                            //Sets the whole string to lowercase to generalize the result and make code more simple
                            save_creds = save_creds.ToLower(); 

                            //Checks the response, the user can quit from here as well
                            if(save_creds == "y" || save_creds == "yes")
                            {
                                Console.WriteLine("Okay, please input a master password for encryption");

                                var mp = "";
                                while (true)
                                {
                                    var key = Console.ReadKey(true);
                                    if (key.Key == ConsoleKey.Enter)
                                    {
                                        Console.Write("\n");

                                        break;
                                    }
                                    if (key.Key != ConsoleKey.Backspace)
                                    {
                                        Console.Write("*");
                                        mp += key.KeyChar;
                                    }
                                    else
                                    {
                                        if (mp.Length != 0)
                                        {
                                            Console.Write("\b \b");
                                            mp = mp.Remove(mp.Length - 1);
                                        }
                                    }
                                }
                                mp = mp.Trim();

                                Console.WriteLine("Testing connection with available credentials");
                                var log = e621Client.LogInAsync(username, apk);
                                log.Wait();

                                //Check for success to login with the currently available credentials
                                var log_success = log.Result;
                                if (log_success)
                                {
                                    Console.WriteLine("Success! You are logged for this session");
                                    Console.WriteLine("Your credentials will now be saved!");

                                    var mp_hash = Hashing.SetHash(mp);
                                    var encrypted_usrnm = StringCipher.Encrypt(username, mp_hash);
                                    var encrypted_apk = StringCipher.Encrypt(apk, mp_hash);
                                    LoginData = new LoginInputData(encrypted_usrnm, encrypted_apk, mp_hash);


                                    //Sets options for the serializers
                                    /*
                                    System.Text.Json.JsonSerializerOptions opts = new()
                                    {
                                        IncludeFields = true
                                    };
                                    */

                                    //var op = System.Text.Json.JsonSerializer.Serialize(LoginData, options: opts);
                                    var op = JsonConvert.SerializeObject(LoginData);
                                    var ss = RandomNumberGenerator.Create().GetHashCode();
                                    var eop = StringCipher.Encrypt(op, $"{ss}");

                                    using (StreamWriter file = File.CreateText($"{WorkDir}/Serializer.json"))
                                    {
                                        file.Write($"{ss}|/|{eop}");
                                        file.Close();//Remove in case of regret
                                    }

                                    Console.WriteLine("Done! Your credentials are now saved");
                                }
                                else
                                {
                                    Console.WriteLine("An error has occurred");
                                    Console.WriteLine("Please, try again later.");

                                }

                            }
                            else if(save_creds == "n" || save_creds == "no")
                            {
                                Console.WriteLine("Okay, but if you want to login you will need to input all your credentials again");
                                Console.WriteLine("An attempt on loging will now be processed");
                                var log = e621Client.LogInAsync(username, apk);
                                log.Wait();

                                //Check for success to login with the currently available credentials
                                var log_success = log.Result;
                                if (log_success)
                                {
                                    Console.WriteLine("Success! You are logged for this session");
                                }
                                else
                                {
                                    Console.WriteLine("Failure! An issue occurred while trying to login to e621");
                                    Console.WriteLine("Check e621 status or try again later");
                                    Console.ReadLine();
                                    Environment.Exit(621);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid response, try again");
                                goto T2_PROMPT_CHECKPOINT_4;
                            }

                        }
                        else if(login_request == "n" || login_request == "no")
                        {
                            Console.WriteLine("Okay then, proceeding.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid response, try again");
                            goto T2_PROMPT_CHECKPOINT_3;
                        }
                    
                    }
                    else
                    {
                        if(global_config.AutoLogin == true)
                        {
                            Console.WriteLine("Auto-Login File found, loging in");
                            var line = File.ReadAllLines($"{WorkDir}/Serializer.json")[0];
                            var match = Regex.Match(line, @"(.+?)\|\/\|(.+)");

                            if(match.Success)
                            {
                                Console.WriteLine("Match found");
                                var key = match.Groups[1].Value;
                                var content = match.Groups[2].Value;
                                //Console.WriteLine($"K: {key}\nContent: {content}");
                                //Console.WriteLine("Decrypting");
                                var ld = StringCipher.Decrypt(content, key);
                                var ldc = JsonConvert.DeserializeObject<LoginInputData>(ld);
                                //Console.WriteLine($"LDCC:\nUsername: {ldc.Username}\nPass: {ldc.Password}\nMN: {ldc.MagicNumber}");
                                var dpu = StringCipher.Decrypt(ldc.Username, ldc.MagicNumber);
                                var dpp = StringCipher.Decrypt(ldc.Password, ldc.MagicNumber);

                                var l = e621Client.LogInAsync(dpu, dpp);
                                l.Wait();
                                if(l.Result == true)
                                {
                                    Console.WriteLine("Login from file succeeded");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid Regex Try Again");
                                Console.ReadLine();
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
//T2_PROMPT_CHECKPOINT_5:
                            Console.WriteLine("Credentials File found but Auto-Login is disabled");
                            Console.WriteLine("Enable Auto-Login or Dismiss this message if you don't want to login.");

                        }
                    }
                    Console.WriteLine("Proceeding");

                    DatabaseContext db = new DatabaseContext(global_config.DatabaseDriver, global_config.DatabasePath);

                    Fetcher fetcher = new Fetcher(e621Client, result_tags, pages);
                    fetcher.AssignLoggerInstance(global_logger);
                    fetcher.AssignConfigurationFile(global_config);
                    fetcher.AssignOutputDir(SessionOutput);
                    fetcher.AssignBlacklistString(backup_list_string);
                    fetcher.AssignDatabaseContext(db);
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

                //Initialize directories
                Console.WriteLine("INITIALIZING DOCKER DIRECTORIES");
                SetupDockerDirectories();
                
                try
                {
                    if(args.Length > 1)
                    {
                        if(args[1].Contains("login", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("Welcome to PURR->Next Docker Login Assistant");
                            Console.WriteLine("Reminder: This argument is only used to REGISTER a user\nfor the first time!");
                            Console.WriteLine("When we REGISTER a user, we encrypt their login data and store their data elsewhere accessible to the app!");
                            Console.WriteLine("The input file will be deleted once the process is done!");
                            Console.WriteLine("---------------------------------------------------- -\n");
                            var LDPath = $"{DataDir}/login-input.txt";
                            LoginInputData LoginData;

                            if(File.Exists(LDPath))
                            {
                                var ui = "";
                                var pi= "";
                                Console.WriteLine("Found the Login file, loading data");
                                //var lines = File.ReadAllLines(LDPath);

                                var text = File.ReadAllText(LDPath);
                                text = text.Trim();
                                Console.WriteLine($"Loaded - {text.Replace("\r", "\\r").Replace("\n", "\\n")}");
                                var reg = Regex.Match(text, @"U=(.+?)\r\nP=(.+)");
                                if(reg.Success)
                                {
                                    Console.WriteLine("Captured");
                                    Console.WriteLine($"U={reg.Groups[1].Value}");
                                    Console.WriteLine($"P={reg.Groups[2].Value}");
                                    ui = reg.Groups[1].Value;
                                    pi = reg.Groups[2].Value;
                                }
                                else
                                {
                                    Console.WriteLine("Login input file malformed or corrupt! Try again.");
                                    Environment.Exit(77);
                                }

                                //Encrypts input data
                                try
                                {
                                    Console.WriteLine("Encrypting data");
                                    //Uses a RNG to create a master password to avoid user Input
                                    var enc_code = RandomNumberGenerator.Create().GetHashCode().ToString();
                                    var locker_hash = Hashing.SetHash(enc_code, false);

                                    //Encrypts both, Username and Password
                                    var u = StringCipher.Encrypt(ui, locker_hash);
                                    var p = StringCipher.Encrypt(pi, locker_hash);
                                    LoginData = new LoginInputData(u, p, locker_hash); //Creates a Serializable instance of thse variables.

                                    var op = JsonConvert.SerializeObject(LoginData);
                                    var ss = RandomNumberGenerator.Create().GetHashCode();
                                    var eop = StringCipher.Encrypt(op, $"{ss}");

                                    using (StreamWriter file = File.CreateText($"{DataDir}/Serializer.json"))
                                    {
                                        file.Write($"{ss}|/|{eop}");
                                        file.Close();//Remove in case of regret
                                    }

                                    Console.WriteLine("Done! Your credentials are now saved");

                                    //The following process 
                                    //Creates a file inside the Data Directory that contains both encrypted User and Password variables
                                    //Note that this process has assigned the HASH of a file as password method of encryption for
                                    //the login data file
                                    //Any changes to the Hashed File will make the Login Data un-accessible.
                                    /*
                                    using (StreamWriter file = File.CreateText($"{DataDir}/Serializer.json"))
                                    {
                                        JsonSerializer serializer = new JsonSerializer();
                                        //serialize object directly into file stream
                                        serializer.Formatting = Formatting.Indented;
                                        serializer.Serialize(file, LoginData);
                                        file.Close();//Remove in case of regret
                                        File.Delete(LDPath); //Deletes the original login file.
                                    }
                                    */
                                    Console.WriteLine("Done! Next time you run a search with PURR->Next you will be already logged onto e621!");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                    Console.ReadLine();
                                }

                                Console.WriteLine("Command Finished!");
                                Environment.Exit(1);

                            }
                            else
                            {
                                Console.WriteLine($"No input file could be found at - {LDPath}\n");
                                Console.WriteLine("If you're in doubt on how to LOGIN, go to the Data directory");
                                Console.WriteLine("and then copy the sample file with:");
                                Console.WriteLine("\n---------------------------------------------------- -\n");
                                Console.WriteLine("cp sample-login-input login-input.txt");
                                Console.WriteLine("\n---------------------------------------------------- -\n");
                                Console.WriteLine("Now you can put your info there and run the Login script again.");
                                Console.WriteLine("Note: This same file will be deleted once the process is finished.\n");
                                Environment.Exit(1);

                            }
                        }
                    }

                    DockerMain();
                    //Parallel.Invoke(DockerMain, DockerUpdater);

                    Console.WriteLine("End of Runtime - Docker");
                    Environment.Exit(0); 
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
