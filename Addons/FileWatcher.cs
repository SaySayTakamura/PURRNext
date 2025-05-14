using System.Security.Cryptography;


namespace PURRNext.Watcher
{
    public class FileWatcher
    {
        public string Path;
        public byte[] current_hash;
        public byte[] new_hash;

        public FileWatcher(String file) 
        {
            Path = file;
            var sha1 = SHA1.Create();
            using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                current_hash = sha1.ComputeHash(stream);
            }
        }
        public bool OnFileChange(bool t)
        {

            return true;
        }
        public async void Run()
        {
            //Taken from:
            //https://stackoverflow.com/questions/26656236/scheduling-task-for-future-execution
            await Task.Delay(TimeSpan.FromMinutes(30));
            var sha1 = SHA1.Create();
            using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                new_hash = sha1.ComputeHash(stream);

                if (current_hash == new_hash)
                {
                    Console.Write("File SHA1 CHECKSUM has not been changed. No Changes detected");
                    OnFileChange(false);
                }
                else if (current_hash != new_hash)
                {
                    Console.WriteLine("SHA1 CHECKSUM doesn't match previous stored values! Changes detected");
                    OnFileChange(true);
                    current_hash = new_hash;
                    //Check this if you have doubts how to Clear a byte array
                    //https://stackoverflow.com/questions/6546114/empty-elements-in-c-sharp-byte-array
                    new_hash = [];

                }
            }
        }
    }
}
