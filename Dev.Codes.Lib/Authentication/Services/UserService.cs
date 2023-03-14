using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev.Codes.Lib.Authentication.Models;
using Dev.Codes.Lib.Database.Contexts;
using Dev.Codes.Lib.Database.Interfaces;
using Dev.Codes.Lib.Database.Models;
using Dev.Codes.Lib.Ioc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Dev.Codes.Lib.Authentication.Services
{
    public class UserService
    {
        private readonly IMongoDbContext _repositoryContext;
        private readonly DatabaseInfo _databaseInfo;
        private readonly IMongoDbClient _mongoDbClient;
        
        public UserService(IConfiguration configuration)
        {
            _repositoryContext = IocContainer.Instance.Resolve<IMongoDbContext>("MongoDbContext");
            _mongoDbClient = IocContainer.Instance.Resolve<IMongoDbClient>();
            _databaseInfo = _databaseInfo = configuration.GetSection("DatabaseInfo").Get<DatabaseInfo>();
            _mongoDbClient.RegisterDbClient(_databaseInfo);
        }
        
        public async Task<bool> CreateUserAsync(UserModel userModel)
        {
            return await _repositoryContext.SaveItemAsync(_databaseInfo, userModel);
        }

        public async Task<bool> IsUserCanLogInAsync(LogInDto logInDto)
        {
            var emailFilter = Builders<UserModel>.Filter.Eq("Email", logInDto.Email);
            var passwordFilter = Builders<UserModel>.Filter.Eq("Password", logInDto.Password);
            var filter = Builders<UserModel>.Filter.And(emailFilter, passwordFilter);
            var userModel = await _repositoryContext.GetItemByFilterDefinitionAsync(_databaseInfo, filter);
            if (userModel != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsUserExist(string email)
        {
            var emailFilter = Builders<UserModel>.Filter.Eq("Email", email);
            var userModel = await _repositoryContext.GetItemByFilterDefinitionAsync(_databaseInfo, emailFilter);
            if (userModel != null) return true;
            return false;
        }

    }
}
