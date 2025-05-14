using Noppes.E621;

namespace PURRNext.LoginData
{
    class LoginInputData
    {
        public string Username = "";
        public string Password = "";

        public LoginInputData(string User, string Pass)
        {
            Username = User;
            Password = Pass;
        }
    }
    
}