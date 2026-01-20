using Dapper;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Data.SqlClient;

namespace PurchaseSystem.Preheat
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Redis数据预热工具 ===");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // 1. 连接Redis
            var redisConnection = config.GetConnectionString("RedisConnection");
            var redis = ConnectionMultiplexer.Connect(redisConnection);
            var db = redis.GetDatabase();

            Console.WriteLine("Redis连接成功");

            // 2. 连接数据库
            var dbConnection = config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(dbConnection);

            // 3. 清空旧数据（可选）
            Console.WriteLine("是否清空旧数据？(y/n)");
            if (Console.ReadKey().KeyChar == 'y')
            {
                Console.WriteLine("\n清空中...");
                await ClearRedisData(db);
            }

            // 4. 加载商品数据
            var products = await connection.QueryAsync<ProductData>(
                "SELECT * FROM Ps_Products WHERE Status = 1");

            Console.WriteLine($"找到 {products.Count()} 个商品");

            // 5. 预热商品信息
            foreach (var product in products)
            {
                await PreloadProductInfo(db, product);
                await PreloadProductStock(db, product);
                Console.WriteLine($"✅ 商品预热: {product.ProductName} 库存: {product.Stock}");
            }

            // 6. 预热用户类型（假设数据）
            var users = (await connection.QueryAsync<UserData>(
                "select Id,UserType from Ps_Users where Status=1")).ToList();
            await PreloadUserTypes(db, users);

            Console.WriteLine("\n🎉 Redis数据预热完成！");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        static async Task ClearRedisData(IDatabase db)
        {
            var server = db.Multiplexer.GetServer(db.Multiplexer.GetEndPoints()[0]);

            // 删除所有与商品相关的key
            foreach (var key in server.Keys(pattern: "product:*"))
                await db.KeyDeleteAsync(key);

            foreach (var key in server.Keys(pattern: "stock:*"))
                await db.KeyDeleteAsync(key);

            foreach (var key in server.Keys(pattern: "purchase:*"))
                await db.KeyDeleteAsync(key);

            Console.WriteLine("旧数据已清除");
        }

        static async Task PreloadProductInfo(IDatabase db, ProductData product)
        {
            var key = $"product:info:{product.Id}";

            var hashEntries = new HashEntry[]
            {
                new HashEntry("id", product.Id),
                new HashEntry("name", product.ProductName),
                new HashEntry("price", product.Price.ToString("F2")),
                new HashEntry("status", product.Status.ToString()),
                new HashEntry("isHot", product.IsHot ? "1" : "0"),
                new HashEntry("shardCount", product.ShardCount.ToString())
            };

            await db.HashSetAsync(key, hashEntries);
            await db.KeyExpireAsync(key, TimeSpan.FromDays(1));
        }

        static async Task PreloadProductStock(IDatabase db, ProductData product)
        {
            if (product.IsHot && product.ShardCount > 1)
            {
                int perShard = product.Stock / product.ShardCount;
                for (int i = 1; i <= product.ShardCount; i++)
                {
                    await db.StringSetAsync($"stock:{product.Id}:{i}", perShard);
                }
            }
            else
            {
                await db.StringSetAsync($"stock:{product.Id}", product.Stock);
            }
        }

        static async Task PreloadUserTypes(IDatabase db, List<UserData> users)
        {
            for (int i = 0; i < users.Count; i++)
            {
                await db.StringSetAsync($"user:type:{users[i].Id}", users[i].UserType);
            }
        }
    }
}