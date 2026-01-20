//using Microsoft.Extensions.Caching.Memory;

//namespace PurchaseSystem.API.Middleware
//{
//    public class OptimizedRateLimitMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<OptimizedRateLimitMiddleware> _logger;

//        // 使用内存缓存做第一层限流（减少Redis压力）
//        private static readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());

//        public async Task InvokeAsync(HttpContext context)
//        {
//            // 只对抢购接口限流
//            if (context.Request.Path.StartsWithSegments("/api/purchase/grab"))
//            {
//                var userId = GetUserId(context);
//                var ip = context.Connection.RemoteIpAddress?.ToString();

//                // 1. 内存级限流（最快）
//                if (!CheckMemoryRateLimit(userId, ip))
//                {
//                    context.Response.StatusCode = 429;
//                    await context.Response.WriteAsJsonAsync(new
//                    {
//                        Success = false,
//                        Message = "请求过于频繁，请稍后再试"
//                    });
//                    return;
//                }

//                // 2. 如果通过内存限流，再走Redis限流（由Lua脚本处理）
//                // 这里不再重复限流
//            }

//            await _next(context);
//        }

//        private bool CheckMemoryRateLimit(int userId, string ip)
//        {
//            // 用户维度：每秒最多3次
//            var userKey = $"user_limit_{userId}_{DateTime.Now:yyyyMMddHHmmss}";
//            var userCount = _memoryCache.GetOrCreate(userKey, entry =>
//            {
//                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(1);
//                return 0;
//            });

//            if (userCount >= 3) return false;
//            _memoryCache.Set(userKey, userCount + 1);

//            // IP维度：每秒最多100次
//            if (!string.IsNullOrEmpty(ip))
//            {
//                var ipKey = $"ip_limit_{ip}_{DateTime.Now:yyyyMMddHHmmss}";
//                var ipCount = _memoryCache.GetOrCreate(ipKey, entry =>
//                {
//                    entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(1);
//                    return 0;
//                });

//                if (ipCount >= 100) return false;
//                _memoryCache.Set(ipKey, ipCount + 1);
//            }

//            return true;
//        }
//    }
//}
