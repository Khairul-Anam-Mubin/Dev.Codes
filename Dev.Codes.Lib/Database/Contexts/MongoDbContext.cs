using System.Composition;
using Dev.Codes.Lib.Database.DbClients;
using MongoDB.Driver;
using Dev.Codes.Lib.Database.Interfaces;
using Dev.Codes.Lib.Database.Models;
using Dev.Codes.Lib.Ioc;
using Newtonsoft.Json;

namespace Dev.Codes.Lib.Database.Contexts
{
    [Export("MongoDbContext", typeof(IMongoDbContext))]
    [Shared]
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDbClient _mongoDbClient;
        
        public MongoDbContext()
        {
            _mongoDbClient = IocContainer.Instance.Resolve<IMongoDbClient>();
        }

        public async Task<bool> SaveItemAsync<T>(DatabaseInfo databaseInfo, T item) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var filter = Builders<T>.Filter.Eq("Id", item.GetId());
                await collection.ReplaceOneAsync(filter, item, new ReplaceOptions {
                    IsUpsert = true
                });
                Console.WriteLine($"Successfully Save Item : {JsonConvert.SerializeObject(item)}\n");
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine($"Problem Save Item : {JsonConvert.SerializeObject(item)}\n");
                return false;
            }
        }

        public async Task<bool> DeleteItemByIdAsync<T>(DatabaseInfo databaseInfo, string id) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var filter = Builders<T>.Filter.Eq("Id", id);
                var res = await collection.DeleteOneAsync(filter);
                Console.WriteLine($"Successfully Item Deleted, Id: {JsonConvert.SerializeObject(id)}\n");
                return res != null && res.DeletedCount > 0;
            }
            catch (Exception)
            {
                Console.WriteLine($"Problem Delete Item, Id : {JsonConvert.SerializeObject(id)}\n");
                return false;
            }
        }

        public async Task<T> GetItemByIdAsync<T>(DatabaseInfo databaseInfo, string id) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var filter = Builders<T>.Filter.Eq("Id", id);
                var items = await collection.FindAsync<T>(filter);
                var item = await items.FirstOrDefaultAsync<T>();
                Console.WriteLine($"Successfully Get Item : {JsonConvert.SerializeObject(item)}\n");
                return item;
            }
            catch (Exception)
            {
                Console.WriteLine($"Problem Get Item, id : {id}\n");
                return null;
            }
        }

        public async Task<List<T>> GetItemsAsync<T>(DatabaseInfo databaseInfo) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var filter = Builders<T>.Filter.Empty;
                var itemsCursor = await collection.FindAsync<T>(filter);
                var items = await itemsCursor.ToListAsync<T>();
                Console.WriteLine($"Successfully Get items, Count: {JsonConvert.SerializeObject(items.Count)}\n");
                return items;
            }
            catch (Exception)
            {
                Console.WriteLine($"Problem Get Items\n");
                return null;
            }
        }

        public async Task<T> GetItemByFilterDefinitionAsync<T>(DatabaseInfo databaseInfo, FilterDefinition<T> filterDefinition) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var items = await collection.FindAsync<T>(filterDefinition);
                var item = await items.FirstOrDefaultAsync<T>();
                 Console.WriteLine($"Successfully Get Item by filter : {JsonConvert.SerializeObject(item)}\n");
                return item;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problem Get Item by fiter \n");
                return null;
            }
        }

        public async Task<bool> DeleteItemsByFilterDefinitionAsync<T>(DatabaseInfo databaseInfo, FilterDefinition<T> filterDefinition) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var res = await collection.DeleteManyAsync(filterDefinition);
                Console.WriteLine($"Successfully Delete Items, count : {JsonConvert.SerializeObject(res?.DeletedCount)}\n");
                return res != null && res.DeletedCount > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problem Delete Items by fiter \n");
                return false;
            }
        }

        public async Task<List<T>> GetItemsByFilterDefinitionAsync<T>(DatabaseInfo databaseInfo, FilterDefinition<T> filterDefinition) where T : class, IRepositoryItem
        {
            try
            {
                var collection = _mongoDbClient.GetCollection<T>(databaseInfo);
                var itemsCursor = await collection.FindAsync<T>(filterDefinition);
                var items = await itemsCursor.ToListAsync();
                Console.WriteLine($"Successfully Get Items by filter count : {JsonConvert.SerializeObject(items.Count)}\n");
                return items;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Problem Get Items by filter \n");
                return null;
            }
        }
    }
}