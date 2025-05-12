using Newtonsoft.Json;

namespace PURRNext.LOG
{
    public class EntryForm
    {
        public string TAG;
        public string Date;
        public string Start;
        public string End;
        public string LastKnownPath;
        public List<string> ERROR;
        public int DownloadedPosts;
        public int TotalPosts;
        public bool Success;
        public bool Logged;

        public EntryForm() { ERROR = new List<string>(); }
        
    }
    public class Logger
    {
        private string LogPath = "";
        List<EntryForm> entries;

        public Logger() { entries = new List<EntryForm>(); }

        public void WithLogPath(string logpath) { LogPath = logpath; }
        public void LoadMainFile()
        {
            using (StreamReader r = new StreamReader(LogPath))
            {
                string json = r.ReadToEnd();
                List<EntryForm> items = JsonConvert.DeserializeObject<List<EntryForm>>(json);
                entries = items;
                r.Close();//Remove in case of regret
            }
        }

        public void Log(EntryForm form)
        {
            if (form != null)
            {
                if(form.ERROR.Count == 0)
                {
                    form.ERROR.Add("No error");
                }

                LoadMainFile();

                entries.Add(new EntryForm()
                {
                    TAG = form.TAG,
                    Date = form.Date,
                    Start = form.Start,
                    End = form.End,
                    ERROR = form.ERROR,
                    DownloadedPosts = form.DownloadedPosts,
                    LastKnownPath = form.LastKnownPath,
                    TotalPosts = form.TotalPosts,
                    Success = form.Success,
                    Logged = form.Logged
                });


                //open file stream
                using (StreamWriter file = File.CreateText(LogPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, entries);
                    file.Close();//Remove in case of regret
                }
            }
        }
    }
}
