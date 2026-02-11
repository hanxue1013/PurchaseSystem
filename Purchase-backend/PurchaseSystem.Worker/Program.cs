// RedisConnectionTest.cs
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("测试Redis连接...");

        try
        {
            // 构建配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // 设置基础路径为当前目录
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 读取连接字符串
            var redis = await ConnectionMultiplexer.ConnectAsync(configuration.GetConnectionString("RedisConnection"));
            var db = redis.GetDatabase();

            // 简单测试
            await db.StringSetAsync("test_key", "test_value", TimeSpan.FromSeconds(10));
            var value = await db.StringGetAsync("test_key");

            Console.WriteLine($"✅ Redis连接成功: {value}");

            // 测试Lua脚本
            var luaScript = "return redis.call('SET', 'lua_test', 'ok')";
            var result = await db.ScriptEvaluateAsync(luaScript);
            Console.WriteLine($"✅ Lua脚本执行成功: {result}");

            string lua = @"-- 文件名: purchase_enhanced.lua
-- 功能：抢购商品（原子操作）
-- 参数：userId, productId, userType, timestamp, orderNo, price
-- 返回：JSON字符串 {success:1/0, orderNo:xxx, msg:""xxx""}

-- ========== 参数解析 ==========
local userId = ARGV[1]
local productId = ARGV[2]
local userType = ARGV[3]
local timestamp = ARGV[4]
local orderNo = ARGV[5]
local price = ARGV[6]

-- ========== 键定义 ==========
local userBoughtKey = ""purchase:user:"" .. userId .. "":products""
local rateLimitKey = ""rate:user:"" .. userId .. "":"" .. math.floor(timestamp/1000)
local productKey = ""product:info:"" .. productId
local stockKey = ""stock:"" .. productId  -- 默认键
local orderKey = ""order:"" .. orderNo
local timeoutQueueKey = ""queue:order:timeout""  -- 超时队列（Sorted Set）
local asyncQueueKey = ""queue:order:async""      -- 异步处理队列（List）

-- ========== 1. 限流检查 ==========
local requestCount = redis.call('INCR', rateLimitKey)
if requestCount == 1 then
    redis.call('EXPIRE', rateLimitKey, 1)
end
if requestCount > 3 then
    return '{""success"":0, ""code"":""RATE_LIMIT"", ""msg"":""请求过于频繁""}'
end

-- ========== 2. 检查用户是否已购买 ==========
if redis.call('SISMEMBER', userBoughtKey, productId) == 1 then
    return '{""success"":0, ""code"":""ALREADY_BOUGHT"", ""msg"":""您已购买过此商品""}'
end

-- ========== 3. 检查用户购买额度 ==========
local maxQuota = 8
if userType == '2' then
    maxQuota = 2
end
local userPurchasedCount = redis.call('SCARD', userBoughtKey)
if userPurchasedCount >= maxQuota then
    return '{""success"":0, ""code"":""QUOTA_LIMIT"", ""msg"":""您的购买额度已用完""}'
end

-- ========== 4. 商品状态检查 ==========
local productStatus = redis.call('HGET', productKey, 'status')
if not productStatus or productStatus ~= '1' then
    return '{""success"":0, ""code"":""PRODUCT_INVALID"", ""msg"":""商品已下架""}'
end

-- ========== 5. 库存扣减 ==========
-- 判断是否热门商品（分片存储）
local isHot = redis.call('HGET', productKey, 'isHot')
local shardCount = tonumber(redis.call('HGET', productKey, 'shardCount')) or 1

if isHot == '1' and shardCount > 1 then
    -- 热门商品：随机选择一个分片
    local shardIndex = math.random(1, shardCount)
    stockKey = ""stock:"" .. productId .. "":"" .. shardIndex
end

local remaining = redis.call('DECR', stockKey)
if remaining < 0 then
    redis.call('INCR', stockKey)  -- 恢复库存
    return '{""success"":0, ""code"":""STOCK_OUT"", ""msg"":""库存不足""}'
end

-- ========== 6. 记录用户购买 ==========
redis.call('SADD', userBoughtKey, productId)
redis.call('EXPIRE', userBoughtKey, 86400)  -- 24小时过期

-- ========== 7. 缓存订单信息 ==========
redis.call('HSET', orderKey, 
    'userId', userId,
    'productId', productId,
    'price', price,
    'status', '0',  -- 0=待支付
    'createTime', timestamp,
    'expireTime', timestamp + 900000  -- 15分钟后过期（毫秒）
)
redis.call('EXPIRE', orderKey, 900)  -- 15分钟

-- ========== 8. 加入超时队列（Sorted Set） ==========
-- Sorted Set的score是过期时间戳，value是订单号
local expireTime = tonumber(timestamp) + 900000  -- 当前时间 + 15分钟
redis.call('ZADD', timeoutQueueKey, expireTime, orderNo)

-- ========== 9. 加入异步处理队列（List） ==========
-- 将订单信息JSON推入List，供后台Worker处理
local orderDataJson = '{""orderNo"":""' .. orderNo .. '"",""userId"":' .. userId .. ',""productId"":' .. productId .. ',""price"":' .. price .. '}'
redis.call('LPUSH', asyncQueueKey, orderDataJson)

-- ========== 10. 返回成功 ==========
return '{""success"":1, ""orderNo"":""' .. orderNo .. '"", ""price"":' .. price .. ', ""msg"":""抢购成功""}'";
            db.ScriptEvaluate(lua,
                    values: new RedisValue[]
                    {
                        13003, 1, 1, "639045093719119347", 2013470590534553600, 800
                    });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Redis连接失败: {ex.Message}");
            Console.WriteLine($"详细: {ex}");
        }
    }
}