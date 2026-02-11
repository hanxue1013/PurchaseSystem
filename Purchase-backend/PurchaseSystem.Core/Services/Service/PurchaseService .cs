using Microsoft.Extensions.Logging;
using PurchaseSystem.Common.Helper;
using PurchaseSystem.Core.Services.IService;
using PurchaseSystem.Data.Redis.Interfaces;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Model.Entities;
using PurchaseSystem.Model.Redis;
using PurchaseSystem.Model.Response;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace PurchaseSystem.Core.Services.Service
{
    /// <summary>
    /// 抢购服务（核心）
    /// </summary>
    public class PurchaseService : IPurchaseService
    {
        private readonly IRedisService _redisService;

        public PurchaseService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        // 商品信息本地缓存（减少Redis访问）
        private static readonly ConcurrentDictionary<int, ProductCache> _productCache = new();
        private static readonly ConcurrentDictionary<int, int> _userTypeCache = new();

        public async Task<ApiResponse> GrabProductAsync(int userId, int productId)
        {
            try
            {
                // 基础校验
                if (userId <= 0 || productId <= 0)
                    return ApiResponse.Error("参数无效");

                // 获取用户类型
                if (!_userTypeCache.TryGetValue(userId, out var userType))
                {
                    userType = await GetUserTypeFromCacheAsync(userId);
                    if (userType != -1)
                    {
                        _userTypeCache.TryAdd(userId, userType);
                    }
                    else
                    {
                        return ApiResponse.Error("当前非有效用户");
                    }
                }

                // 从本地缓存获取商品信息
                if (!_productCache.TryGetValue(productId, out var productCache))
                {
                    // 如果缓存没有，从Redis获取并缓存
                    productCache = await GetProductFromRedisAsync(productId);
                    if (productCache == null || productCache.Price <= 0)
                        return ApiResponse.Error("商品不存在");

                    _productCache.TryAdd(productId, productCache);
                }

                // 检查商品状态
                if (productCache.Status != 1)
                    return ApiResponse.Error("商品已下架");

                // 执行Redis Lua脚本（核心：原子操作，一次网络往返）
                var redisResult = await _redisService.DeductStockAsync(userId, productId, userType, DateTime.Now, productCache.Price);

                if (!redisResult.Success)
                    return ApiResponse.Error(redisResult.ErrorCode, redisResult.Message);

                return ApiResponse.Success(
                    new
                    {
                        redisResult.OrderNo,
                        Amount = productCache.Price,
                    }, "抢购成功，请在15分钟内完成支付");
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(ex.Message);
            }
        }

        // 从Redis获取商品信息（非抢购核心路径，可缓存）
        private async Task<ProductCache> GetProductFromRedisAsync(int productId)
        {
            var key = $"product:info:{productId}";
            var hash = await _redisService.HashGetAllAsync(key);

            if (hash.Length == 0) return null;

            var dict = hash.ToStringDictionary();
            return new ProductCache
            {
                Id = productId,
                Name = dict.GetValueOrDefault("name", ""),
                Price = decimal.TryParse(dict.GetValueOrDefault("price"), out var price) ? price : 0,
                Status = int.TryParse(dict.GetValueOrDefault("status"), out var status) ? status : 0,
                IsHot = dict.GetValueOrDefault("isHot") == "1",
                ShardCount = int.TryParse(dict.GetValueOrDefault("shardCount"), out var shard) ? shard : 1
            };
        }

        // 从Redis获取用户类型
        private async Task<int> GetUserTypeFromCacheAsync(int userId)
        {
            // 1. 先从Redis缓存获取
            var userType = await _redisService.StringGetAsync($"user:type:{userId}");
            if (userType.HasValue && int.TryParse(userType, out var type))
                return type;
            else
                return -1;

            // 2. Redis没有，从数据库获取（应该很少发生）
            // 这里简化处理，实际应从用户服务获取
            //return userId % 2 == 1 ? 1 : 2; // 假设规则
        }
    }
}
