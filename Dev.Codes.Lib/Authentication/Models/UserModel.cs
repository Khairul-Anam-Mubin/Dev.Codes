using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev.Codes.Lib.Database.Models;

namespace Dev.Codes.Lib.Authentication.Models
{
    public class UserModel : ARepositoryItem
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
