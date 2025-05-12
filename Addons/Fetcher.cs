using System;
using System.Diagnostics;
using System.Text;
using Noppes.E621;
using PURR.PostMD;
using PURRNext.Configs;
using PURRNext.DManager;
using PURRNext.LOG;

public class PostPoolDuplicateData
{
    public int PostID; //The post ID
    public Dictionary<int, string> Pools; //Pool ids with the status for this post on them;

    public PostPoolDuplicateData()
    {
        PostID = -1;
        Pools = new Dictionary<int, string>();
    }
}

public class Fetcher
{
    private IE621Client? iClient = null;
    private string iTags = "";
    private int iPages = 0;
    private string OutputDir = "";
    private Logger? LoggerInstance = null;
    private Configuration? Configuration = null;
    private EntryForm ef;
    private string BlacklistString = "";

    public Fetcher(IE621Client client, string tags)
    {
        Console.WriteLine("Initializing Fetcher Instance");
        iClient = client;
        iTags = tags;
        Console.WriteLine($"Tags - {tags}");

        //Setting up entry form for this instance
        ef = new EntryForm();
        ef.TAG = tags;
        ef.Date = DateTime.Now.ToString("d");
        ef.LastKnownPath = OutputDir;        
    }
    public Fetcher(IE621Client client, string tags, int pages)
    {
        Console.WriteLine("Initializing Fetcher Instance");
        iClient = client;
        iTags = tags;
        iPages = pages;
        Console.WriteLine($"Tags - {tags}");
        Console.WriteLine($"Pages - {pages}");

        //Setting up entry form for this instance
        ef = new EntryForm();
        ef.TAG = tags;
        ef.Date = DateTime.Now.ToString("d");
    }

    public void AssignLoggerInstance(Logger l)
    {
        Console.WriteLine("Assigning Logger instance");
        LoggerInstance = l;
    }
    public void AssignOutputDir(string output)
    {
        Console.WriteLine($"Assigning Output dir to - {output}");
        OutputDir = output;
    }
    public void AssignConfigurationFile(Configuration cfg)
    {
        Configuration = cfg;
    }
    public void AssignBlacklistString(string list)
    {
        BlacklistString = list;
    }

    //Sets the work folder where this fetcher will operate
    private void SetBaseDirectory()
    {
        var result_tags = iTags;
        if(BlacklistString.Length != 0)
        {
            Console.WriteLine($"Removing GLOBAL BLACKLIST");
            Console.WriteLine($"PRE - {result_tags}");
            result_tags = result_tags.Replace(BlacklistString, "");
            Console.WriteLine($"POS - {result_tags}");
        }
        else
        {
            Console.WriteLine("Blacklist string can't be empty for this procedure, nothing has changed");
        }
        if (result_tags.Contains("score:>="))
        {
            Console.WriteLine("Special characters will be removed....");
            var finaltag = result_tags.Replace("score:>=", "Score - BiggerThan ");
            Console.WriteLine($"Result = {finaltag}" + "\n");
            result_tags = finaltag;
        }
        if (result_tags.Contains("score:<="))
        {
            Console.WriteLine("Special characters will be removed....");
            var finaltag = result_tags.Replace("score:<=", "Score - SmallerThan ");
            Console.WriteLine($"Result = {finaltag}" + "\n");
            result_tags = finaltag;
        }
        if (result_tags.Contains("rating:explicit"))
        {
            Console.WriteLine("Special characters will be removed....");
            var finaltag = result_tags.Replace("rating:explicit", "Rating - Explicit");
            Console.WriteLine($"Result = {finaltag}" + "\n");
            result_tags = finaltag;
        }
        else if (result_tags.Contains("rating:safe"))
        {
            Console.WriteLine("Special characters will be removed....");
            var finaltag = result_tags.Replace("rating:safe", "Rating - Safe");
            Console.WriteLine($"Result = {finaltag}" + "\n");
            result_tags = finaltag;
        }
        else if (result_tags.Contains("rating:questionable"))
        {
            Console.WriteLine("Special characters will be removed....");
            var finaltag = result_tags.Replace("rating:questionable", "Rating - Questionable");
            Console.WriteLine($"Result = {finaltag}" + "\n");
            result_tags = finaltag;
        }
        else if(result_tags.Contains("order:")) // In case of issues, remove this
        {
            Console.WriteLine("Special characters will be removed....");
            var finaltag = result_tags.Replace("order:", "Order - ");
            Console.WriteLine($"Result = {finaltag}" + "\n");
            result_tags = finaltag;
        }
        
        if(result_tags.ElementAt(result_tags.Length-1) == ' ')
        {
            Console.WriteLine("Removing blank space");
            result_tags = result_tags.Remove(result_tags.Length-1);
        }
        else
        {
            Console.WriteLine("No blank space found.... Good!");
        }

        //Base path for this fetcher
        var default_path = $"{OutputDir}/{result_tags}"; // t = tags
        
        if (!Directory.Exists(default_path))
        {
            Directory.CreateDirectory(default_path);
            default_path = $"{OutputDir}/{result_tags}/";
        }
        else
        {
            default_path = $"{OutputDir}/{result_tags}/";
        }
        //Sets the variable and notifies the user.
        OutputDir = default_path;
        Console.WriteLine($"Output Directory defined to - {default_path}");
    }

