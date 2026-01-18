using PurchaseSystem.Data.Redis.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseSystem.Data.Redis.Services
{
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly IServer _server;

        public RedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
            _server = _redis.GetServer(_redis.GetEndPoints()[0]);
        }

        // 商品库存预热
        public async Task PreloadStockAsync(int productId, int totalStock, int shardCount = 1)
        {
            if (shardCount > 1)
            {
                // 分片存储
                var perShard = totalStock / shardCount;
                for (int i = 1; i <= shardCount; i++)
                {
                    await _db.StringSetAsync($"stock:{productId}_{i}", perShard);
                }
            }
            else
            {
                await _db.StringSetAsync($"stock:{productId}", totalStock);
            }
        }
    }
}
