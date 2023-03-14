using Dev.Codes.Lib.Database.Models;

namespace Dev.Codes.ConsoleApp.Models
{
    public class UserModel : ARepositoryItem
    {
        public string Name {get; set;}
        public string Email {get; set;}
    }
}