    static private async Task<List<Post>> FetchPostsAsync(IE621Client client, string tgs, int lmt = 75)
    {
        var ps = await client.GetPostsAsync(tags: tgs, limit: lmt);

        var UsablePosts = new List<Post>();
        if (ps.Count != 0)
        {
            for (var i = 0; i < ps.Count; i++)
            {
                var post = ps.ElementAt(i);

                //Removed all the Blacklist checking crap
                //Cleaner not isn't it?
                if (post != null)
                {
                    UsablePosts.Add(post);
                }
            }
        }
        return UsablePosts;
    }
    static private async Task<List<Post>> FetchPostsPagesAsync(IE621Client client, string tgs, int pge, int lmt = 75)
    {
        var ps = await client.GetPostsAsync(tags: tgs, limit: lmt, page: pge);

        var UsablePosts = new List<Post>();
        if (ps.Count != 0)
        {
            for (var i = 0; i < ps.Count; i++)
            {
                var post = ps.ElementAt(i);

                //Removed all the Blacklist checking crap
                //Cleaner not isn't it?
                if (post != null)
                {
                    UsablePosts.Add(post);
                }
            }
        }
        return UsablePosts;
    }

    static private async Task<Pool> FetchPoolAsync(IE621Client client, int ID)
    {
        var pool = await client.GetPoolAsync(ID);
        return pool;
    }
    
