using Noppes.E621;

namespace PURRNext.LoginData
{
    class LoginInputData
    {
        public string Username  {get; set;} = "";
        public string Password {get; set;} = "";
        public string MagicNumber {get; set;} = "";

        public LoginInputData(string User, string Pass, string Number)
        {
            Username = User;
            Password = Pass;
            MagicNumber = Number;
        }
    }
    
}