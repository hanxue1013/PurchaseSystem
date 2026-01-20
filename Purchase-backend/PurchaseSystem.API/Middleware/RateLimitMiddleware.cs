using PurchaseSystem.Data.Redis.Interfaces;

namespace PurchaseSystem.API.Middleware
{
    /// <summary>
    /// 限流中间件
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisService _redisService;

        public RateLimitMiddleware(RequestDelegate next, IRedisService redisService)
        {
            _next = next;
            _redisService = redisService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 只对抢购接口进行限流
            if (context.Request.Path.StartsWithSegments("/api/purchase/grab"))
            {
                // 从请求中获取用户ID（实际应从Token或Session获取）
                //var userId = GetUserIdFromRequest(context);
                //if (userId > 0)
                //{
                // IP限流
                var ip = context.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(ip))
                {
                    var ipLimitKey = $"rate:ip:{ip}:minute";
                    var ipAllowed = await _redisService.CheckRateLimitAsync(ipLimitKey, 100, TimeSpan.FromMinutes(1));
                    if (!ipAllowed)
                    {
                        context.Response.StatusCode = 429;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Success = false,
                            Message = "IP请求过于频繁"
                        });
                        return;
                    }
                }

                //// 用户维度限流
                //var userLimitKey = $"rate:user:{userId}:second";
                //var userAllowed = await _redisService.CheckRateLimitAsync(userLimitKey, 3, TimeSpan.FromSeconds(1));
                //if (!userAllowed)
                //{
                //    context.Response.StatusCode = 429;
                //    await context.Response.WriteAsJsonAsync(new
                //    {
                //        Success = false,
                //        Message = "请求过于频繁，请稍后再试"
                //    });
                //    return;
                //}
                //}
            }

            await _next(context);
        }

        private int GetUserIdFromRequest(HttpContext context)
        {
            // 从Header获取
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) &&
                int.TryParse(userIdHeader, out var userId))
            {
                return userId;
            }

            // 从QueryString获取（测试用）
            if (context.Request.Query.TryGetValue("userId", out var queryUserId) &&
                int.TryParse(queryUserId, out userId))
            {
                return userId;
            }

            return 0;
        }
    }
}