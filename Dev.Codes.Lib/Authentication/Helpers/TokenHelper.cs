using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dev.Codes.Lib.Authentication.Helpers
{
    public class TokenHelper
    {
        public TokenHelper(IConfiguration configuration)
        {
            
        }

        public string GenerateJwtToken(string secretKey, string issuer, string audience, int expiredTimeInSec, List<Claim> claims = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expiredTime = DateTime.Now.AddSeconds(expiredTimeInSec);
            var tokenOptions = new JwtSecurityToken(
                issuer : issuer, 
                audience : audience, 
                claims : claims, 
                expires : expiredTime, 
                signingCredentials: signingCredentials
            );
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return token;
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
        
        public List<Claim> GetClaims(string token)
        {
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtSecurityToken.Claims.ToList();
        }
        
        public string GetClaimType(Claim claim)
        {
            return claim.Type;
        }
        
        public string GetClaimValue(Claim claim)
        {
            return claim.Value;
        }

        public bool IsTokenValid(string token, TokenValidationParameters validateParameters)
        {
            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, validateParameters, out var validatedToken);
                Console.WriteLine($"Token Valid : {token}\n");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Token InValid : {token}\n");
                return false;
            }
        }

        public bool IsExpired(string token)
        {
            try
            {
                var securityToken = new JwtSecurityToken(token);
                bool isExpired = securityToken.ValidTo < DateTime.UtcNow;
                string message = "Un Expired";
                if (isExpired) message = "Expired";
                Console.WriteLine($"Token {message}. Token : {token}\n");
                return isExpired;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Token error : {token}\n");
                return false;
            }
        }
    }
}