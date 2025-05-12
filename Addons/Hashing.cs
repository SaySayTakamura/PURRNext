using System.Security.Cryptography;
using System.Text;


namespace PURR.Crypto.Hash
{
    internal class Hashing
    {
        //Both private methods taken from:
        //Link: https://stackoverflow.com/questions/3984138/hash-string-in-c-sharp
        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string ReduceHashSize(string input)
        {
            //Taken from:
            //https://stackoverflow.com/questions/61000130/c-sharp-hash-string-into-short-number
            string rs = (BitConverter.ToUInt32(Encoding.UTF8.GetBytes(input), 0) % 1000000).ToString();

            return rs;
        }

        //https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/compute-hash-values
        public static string SetHash(string inputString, bool reduce = true)
        {
            string r = "";

            string hr = GetHashString(inputString);
            r = hr;
            if (reduce == true)
            {
                string rs = ReduceHashSize(hr);
                r = rs;
            }
            return r;
        }

        
    }
}
