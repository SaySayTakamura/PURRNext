
using Noppes.E621;
using PURRNext.LOG;

namespace PURRNext.DManager
{
    public class DownloadItem
    {
        public string STATUS;
        public Post? post;
        public DownloadItem()
        {
            STATUS = "UNDEFINED";
            post = null;
        }
    }
    public class DownloadManager
    {
        private List<DownloadItem> items; //List of items to be downloaded.
        private string DownloadPath; //The path to download the files with this instance.
        private int ParallelAmount; //Amount of parallel downloads.
        private EntryForm ef; //Entry form for errors.

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="post"> A list of posts which will be downloaded </param>
        /// <param name="path"> The path where the posts will be downloaded into </param>
        /// <param name="form"> An entry form for logging erros if needed </param>
        /// <param name="pA"> The number of posts that will be downloaded in parallel </param>
        public DownloadManager(List<Post> post, string path, EntryForm form,int pA = 4)
        {
            items = new List<DownloadItem>();
            DownloadPath = path;
            ParallelAmount = pA;
            ef = form;

            Console.WriteLine("Parsing posts to list and marking all as Pending\n");

            for(int i = 0; i < post.Count; i++)
            {
                DownloadItem item = new DownloadItem();
                item.STATUS = "Pending";
                item.post = post[i];
                items.Add(item);
            }
        }
        /// <summary>
        /// Clean function mean to be used internally
        /// </summary>
        void Cleanup()
        {
            Console.WriteLine("Cleaning UP\n");
            items.Clear();
            DownloadPath = string.Empty;
        }
        /// <summary>
        /// Starts the download process
        /// </summary>
        public void Start()
        {
            var done = false;
            List<DownloadItem> downloads = new List<DownloadItem>();

            while(!done)
            {
                Console.WriteLine("Getting pending posts\n");
                for(int i = 0; i < ParallelAmount; i++)
                {
                    var itemIndex = items.FindIndex(x=>x.STATUS == "Pending");
                    
                    if(itemIndex != -1)
                    {   
                        var item = items.ElementAt(itemIndex);
                        item.STATUS = "Downloading";
                        if(!downloads.Contains(item))
                        {
                            downloads.Add(item);
                        }
                        else
                        {
                            Console.WriteLine("Post is duplicated");
                            Exception e = new Exception("Post is duplicated");
                            throw e;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No pending item found");
                    }
                    
                }
                if(downloads.Count > 0)
                {
                    Console.WriteLine("Starting Downloads\n");

                    try
                    {
                        Task p1 = Task.Run(
                        async ()=>
                        {
                            var error = false;
                            var element = downloads.ElementAtOrDefault(0);
                            if(element != null)
                            {     
                                var ElementPost = element.post;
                                var filename = $"{ElementPost.Id}.{ElementPost.File.FileExtension}";
                                var currentAttempt = 0;

                                reattempt:
                                Console.WriteLine($"Attempt({currentAttempt}) for file: {filename}");
                                try
                                {
                                    using var client = new HttpClient();
                                    client.Timeout = TimeSpan.FromSeconds(180); //Extends the timeout duration to download files to 180 Secs - 3 Minutes
                                    using var s = await client.GetStreamAsync(ElementPost.File.Location);
                                    using var fs = new FileStream($"{DownloadPath}/{filename}", FileMode.OpenOrCreate);
                                    await s.CopyToAsync(fs);
                                    element.STATUS = "COMPLETED";
                                }
                                catch (Exception e)
                                {
                                    if(currentAttempt != 5)
                                    {
                                        Console.WriteLine($"An error has occurred\nError - {e}\n\nTrying Again");
                                        currentAttempt++;
                                        goto reattempt;
                                    }
                                    else
                                    {
                                        error = true;
                                        element.STATUS = "ERROR";
                                        goto end;
                                    }
                                }
                                end:
                                Console.WriteLine((error == false) ? $"Finished - {filename}" : "Finished with an error");
                            }
                            else
                            {
                                Console.WriteLine("Item is NULL, maybe we are already nearing the end of the list");
                            }
                            
                        }
                        );
                        Task p2 = Task.Run(
                        async ()=>
                        {
                            var error = false;
                            var element = downloads.ElementAtOrDefault(1);
                            if(element != null)
                            {     
                                var ElementPost = element.post;
                                var filename = $"{ElementPost.Id}.{ElementPost.File.FileExtension}";
                                var currentAttempt = 0;

                                reattempt:
                                Console.WriteLine($"Attempt({currentAttempt}) for file: {filename}");
                                try
                                {
                                    using var client = new HttpClient();
                                    client.Timeout = TimeSpan.FromSeconds(180); //Extends the timeout duration to download files to 180 Secs - 3 Minutes
                                    using var s = await client.GetStreamAsync(ElementPost.File.Location);
                                    using var fs = new FileStream($"{DownloadPath}/{filename}", FileMode.OpenOrCreate);
                                    await s.CopyToAsync(fs);
                                    element.STATUS = "COMPLETED";
                                }
                                catch (Exception e)
                                {
                                    if(currentAttempt != 5)
                                    {
                                        Console.WriteLine($"An error has occurred\nError - {e}\n\nTrying Again");
                                        currentAttempt++;
                                        goto reattempt;
                                    }
                                    else
                                    {
                                        error = true;
                                        element.STATUS = "ERROR";
                                        goto end;
                                    }
                                }
                                end:
                                Console.WriteLine((error == false) ? $"Finished - {filename}" : "Finished with an error");
                            }
                            else
                            {
                                Console.WriteLine("Item is NULL, maybe we are already nearing the end of the list");
                            }
                        }
                        );
                        Task p3 = Task.Run(
                        async ()=>
                        {
                            var error = false;
                            var element = downloads.ElementAtOrDefault(2);
                            if(element != null)
                            {     
                                var ElementPost = element.post;
                                var filename = $"{ElementPost.Id}.{ElementPost.File.FileExtension}";
                                var currentAttempt = 0;

                                reattempt:
                                Console.WriteLine($"Attempt({currentAttempt}) for file: {filename}");
                                try
                                {
                                    using var client = new HttpClient();
                                    client.Timeout = TimeSpan.FromSeconds(180); //Extends the timeout duration to download files to 180 Secs - 3 Minutes
                                    using var s = await client.GetStreamAsync(ElementPost.File.Location);
                                    using var fs = new FileStream($"{DownloadPath}/{filename}", FileMode.OpenOrCreate);
                                    await s.CopyToAsync(fs);
                                    element.STATUS = "COMPLETED";
                                }
                                catch (Exception e)
                                {
                                    if(currentAttempt != 5)
                                    {
                                        Console.WriteLine($"An error has occurred\nError - {e}\n\nTrying Again");
                                        currentAttempt++;
                                        goto reattempt;
                                    }
                                    else
                                    {
                                        error = true;
                                        element.STATUS = "ERROR";
                                        goto end;
                                    }
                                }
                                end:
                                Console.WriteLine((error == false) ? $"Finished - {filename}" : "Finished with an error");
                            }
                            else
                            {
                                Console.WriteLine("Item is NULL, maybe we are already nearing the end of the list");
                            }
                        }
                        );
                        Task p4 = Task.Run(
                        async ()=>
                        {
                            var error = false;
                            var element = downloads.ElementAtOrDefault(3);
                            if(element != null)
                            {     
                                var ElementPost = element.post;
                                var filename = $"{ElementPost.Id}.{ElementPost.File.FileExtension}";
                                var currentAttempt = 0;

                                reattempt:
                                Console.WriteLine($"Attempt({currentAttempt}) for file: {filename}");
                                try
                                {
                                    using var client = new HttpClient();
                                    client.Timeout = TimeSpan.FromSeconds(180); //Extends the timeout duration to download files to 180 Secs - 3 Minutes
                                    using var s = await client.GetStreamAsync(ElementPost.File.Location);
                                    using var fs = new FileStream($"{DownloadPath}/{filename}", FileMode.OpenOrCreate);
                                    await s.CopyToAsync(fs);
                                    element.STATUS = "COMPLETED";
                                }
                                catch (Exception e)
                                {
                                    if(currentAttempt != 5)
                                    {
                                        Console.WriteLine($"An error has occurred\nError - {e}\n\nTrying Again");
                                        currentAttempt++;
                                        goto reattempt;
                                    }
                                    else
                                    {
                                        error = true;
                                        element.STATUS = "ERROR";
                                        goto end;
                                    }
                                }
                                end:
                                Console.WriteLine((error == false) ? $"Finished - {filename}" : "Finished with an error");
                            }
                            else
                            {
                                Console.WriteLine("Item is NULL, maybe we are already nearing the end of the list");
                            }
                        }
                        );

                        Task.WhenAll(p1, p2, p3, p4).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"En error has occurred = {e}");
                        
                    }
                    Console.WriteLine("\nCurrent queue completed, proceeding for the next one!\n");
                    downloads.Clear();
                }
                else
                {
                    Console.WriteLine("\nNo posts on the download array\n");
                    Cleanup();
                    downloads.Clear();
                    done = true;
                }
            }
            Console.WriteLine("Download Finished\n");
        }
    }
}