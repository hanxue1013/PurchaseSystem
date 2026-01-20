using PurchaseSystem.Model.Redis;
using StackExchange.Redis;

namespace PurchaseSystem.Data.Redis.Interfaces
{
    /// <summary>
    /// Redis服务接口
    /// </summary>
    public interface IRedisService
    {
        #region 基本操作
        Task<RedisValue> StringGetAsync(RedisKey key);
        Task<bool> StringSetAsync(RedisKey key, RedisValue value);
        Task HashSetAsync(RedisKey key, HashEntry[] hashEntries);
        Task<bool> KeyExpireAsync(RedisKey key, TimeSpan timeSpan);
        Task<HashEntry[]> HashGetAllAsync(RedisKey key);
        #endregion

        #region 缓存
        // 商品库存预热
        Task PreloadStockAsync(int productId, int totalStock, int shardCount = 1);
        // 限流控制
        Task<bool> CheckRateLimitAsync(string key, int limit, TimeSpan period);
        #endregion

        #region 抢购
        Task<RedisStockResult> DeductStockAsync(int userId, int productId, int userType, DateTime now, decimal price);
        Task<string> GetAsyncOrderFromQueueAsync();
        Task<string[]> GetTimeoutOrdersAsync(int batchSize = 50);
        Task<RedisTimeoutResult> ProcessTimeoutOrderAsync(long orderNo);
        Task<string> GetStockRestoreTaskAsync();
        Task<RedisRestoreResult> RestoreStockAsync(int productId, long orderNo);
        #endregion
    }
}
