using Dev.Codes.Lib.Authentication.Models;
using Dev.Codes.Lib.Database.Interfaces;
using Dev.Codes.Lib.Database.Models;
using Dev.Codes.Lib.Ioc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Dev.Codes.Lib.Authentication.Repository
{
    public class AuthRepository
    {
        private readonly DatabaseInfo _databaseInfo;
        private readonly IMongoDbClient _mongoDbClient;
        private readonly IMongoDbContext _context;

        public AuthRepository(IConfiguration configuration)
        {
            _databaseInfo = configuration.GetSection("DatabaseInfo").Get<DatabaseInfo>();
            _mongoDbClient = IocContainer.Instance.Resolve<IMongoDbClient>("MongoDbClient");
            _context = IocContainer.Instance.Resolve<IMongoDbContext>("MongoDbContext");
            _mongoDbClient.RegisterDbClient(_databaseInfo);
        }

        public async Task<bool> SaveTokenModelAsync(TokenModel tokenModel)
        {
           return await _context.SaveItemAsync(_databaseInfo, tokenModel);
        }

        public async Task<TokenModel> GetTokenModelByRefreshTokenAsync(string refreshToken)
        {
            var tokenModel = await _context.GetItemByIdAsync<TokenModel>(_databaseInfo, refreshToken);
            return tokenModel;
        }

        public async Task<bool> DeleteAllTokenByEmailAsync(string email)
        {
            var filter = Builders<TokenModel>.Filter.Eq("Email", email);
            return await _context.DeleteItemsByFilterDefinitionAsync(_databaseInfo, filter);
        }

        public async Task<bool> DeleteAllTokenByAppIdAsync(string appId)
        {
            var filter = Builders<TokenModel>.Filter.Eq("AppId", appId);
            return await _context.DeleteItemsByFilterDefinitionAsync(_databaseInfo, filter);
        }
    }
}