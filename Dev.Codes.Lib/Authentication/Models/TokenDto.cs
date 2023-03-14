using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Codes.Lib.Authentication.Models
{
    public class TokenDto : ResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string? AppId { get; set; }
    }
}
