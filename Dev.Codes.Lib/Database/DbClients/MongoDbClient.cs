using MongoDB.Driver;
using System.Composition;
using Dev.Codes.Lib.Database.Models;
using Dev.Codes.Lib.Database.Interfaces;

namespace Dev.Codes.Lib.Database.DbClients
{
    [Export("MongoDbClient", typeof(IMongoDbClient))]
    [Shared]
    public class MongoDbClient : IMongoDbClient
    {
        private readonly Dictionary<string, MongoClient> _dbClients;

        public MongoDbClient()
        {
            _dbClients = new Dictionary<string, MongoClient>();
        }

        public MongoClient RegisterDbClient(DatabaseInfo databaseInfo)
        {
            if (_dbClients.ContainsKey(databaseInfo.ConnectionString))
            {
                Console.WriteLine($"{databaseInfo.ConnectionString} Already Registered ConnectionString.");
                return _dbClients[databaseInfo.ConnectionString];
            }
            
            try
            {
                var client = new MongoClient(databaseInfo.ConnectionString);
                _dbClients.Add(databaseInfo.ConnectionString, client);
                return client;
            }
            catch (Exception)
            {
                var message = $"Client Creation Error. Connection string {databaseInfo.ConnectionString}";
                Console.WriteLine(message);
                throw new Exception(message);
            }
        }

        public MongoClient GetDbClient(DatabaseInfo databaseInfo)
        {
            return GetDbClient(databaseInfo.ConnectionString);
        }

        public MongoClient GetDbClient(string connectionString)
        {
            if (_dbClients.ContainsKey(connectionString))
            {
                return _dbClients[connectionString];
            }
            var message = $"Client not exist. Connection string {connectionString}";
            Console.WriteLine(message);
            return null;
        }

        public IMongoDatabase GetDatabase(DatabaseInfo databaseInfo)
        {
            try
            {
                var client = GetDbClient(databaseInfo);
                var database = client.GetDatabase(databaseInfo.DatabaseName);
                return database;
            }
            catch (Exception)
            {
                Console.WriteLine("Get Database Error");
                return null;
            }
        }

        public IMongoCollection<T> GetCollection<T>(DatabaseInfo databaseInfo)
        {
            try
            {
                var database = GetDatabase(databaseInfo);
                return database.GetCollection<T>(typeof(T).Name);
            }
            catch (Exception)
            {
                Console.WriteLine("Get Collection Error");
                return null;
            }
        }
    }
}