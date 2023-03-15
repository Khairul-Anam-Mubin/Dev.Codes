using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dev.Codes.Lib.Authentication.Helpers;
using Dev.Codes.Lib.Authentication.Models;
using Dev.Codes.Lib.Authentication.Repository;
using Dev.Codes.Lib.Ioc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Dev.Codes.Lib.Authentication.Constants;
using Dev.Codes.Lib.EmailService.Interfaces;
using Dev.Codes.Lib.EmailService.Models;
using Microsoft.IdentityModel.Tokens;

namespace Dev.Codes.Lib.Authentication.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly TokenHelper _tokenHelper;
        private readonly AuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public AuthService()
        {
            _configuration = IocContainer.Instance.GetConfiguration();
            _userService = IocContainer.Instance.Resolve<UserService>();
            _tokenHelper = IocContainer.Instance.Resolve<TokenHelper>();
            _authRepository = IocContainer.Instance.Resolve<AuthRepository>();
            _emailSender = IocContainer.Instance.Resolve<IEmailSender>();
        }

        public async Task<ResponseDto> CanRegisterAsync(UserModel userModel)
        {
            if (await _userService.IsUserExist(userModel.Email))
            {
                return await Task.FromResult(new ResponseDto
                {
                    Message = "User already exist",
                    Status = ResponseStatus.Failed
                });
            }
            return await Task.FromResult(new ResponseDto
            {
                Message = "User can register",
                Status = ResponseStatus.Success
            });
        }

        public async Task<ResponseDto> RegisterAsync(UserModel userModel)
        {
            userModel.CreateGuidId();
            if (await _userService.CreateUserAsync(userModel))
            {
                var confirmationMessage = new Message
                {
                    To = new List<string>{userModel.Email},
                    Subject = "Account verify",
                    IsHtmlContent = true,
                    Content = $"<h1>Account registration successfully.</h1><br><button><a href=\"http://localhost:7038/api/auth/verify-account/{userModel.Id}\">Click to verify account</a></button><br><br><p>Thanks</p>"
                };
                await _emailSender.SendEmailAsync(confirmationMessage);
                return await Task.FromResult(new ResponseDto
                {
                    Message = "Register successfully",
                    Status = ResponseStatus.Success
                });
            }
            return await Task.FromResult(new ResponseDto
            {
                Message = "Register Error",
                Status = ResponseStatus.Failed
            });
        }

        public async Task<ResponseDto> LogOutAsync(LogOutDto logOutDto)
        {
            if (await RevokeAllTokenByAppIdAsync(logOutDto.AppId))
            {
                return new ResponseDto
                {
                    Status = ResponseStatus.Success,
                    Message = "Logged out successfully"
                };
            }
            return new ResponseDto
            {
                Status = ResponseStatus.Failed,
                Message = "Logout error"
            };
        }
        
        public async Task<bool> RevokeAllTokenByAppIdAsync(string appId)
        {
            return await _authRepository.DeleteAllTokenByAppIdAsync(appId);
        }

        public TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,

                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(_configuration["JWT:SecretKey"]))
            };
        }

        public async Task<ResponseDto> CanGetRefreshTokenAsync(TokenDto tokenDto)
        {
            if (_tokenHelper.IsTokenValid(tokenDto.AccessToken, GetTokenValidationParameters()) == false)
            {
                return new ResponseDto
                {
                    Status = ResponseStatus.Failed,
                    Message = "Access token not valid"
                };
            }

            if (_tokenHelper.IsExpired(tokenDto.AccessToken) == false)
            {
                return new ResponseDto
                {
                    Status = ResponseStatus.Ignored,
                    Message = "Access token not expired yet"
                };
            }

            var tokenModel = await _authRepository.GetTokenModelByRefreshTokenAsync(tokenDto.RefreshToken);

            if (tokenModel == null || tokenModel.AccessToken != tokenDto.AccessToken)
            {
                return new ResponseDto
                {
                    Status = ResponseStatus.Failed,
                    Message = "Refresh or Access Token error"
                };
            }
            
            var email = GetEmailByAccessToken(tokenDto.AccessToken);
            var appId = tokenDto.AppId;

            if (tokenModel.AppId != appId)
            {
                return new ResponseDto
                {
                    Status = ResponseStatus.Failed,
                    Message = "AppId problem"
                };
            }

            if (tokenModel.Expired == true)
            {
                await RevokeAllTokenByEmailAsync(email);
                return new ResponseDto
                {
                    Status = ResponseStatus.Failed,
                    Message = "Suspicious Token refresh attempt"
                };
            }

            tokenModel.Expired = true;
            await _authRepository.SaveTokenModelAsync(tokenModel);
            
            return new ResponseDto
            {
                Status = ResponseStatus.Success,
                Message = "Refresh token can get"
            };
        }

        public async Task<TokenDto> GetRefreshTokenAsync(TokenDto tokenDto)
        {
            var email = GetEmailByAccessToken(tokenDto.AccessToken);
            var appId = tokenDto.AppId;

            var refreshTokenModel = await GenerateTokenModelAsync(email, appId);
            await _authRepository.SaveTokenModelAsync(refreshTokenModel);
            
            var newTokenDto = refreshTokenModel.ToTokenDto();
            newTokenDto.Status = ResponseStatus.Success;
            newTokenDto.Message = "Token Generated";

            return newTokenDto;
        }

        public string GetEmailByAccessToken(string accessToken)
        {
            var claims = _tokenHelper.GetClaims(accessToken);
            var emailClaim = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email);
            if (emailClaim == null) return string.Empty;
            return _tokenHelper.GetClaimValue(emailClaim);
        }

        public async Task<bool> RevokeAllTokenByEmailAsync(string email)
        {
            return await _authRepository.DeleteAllTokenByEmailAsync(email);
        }

        public async Task<TokenDto> LogInAsync(LogInDto loginDto)
        {
            var tokenModel = await GenerateTokenModelAsync(loginDto.Email, loginDto.AppId);
            
            await _authRepository.SaveTokenModelAsync(tokenModel);

            return tokenModel.ToTokenDto();
        }

        public async Task<ResponseDto>CanLogInAsync(LogInDto logInDto)
        {
            var response = new ResponseDto();
            
            if (string.IsNullOrEmpty(logInDto.Email) || string.IsNullOrEmpty(logInDto.Password))
            {
                response.Status = ResponseStatus.Failed;
                response.Message = "Email or Password not set";
                return response;
            }

            var canLogIn = await _userService.IsUserCanLogInAsync(logInDto);

            if (canLogIn == false)
            {
                response.Status = ResponseStatus.Failed;
                response.Status = "Email or Password not matched";
                return response;
            }

            response.Status = ResponseStatus.Success;
            response.Message = "Login Successfully";

            return response;
        }

        public async Task<TokenModel> GenerateTokenModelAsync(string email, string appId)
        {
            var claims = await GetClaimsByEmailAsync(email);
            
            var accessToken = _tokenHelper.GenerateJwtToken(_configuration["JWT:SecretKey"], _configuration["JWT:Issuer"], _configuration["JWT:Audience"], int.Parse(_configuration["JWT:ExpirationTimeInSec"]), claims);
            var refreshToken = _tokenHelper.GenerateRefreshToken();
            
            var tokenModel = new TokenModel
            {
                AppId = appId,
                AccessToken = accessToken,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };
            tokenModel.CreateGuidId();

            return tokenModel;
        }

        public async Task<List<Claim>> GetClaimsByEmailAsync(string email)
        {
            var claims = new List<Claim>(); // will get from user access model later
            
            // custom claims
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()));
            
            return await Task.FromResult(claims);
        }
    }
}
