using Dev.Codes.Lib.Database.Models;
namespace Dev.Codes.Lib.Authentication.Models
{
    public class TokenModel : ARepositoryItem
    {
        public string AppId {get; set;}
        public string AccessToken {get; set;}
        public string Email {get; set;}
        public DateTime CreatedAt {get; set;}
        public bool Expired {get; set;}
        
        public TokenDto ToTokenDto()
        {
            return new TokenDto
            {
                AccessToken = AccessToken,
                RefreshToken = Id
            };
        }
    }
}