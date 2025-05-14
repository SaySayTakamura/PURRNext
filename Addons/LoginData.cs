using Noppes.E621;

namespace PURRNext.LoginData
{
    class LoginInputData
    {
        private string Username = "";
        private string Password = "";

        public LoginInputData(string User, string Pass)
        {
            Username = User;
            Password = Pass;
        }
        public string GetUsername()
        {
            return Username;
        }
        public string GetPassword()
        {
            return Password;
        }
    }
    
}