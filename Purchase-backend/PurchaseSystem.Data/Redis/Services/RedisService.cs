using PurchaseSystem.Data.Redis.Interfaces;
using PurchaseSystem.Model.Redis;
using PurchaseSystem.Model.Response;
using StackExchange.Redis;
using System.Text.Json;

namespace PurchaseSystem.Data.Redis.Services
{
    /// <summary>
    /// Redis服务（核心）
    /// </summary>
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly IServer _server;

        private readonly string _purchaseScript;
        private readonly string _timeoutScript;
        private readonly string _restoreScript;

        // 常量定义（Lua脚本）
        private const string PURCHASE_SCRIPT = "purchase_enhanced.lua";
        private const string TIMEOUT_SCRIPT = "order_timeout.lua";
        private const string RESTORE_SCRIPT = "stock_restore.lua";

        public RedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
            _server = _redis.GetServer(_redis.GetEndPoints()[0]);

            _purchaseScript = LuaScriptReader.GetLuaScript(PURCHASE_SCRIPT);
            _timeoutScript = LuaScriptReader.GetLuaScript(TIMEOUT_SCRIPT);
            _restoreScript = LuaScriptReader.GetLuaScript(RESTORE_SCRIPT);
        }

        #region 基本操作
        public async Task<RedisValue> StringGetAsync(RedisKey key)
        {
            var result = await _db.StringGetAsync(key);
            return result;
        }

        public async Task<bool> StringSetAsync(RedisKey key, RedisValue value)
        {
            bool flag = await _db.StringSetAsync(key, value);
            return flag;
        }

        public async Task HashSetAsync(RedisKey key, HashEntry[] hashEntries)
        {
            await _db.HashSetAsync(key, hashEntries);
        }

        public async Task<bool> KeyExpireAsync(RedisKey key, TimeSpan timeSpan)
        {
            bool flag = await _db.KeyExpireAsync(key, timeSpan);
            return flag;
        }

        public async Task<HashEntry[]> HashGetAllAsync(RedisKey key)
        {
            var result = await _db.HashGetAllAsync(key);
            return result;
        }
        #endregion

        #region 缓存
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

        public async Task<bool> CheckRateLimitAsync(string key, int limit, TimeSpan period)
        {
            var current = await _db.StringIncrementAsync(key);
            if (current == 1)
            {
                await _db.KeyExpireAsync(key, period);
            }
            return current <= limit;
        }
        #endregion

        #region 抢购
        public async Task<RedisStockResult> DeductStockAsync(int userId, int productId, int userType, DateTime now, decimal price)
        {
            try
            {
                long orderNo = GenerateOrderNo();
                var timestamp = now.Ticks;

                var result = await _db.ScriptEvaluateAsync(_purchaseScript,
                    values: new RedisValue[]
                    {
                        userId, productId, userType, timestamp, orderNo, price.ToString("F2")
                    });

                return ParseRedisResult(result.ToString());
            }
            catch (Exception ex)
            {
                return new RedisStockResult
                {
                    Success = false,
                    ErrorCode = "REDIS_ERROR",
                    Message = $"系统繁忙: {ex.Message}"
                };
            }
        }

        // 从异步队列获取订单进行处理
        public async Task<string> GetAsyncOrderFromQueueAsync()
        {
            return await _db.ListRightPopAsync("queue:order:async");
        }

        // 处理超时订单
        public async Task<RedisTimeoutResult> ProcessTimeoutOrderAsync(long orderNo)
        {
            var result = await _db.ScriptEvaluateAsync(_timeoutScript,
                values: new RedisValue[] { orderNo, DateTime.Now.Ticks });

            return JsonSerializer.Deserialize<RedisTimeoutResult>(result.ToString());
        }

        // 恢复库存
        public async Task<RedisRestoreResult> RestoreStockAsync(int productId, long orderNo)
        {
            var result = await _db.ScriptEvaluateAsync(_restoreScript,
                values: new RedisValue[] { productId, orderNo });

            return JsonSerializer.Deserialize<RedisRestoreResult>(result.ToString());
        }

        // 获取超时需要处理的订单
        public async Task<string[]> GetTimeoutOrdersAsync(int batchSize = 50)
        {
            var now = DateTime.Now.Ticks;
            var orders = await _db.SortedSetRangeByScoreAsync(
                "queue:order:timeout",
                0, now,
                take: batchSize);

            return orders.ToStringArray();
        }

        // 获取库存恢复队列的任务
        public async Task<string> GetStockRestoreTaskAsync()
        {
            return await _db.ListRightPopAsync("queue:stock:restore");
        }


        private long GenerateOrderNo()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1288834974657L;
            var workerId = 1L;
            var datacenterId = 1L;
            var sequence = 0L;

            return (timestamp << 22) | (datacenterId << 17) | (workerId << 12) | sequence;
        }

        private RedisStockResult ParseRedisResult(string json)
        {
            try
            {
                var result = JsonSerializer.Deserialize<RedisResultWrapper>(json);

                if (result.success == 1)
                {
                    return new RedisStockResult
                    {
                        Success = true,
                        OrderNo = long.Parse(result.orderNo),
                        Amount = decimal.Parse(result.price),
                        Message = result.msg
                    };
                }
                else
                {
                    return new RedisStockResult
                    {
                        Success = false,
                        ErrorCode = result.code,
                        Message = result.msg
                    };
                }
            }
            catch
            {
                return new RedisStockResult
                {
                    Success = false,
                    ErrorCode = "PARSE_ERROR",
                    Message = "系统异常"
                };
            }
        }
        #endregion
    }
}
