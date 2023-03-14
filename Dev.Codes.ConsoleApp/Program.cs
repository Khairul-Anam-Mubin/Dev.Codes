using Dev.Codes.Lib.Database.Models;
using Dev.Codes.Lib.Database.Contexts;
using Dev.Codes.Lib.Database.DbClients;
using Dev.Codes.Lib.Database.Interfaces;
using Newtonsoft.Json;
using Dev.Codes.Lib.Ioc;
using Dev.Codes.ConsoleApp.Models;

namespace Dev.Codes.ConsoleApp
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Console.WriteLine("Hello");
            // var tokenService = new TokenService();
            // var token = tokenService.GenerateJwtToken("lamia", "mubin", "mubin", 10);
            // tokenService.GetClaims(token);
            //IocContainer.Instance.AddAllAssemblies();
            var databaseInfo = new DatabaseInfo();
            databaseInfo.ConnectionString = "mongodb://localhost:27017";
            databaseInfo.DatabaseName = "TestLibDb";

            var mongoDbClient = IocContainer.Instance.Resolve<IMongoDbClient>("MongoDbClient");
            var dbClient = mongoDbClient.RegisterDbClient(databaseInfo);
            var mongoDbContext = IocContainer.Instance.Resolve<IMongoDbContext>("MongoDbContext");

            var user = new UserModel();
            user.CreateGuidId();
            user.Name = "Mubin";
            user.Email = "anam.mubin1999@gmail.com";

            IRedisCacheClient redis = IocContainer.Instance.Resolve<IRedisCacheClient>("RedisCacheClient");
            var redisDb = new DatabaseInfo();
            redisDb.DatabaseName = "RedisDb";
            redisDb.ConnectionString = "127.0.0.1:6379";
            databaseInfo = redisDb;
            redis.RegisterDbClient(redisDb);

            var redisDbContext = IocContainer.Instance.Resolve<IRedisCacheContext>("RedisCacheContext");
            await redisDbContext.InsertItemAsync<UserModel>(redisDb, user);
            
            var redisUser = await redisDbContext.GetItemByIdAsync<UserModel>(redisDb, user.Id);
            var ser = JsonConvert.SerializeObject(redisUser);
            Console.WriteLine($"Redis User: {ser}");

            var insert = await redisDbContext.InsertItemAsync<UserModel>(databaseInfo, (UserModel)user);
            Console.WriteLine($"Redis Insert : {insert}");
            
            var getAndUpdateId = user.GetId();
            var get = await redisDbContext.GetItemByIdAsync<UserModel>(databaseInfo, getAndUpdateId);
            var getString = JsonConvert.SerializeObject(get);
            Console.WriteLine($"Redis Get : {getString}");
            
            get.Name = "Araf";
            get.Email = "aman.araf@gmail.com";
            var update = await redisDbContext.UpdateItemAsync<UserModel>(databaseInfo, get);
            var updateString = JsonConvert.SerializeObject(get);
            Console.WriteLine($"Redis Update : {update}");
            Console.WriteLine($"Redis Update : {updateString}");

            var deleteId = user.GetId();
            var delete = await redisDbContext.DeleteItemByIdAsync<UserModel>(databaseInfo, deleteId);
            Console.WriteLine($"Redis Delete : {delete}");
            var contexts = IocContainer.Instance.ResolveMany<IRedisCacheContext>().ToArray();
            Console.WriteLine($"Contexts Count {contexts.Count()}");
            foreach (var context in contexts)
            {
                Console.WriteLine($"{context.GetType()}");
            }
        }
    }
}