    public void Start()
    {
        //Sets the work directory
        SetBaseDirectory();

        //Setting up the Start time of this process for the logger
        ef.Start = DateTime.Now.ToString("HH:mm");

        //Post lists (Making it global to streamline its access on both processes)
        List<Post> PostGallery = new List<Post>();

        //If pages equals 0, paginations is disable, otherwise... . you know the drill
        if(iPages == 0)
        {
            Console.WriteLine("Pagination not selected, proceeding without it!");

            try
            {
                var ps = FetchPostsAsync(client: iClient, tgs: iTags, lmt: Configuration.MaxPostsPerCall);
                ps.Wait();
                if(ps.IsCompleted)
                {
                    Console.WriteLine("Showing ID's now");
                    var result = ps.Result;
                    for (int i = 0; i < result.Count; i++)
                    {
                        Console.WriteLine($"POST[{i}] - {result[i].Id}");
                    }
                }

            }
            catch(Exception e)
            {
                
            }           
        }
        else
        {
            Console.WriteLine("Pagination enabled!");

            try
            {
               
                for(int p = 0; p < iPages; p++)
                {
                    Console.WriteLine($"Fetching posts for page - {p+1}");
                    var ps = FetchPostsPagesAsync(client: iClient, tgs: iTags, lmt: Configuration.MaxPostsPerPage, pge: p+1);
                    ps.Wait();
                    if(ps.IsCompleted)
                    {
                        var result = ps.Result;
                        if(result.Count != 0)
                        {
                            for (int i = 0; i < result.Count; i++)
                            {
                                var post = result[i];
                                if(post.File != null)
                                {
                                    Console.WriteLine($"POST[{p+1}][{i}] - {post.Id}");
                                    PostGallery.Add(post);
                                }
                                else
                                {
                                    Console.WriteLine("A null post has been found, this won't be parsed to the Gallery");
                                    Console.WriteLine("The post may contain items that are blocked without a login");
                                }
                            }
                            Console.WriteLine($"Total fetched posts - {PostGallery.Count}");
                        }
                        else
                        {
                            Console.WriteLine("No posts found for this page");
                            goto process;
                        }
                    }
                }
                
            }
            catch(Exception e)
            {
                Console.WriteLine("An Error has occured, following is the details of the exception");
                Console.WriteLine(e);
                ef.End = DateTime.Now.ToString("HH:mm");
                ef.ERROR.Add(e.ToString());
                LoggerInstance.Log(ef);
            }           
        }

        process:
        try
        {

            //Pools dictionary
            Dictionary<int, List<Post>> Pools = new Dictionary<int, List<Post>>();
            var PPC = 0;
            List<PostPoolDuplicateData> Duplicates = new List<PostPoolDuplicateData>(); //List<Struct<PID,Status>

            //Content lists
            List<Post> Videos = new List<Post>();
            List<Post> SWF_Posts = new List<Post>();

            //Posts marked for deletion go here
            //Note that this list takes only the post ID not the whole ass post
            //This is reserved for Content lists
            List<int> PostsToDelete = new List<int>();

            //Posts Metadata
            //This dictionary is divided by section, like this we can register all posts when processing them
            //For instance, Main is the main posts list and Pools, are posts associated with pools;
            Dictionary<string, List<Metadata>> PostsMetadata = new Dictionary<string, List<Metadata>>();

            //This section process posts to send them to their respective lists

            /*
                Note that this section has some optional parts, but the process is the same for all.

                If the post has *ANY* association to a pool it won't be moved to the Type folder (SWF, Videos and such)
                instead, it will be moved on its respective pool folder, if more than a pool is associated with a post
                the post will be replaced with a reference/shortcut file....

                Honestly, i have no idea how i will do this last part..... but i will.

                Duplicates will be saved as REFFER file, those files are intended to be used with a Frontend for parsing them.

                Only time will tell.

                If the post does not fall onto any Type folder (images for instance), it will fall under the main list and
                parsed onto the pools accordingly.

                Note that the remaining posts that are left onto the main list after the process is done
                don't fall onto any Type folder (SWF, Video and Pools), hence why they are left on the main folder!
            */

            Console.WriteLine("Processing Posts");
            Console.WriteLine("Let's begin with posts files that contain the .SWF extensions");
            for(int s = 0; s < PostGallery.Count; s++)
            {
                //Gets the current posts
                var Current = PostGallery[s];

                //Checks if the extensions is SWF
                if(Current.File.FileExtension == "swf")
                {
                    //Checks if the post also has any association to a pool
                    if(Current.Pools.Count == 0)
                    {
                        //Add the post to the list
                        SWF_Posts.Add(Current);
                        //If not, he will be tagged to be removed anyway from the MAIN List
                        Console.WriteLine("This post is not associated with any pools, is safe to remove it from the main list");
                        PostsToDelete.Add(Current.Id);
                        Console.WriteLine("Occurence will be removed from the main list after the completion");
                    }
                    else
                    {
                        //If the post has any association to a pool
                        //It won't be tagged for deletion until he is associated to a pool
                        //Note that this posts has already been added to the SWF List
                        //So adding it to a Pool, would create a duplicate
                        //This is the step where we mark it to be removed from the pools
                        //So the SWF posts are kept in their own folder
                        //Removing ANY other occurence of this post inside any pool
                        Console.WriteLine($"This post is associated with ({Current.Pools.Count}) pools");
                        Console.WriteLine("It's not safe to remove it from the main list as it may compromise");
                        Console.WriteLine("The pool association process");
                    }
                }
                else
                {
                    Console.WriteLine($"Post at index {s} is does not contain a SWF file");
                }
            }
            
            //Assign video posts to the Video Folders
            if(Configuration.VideoOnFolders == true)
            {
                Console.WriteLine("Okay! Now let's see and separate posts that are video files");
                for(int s = 0; s < PostGallery.Count; s++)
                {
                    var Current = PostGallery[s];
                    if(Current.File.FileExtension == "webm")
                    {
                        if(Current.Pools.Count == 0)
                        {
                            Console.WriteLine("This video post is not associated with any pools, is safe to remove it from the main list");
                            Videos.Add(Current);
                            PostsToDelete.Add(Current.Id);
                            Console.WriteLine("Occurence will be removed from the main list after the completion");
                        }
                        else
                        {
                            Console.WriteLine($"This video post is associated with ({Current.Pools.Count}) pools");
                            Console.WriteLine("It's not safe to remove it from the main list as it may compromise");
                            Console.WriteLine("The pool association process");
                        }
                    }
                }

            }
            
            //Assign Pools to their own folder
            Console.WriteLine("Done! Now let's separate the posts by their pool Association");
            for(int s = 0; s < PostGallery.Count; s++)
            {
                var Current = PostGallery[s];
                if(Current.Pools.Count != 0)
                {
                    var AddedTo = 0;
                    for(int p = 0; p < Current.Pools.Count; p++)
                    {
                        
                        var CurrentPool = Current.Pools.ElementAt(p);
                        /* Console.WriteLine($"Fetching data for pool - {CurrentPool}");
                        var pool_data = iClient.GetPoolAsync(CurrentPool);
                        pool_data.Wait();
                        if(pool_data.IsCompleted)
                        {
                            var pdr = pool_data.Result;
                        }
                        */
                        if(Pools.ContainsKey(CurrentPool) == false)
                        {
                            Console.WriteLine("Adding first entry to the Dictionary");
                            Pools.Add(CurrentPool, new List<Post>());
                            Pools[CurrentPool].Add(Current);
                        }
                        else
                        {
                            Console.WriteLine("Adding entry to the dictionary");
                            Pools[CurrentPool].Add(Current);
                        }
                        AddedTo++;
                    }
                    Console.WriteLine($"The post with ID ({Current.Id}) has been added to {AddedTo} pools");
                    Console.WriteLine($"Current number of pools - {Pools.Count}");
                    PostsToDelete.Add(Current.Id);
                    Console.WriteLine("Occurence will be removed from the main list after the completion");

                    //If the pool count is bigger than 1
                    //We will need to register the duplicates for when saving the post on the pools folder
                    if(Current.Pools.Count > 1)
                    {
                        Console.WriteLine($"This post ({Current.Id}) is referenced by more than 1 pool");
                        Console.WriteLine("After the first associated pool, the post will marked as a duplicate");
                        Console.WriteLine("This means that for each file that is saved AFTER the first one (the original)");
                        Console.WriteLine("A symbolic link will be created aiming toward the original file... just like a shortcut");

                        PostPoolDuplicateData PDD = new PostPoolDuplicateData();
                        PDD.PostID = Current.Id;
                        for(int pd = 0; pd < Current.Pools.Count; pd++)
                        {
                            if(pd == 0)
                            {
                                Console.WriteLine("Registering ORIGINAL post association");
                                Console.WriteLine($"Post ({Current.Id}) first found on pool ({Current.Pools.ElementAt(pd)})");
                                PDD.Pools.Add(Current.Pools.ElementAt(pd), "Original");

                            }
                            else
                            {
                                Console.WriteLine("Registering DUPLICATE post association");
                                Console.WriteLine($"Post ({Current.Id}) also found on pool ({Current.Pools.ElementAt(pd)})");
                                PDD.Pools.Add(Current.Pools.ElementAt(pd), "Duplicate");
                            }
                        } 
                        var sID = Duplicates.FindIndex(x=>x.PostID == Current.Id);
                        if(sID == -1)
                        {
                            Console.WriteLine("No occurence of this post has been found on the duplicate list");
                            Console.WriteLine("Inserting it");
                            Duplicates.Add(PDD);
                        }              
                        else
                        {
                            Console.WriteLine("This post already exists on the Duplicate list");
                            Console.WriteLine("Honestly i don't think this should happen");
                            Console.WriteLine($"Index - {sID}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Post is only associated with one pool, no need to check for duplicates");
                    }
                }
                else
                {
                    Console.WriteLine($"Post at index {s}({Current.Id}) is does not contain association to any pool");
                }
            }
            
            //Remove duplicated posts (that have been place onto the SWF/Videos or Pools Lists) from the main list
            Console.WriteLine("Removing duplicates");
            Console.WriteLine($"Amount of Duplicates - {PostsToDelete.Count}");
            Console.WriteLine($"Current Number of Posts (Before deletion) - {PostGallery.Count}");
            for (int d = 0; d < PostsToDelete.Count; d++)
            {
                var CurrentDeletion = PostsToDelete[d];
                var index = PostGallery.FindIndex(x=> x.Id == CurrentDeletion);
                if(index != -1)
                {
                    Console.WriteLine($"Removing post with id ({CurrentDeletion}) on index ({index}) from main list"); 
                    PostGallery.RemoveAt(index);
                }
            }
            Console.WriteLine($"Current Number of Posts (After deletion) - {PostGallery.Count}");
            
            //Outputs the total amount of posts in all lists
            Console.WriteLine("Done");
            Console.WriteLine($"Number of Posts - {PostGallery.Count}");
            if(Pools.Count != 0)
            {
                for(int pc = 0; pc < Pools.Count; pc++)
                {
                    PPC += Pools.ElementAt(pc).Value.Count;
                }
                Console.WriteLine($"Number of Pools - {Pools.Count} (With a total of {PPC} posts within those {Pools.Count} pools)");
            }
            else
            {
                Console.WriteLine("No pools have been found!");
            }
            Console.WriteLine($"Number of SWF Posts - {SWF_Posts.Count}");
            Console.WriteLine($"Number of Video Posts - {Videos.Count}");
            Console.WriteLine($"Making a total of..... {PostGallery.Count + PPC + SWF_Posts.Count + Videos.Count} posts");

            Console.WriteLine("Preparing to save posts on their respective folders");

            var SWF_PATH = $"{OutputDir}/SWF/";
            var VIDEOS_PATH = $"{OutputDir}/VIDEOS/";
            var POOLS_PATH = $"{OutputDir}/POOLS/";

            Console.WriteLine("Creating Paths");
            if(!Directory.Exists(SWF_PATH))
            {
                Directory.CreateDirectory(SWF_PATH);
            }
            if(!Directory.Exists(VIDEOS_PATH) && Configuration.VideoOnFolders == true)
            {
                Directory.CreateDirectory(VIDEOS_PATH);
            }
            if(!Directory.Exists(POOLS_PATH))
            {
                Directory.CreateDirectory(POOLS_PATH);
            }

            Console.WriteLine("Begining Downloads\n");
            //Posts
            DownloadManager manager;
            if(PostGallery.Count > 0)
            {
                Console.WriteLine("Downloading Normal Posts\n");
                manager = new DownloadManager(PostGallery, OutputDir, ef);
                manager.Start();
                Console.WriteLine("Normal Posts downloaded\n");
            }
            else
            {
                Console.WriteLine("Weirdly.... There's no posts in the Post List\n");
            }
           

            //SWF Posts
            if(SWF_Posts.Count > 0)
            {
                Console.WriteLine("Downloading SWF Posts\n");
                manager = new DownloadManager(SWF_Posts, SWF_PATH, ef);
                manager.Start();
                Console.WriteLine("SWF Posts downloaded\n");
            }
            else
            {
                Console.WriteLine("No SWF posts to download\n");
            }
            
            //Videos
            if(Configuration.VideoOnFolders == true && Videos.Count > 0)
            {
                Console.WriteLine("Downloading Video Posts\n");
                manager = new DownloadManager(Videos, VIDEOS_PATH, ef);
                manager.Start();
                Console.WriteLine("Video Posts downloaded\n");
            }
            else
            {
                Console.WriteLine("Video separation disabled\n");
            }

            //Pools
            if(Pools.Count > 0)
            {
                //Fetch Pools Data and Create Directories
                Console.WriteLine("Parsing Pools and Creating Directories\n");
                for(int ps = 0; ps < Pools.Count; ps++)
                {
                    var p = FetchPoolAsync(iClient, Pools.ElementAt(ps).Key);
                    p.Wait();

                    if(p.IsCompleted)
                    {
                        var Pool = p.Result;
                        Console.WriteLine($"Current Pool:\nName: {Pool.Name}\nID: {Pool.Id}\nDescription: {Pool.Description}\n");
                        Console.WriteLine("Creating Directory");
                        var CurrentPoolDirectory = $"{POOLS_PATH}/{Pool.Id}";
                        if(!Directory.Exists(CurrentPoolDirectory))
                        {
                            Directory.CreateDirectory(CurrentPoolDirectory);
                            Console.WriteLine("Created!\n");
                        }
                        
                        //Discarded code
                        /* for(int ids = 0; ids < Pool.PostIds.Count; ids++)
                        {
                            var ID = Pool.PostIds.ElementAt(ids);
                            var dID = Duplicates.FindIndex(x=>x.PostID == ID);
                            if(dID != -1)
                            {
                                Console.WriteLine("Duplicate found, checking status");
                                var status = Duplicates.ElementAt(dID).Pools[Pool.Id];
                                if (status != "Original")
                                {
                                    Console.WriteLine("Duplicate confirmed");
                                    
                                    var ppID = Pools[Pool.Id].FindIndex(x=>x.Id == ID);
                                    if(ppID != -1)
                                    {
                                        var Post = Pools[Pool.Id].ElementAt(ppID);
                                        Console.WriteLine("Removing post from list entry");
                                        Pools[Pool.Id].RemoveAt(ppID);
                                        Console.WriteLine("Done!");
                                        Console.WriteLine("Creating Reffer File");
                                        var OGID = Duplicates.ElementAt(dID).Pools.Values.ToList().FindIndex(x=> x == "Original");
                                        var OGK = Duplicates.ElementAt(dID).Pools.Keys.ElementAt(OGID);


                                    }

                                }
                                else
                                {
                                    Console.WriteLine($"Post ({ID}) is the original");
                                }

                            }
                            else
                            {
                                Console.WriteLine($"No duplicates marked with ID - {ID}");
                            }
                        } */
                    }
                }
                Console.WriteLine("Done!\n");
                
                //Process duplicates
                if(Duplicates.Count > 0)
                {
                    Console.WriteLine("Checking for duplicates\n");
                    for(int d = 0; d < Duplicates.Count; d++)
                    {
                        //Create assign a duplicate to a variable
                        var dup = Duplicates.ElementAt(d);

                        //Iterates for each pool this duplicate is associated to
                        for(int p = 0; p < dup.Pools.Count; p++)
                        {
                            //Gets a key for each pool in the List
                            var k = dup.Pools.ElementAt(p).Key; //Key is also the Pool ID
                            var status = dup.Pools[k]; //Get the status of this DUPLICATE in the pool associated with the current key

                            //Checks if this Duplicate Entry is indeed a Duplicate or an original file
                            if(status != "Original")
                            {
                                Console.WriteLine("Duplicate Confirmed");
                                Console.WriteLine("Deleting from pool entry");
                                //Gets the index of this duplicate inside the Original Pool List
                                var pid = Pools[k].FindIndex(x=>x.Id == dup.PostID);
                                //Creates a post reference to get data later
                                var PostReference = Pools[k][pid];

                                //Removes this Duplicate from the original Pool List using the Current Key as identifier
                                Pools[k].RemoveAt(pid);

                                Console.WriteLine("Done!\n");

                                //Creating the reffer file
                                Console.WriteLine("Creating Reffer File");

                                //Creates the reffer file on the pool folder associated with the current key
                                //Said path was created in the previous step.
                                using (FileStream fs = File.Create($"{POOLS_PATH}/{k}/{dup.PostID}.reffer"))
                                {
                                    byte[] info = new UTF8Encoding(true).GetBytes($"RefferTo:'{dup.PostID}.{PostReference.File.FileExtension}'");
                                    // Add some information to the file.
                                    fs.Write(info, 0, info.Length);
                                    fs.Close(); //Remove in case of regret
                                }
                                Console.WriteLine("Done!\n");
                            }
                            else
                            {
                                //Nothin needs to be done here, as we delete duplicates and replace them with Reffer files.
                                //This is here merely to show that the Original file was found.
                                Console.WriteLine($"Post with ID - {dup.PostID} is marked as original in Pool - {k}\n");
                            }
                        }
                    }
                    Console.WriteLine("Finished!\n");
                }
                else
                {
                    Console.WriteLine("No duplicates to process!\n");
                }
                
                //Downloads all pools posts
                for(int psd = 0; psd < Pools.Count; psd++)
                {
                    Console.WriteLine($"Downloading posts associated with Pool - {Pools.ElementAt(psd).Key}");
                    manager = new DownloadManager(Pools.ElementAt(psd).Value, $"{POOLS_PATH}/{Pools.ElementAt(psd).Key}", ef);
                    manager.Start();
                    Console.WriteLine($"Posts from Pool - {Pools.ElementAt(psd).Key} - downloaded\n");
                }
                Console.WriteLine("Pools download finished\n");
            }
            else
            {
                Console.WriteLine("No Pools to download\n");
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("An Error has occured, following is the details of the exception");
            Console.WriteLine(e);
            ef.End = DateTime.Now.ToString("HH:mm");
            ef.ERROR.Add(e.ToString());
            LoggerInstance.Log(ef);
        }

        //Directory.GetFiles("", "*", SearchOption.AllDirectories)

        end:
        //Sets the time the task has finished
        ef.End = DateTime.Now.ToString("HH:mm");

        //Goodbye and log
        Console.WriteLine("Task completed");
        Console.WriteLine("Logging");
        //LoggerInstance.Log(ef);
        Console.WriteLine("Logging Completed");
    }
}