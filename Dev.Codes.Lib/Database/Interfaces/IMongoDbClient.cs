using Dev.Codes.Lib.Database.Models;
using MongoDB.Driver;

namespace Dev.Codes.Lib.Database.Interfaces
{
    public interface IMongoDbClient
    {
        MongoClient RegisterDbClient(DatabaseInfo databaseInfo);
        MongoClient GetDbClient(DatabaseInfo databaseInfo);
        MongoClient GetDbClient(string connectionString);
        IMongoDatabase GetDatabase(DatabaseInfo databaseInfo);
        IMongoCollection<T> GetCollection<T>(DatabaseInfo databaseInfo);
    }
}