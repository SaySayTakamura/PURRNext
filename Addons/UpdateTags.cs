
using System.Reflection.Metadata;
using Newtonsoft.Json;
using Noppes.E621;
using PURRNext.LOG;

namespace PURRNext.TUpdater
{
    public class UpdateTag
    {
        public string TAG;
        public string LastKnownPath;
        public int LastKnownPostID;
    }

    public class TagUpdater
    {
        private string ListPath = "";
        private List<UpdateTag> tgs;

        public TagUpdater()
        {
            tgs = new List<UpdateTag>();
        }
        public void WithListPath(string Path)
        {
            ListPath = Path;
        }
        public void LoadTagList()
        {
            using (StreamReader r = new StreamReader("updates.json"))
            {
                string json = r.ReadToEnd();
                List<UpdateTag> items = JsonConvert.DeserializeObject<List<UpdateTag>>(json);
                r.Close();

                tgs = items;
            }
        }
        public async Task<bool> UpdateTagsAsync(Logger l)
        {
            var e621Client = new E621ClientBuilder()
            .WithUserAgent("PURRNext- An E621 CLI BACKEND (WIP) - UPDATER FUNCTION", "0.0.1", "EdgarTakamura", "Bluesky")
            .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
            .WithRequestInterval(E621Constants.MinimumRequestInterval)
            .WithTimeout(TimeSpan.FromMilliseconds(10000))
            .Build();

            var UpdatedList = tgs;
            var changed = false;
            var updatedtags = 0;

            EntryForm ef = new EntryForm();
            ef.TAG = "TAG-UPDATER";
            ef.Date = DateTime.Now.ToString("d");
            ef.Start = DateTime.Now.ToString("HH:mm");
            ef.TotalPosts = -1;
            ef.DownloadedPosts = -1;

            for (int i = 0; i < tgs.Count; i++)
            {
                Console.WriteLine($"TAG - {tgs[i].TAG}\nLast Know Path - '{tgs[i].LastKnownPath}'\nLast Known Post ID - {tgs[i].LastKnownPostID}");
                var posts = new List<Post>();
                try
                {
                    var ps = await e621Client.GetPostsAsync(id: tgs[i].LastKnownPostID, position: Position.After, tags: tgs[i].TAG);
                    posts = ps.ToList();
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown - {ex.ToString()}");
                    ef.End = DateTime.Now.ToString("HH:mm");
                    ef.ERROR.Add(ex.ToString());
                    ef.Success = false;
                    l.Log(ef);
                    return false;
                }

                if (posts.Count != 0)
                {
                    changed = true;
                    updatedtags++;
                    Console.WriteLine($"New posts found for the tag - {tgs[i].TAG}\nNew Posts - {posts.Count}");

                    for (int p = 0; p < posts.Count; p++)
                    {
                        var post = posts.ElementAt(p);
                        //This checks for a file in the posts... Why would not have one???
                        if (post.File != null)
                        {
                            if (Directory.Exists(tgs[i].LastKnownPath))
                            {
                                var ExtensionID = "." + post.File.FileExtension;
                                var path = $"{tgs[i].LastKnownPath}/";
                                Console.WriteLine($"Saving[I:{p}][{post.File.FileExtension.ToUpper()}]: {path}{post.Id}{ExtensionID}");

                                try
                                {
                                    using var client = new HttpClient();
                                    using var s = await client.GetStreamAsync(post.File.Location);
                                    using var fs = new FileStream($"{path}/{post.Id}.{post.File.FileExtension}", FileMode.OpenOrCreate);
                                    await s.CopyToAsync(fs);
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine($"Exception thrown - {ex.ToString()}");
                                    ef.End = DateTime.Now.ToString("HH:mm");
                                    ef.ERROR.Add(ex.ToString());
                                    ef.Success = false;
                                    l.Log(ef);
                                    return false;
                                }

                            }
                            else
                            {
                                Console.WriteLine("Last Known Path does not exist or is unreachable....");
                                Console.WriteLine("Quiting....");

                                ef.End = DateTime.Now.ToString("HH:mm");
                                ef.ERROR.Add("Last Known Path does not exist");
                                ef.Success = false;
                                l.Log(ef);
                                Environment.Exit(-1);
                            }
                            if(p == posts.Count-1)
                            {
                                var Latestpost = posts.ElementAt(0).Id;
                                Console.WriteLine($"Updating Variables - \nLast known post is now - {Latestpost}");
                                UpdatedList[i].LastKnownPostID = Latestpost;
                            }
                        }
                    }
                }
                else
                {
                    var tt = tgs[i].TAG;
                    var bl = " -cub -diaper -feces -scat -fart";
                    tt = tt.Replace(bl, "");
                    Console.WriteLine($"No new posts found for this tag({tt}).\n");
                }
            }
            if (changed)
            {
                Console.WriteLine($"Tags updated. {updatedtags} tags were updated.");
                Console.WriteLine("Parsing and updating the JSON file");
                try
                {
                    //open file stream
                    using (StreamWriter file = File.CreateText("updates.json"))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //serialize object directly into file stream
                        serializer.Formatting = Formatting.Indented;
                        serializer.Serialize(file, UpdatedList);
                    }
                    ef.End = DateTime.Now.ToString("HH:mm");
                    ef.Success = true;
                    l.Log(ef);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown - {ex.ToString()}");
                    ef.End = DateTime.Now.ToString("HH:mm");
                    ef.ERROR.Add(ex.ToString());
                    ef.Success = false;
                    l.Log(ef);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("No TAGS were updated");
                ef.End = DateTime.Now.ToString("HH:mm");
                ef.ERROR.Add("No tags updated");
                ef.Success = true;
                l.Log(ef);
            }
            return true;
        }
    }
}