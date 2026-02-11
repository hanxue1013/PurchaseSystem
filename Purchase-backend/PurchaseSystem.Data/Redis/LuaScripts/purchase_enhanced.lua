-- 文件名: purchase_enhanced.lua
-- 功能：抢购商品（原子操作）
-- 参数：userId, productId, userType, timestamp, orderNo, price
-- 返回：JSON字符串 {success:1/0, orderNo:xxx, msg:"xxx"}

-- ========== 参数解析 ==========
local userId = ARGV[1]
local productId = ARGV[2]
local userType = ARGV[3]
local timestamp = ARGV[4]
local orderNo = ARGV[5]
local price = ARGV[6]

-- ========== 键定义 ==========
local userBoughtKey = "purchase:user:" .. userId .. ":products"
local rateLimitKey = "rate:user:" .. userId .. ":" .. math.floor(timestamp/1000)
local productKey = "product:info:" .. productId
local stockKey = "stock:" .. productId  -- 默认键
local orderKey = "order:" .. orderNo
local timeoutQueueKey = "queue:order:timeout"  -- 超时队列（Sorted Set）
local asyncQueueKey = "queue:order:async"      -- 异步处理队列（List）

-- ========== 1. 限流检查 ==========
local requestCount = redis.call('INCR', rateLimitKey)
if requestCount == 1 then
    redis.call('EXPIRE', rateLimitKey, 1)
end
if requestCount > 3 then
    return '{"success":0, "code":"RATE_LIMIT", "msg":"请求过于频繁"}'
end

-- ========== 2. 检查用户是否已购买 ==========
--if redis.call('SISMEMBER', userBoughtKey, productId) == 1 then
--    return '{"success":0, "code":"ALREADY_BOUGHT", "msg":"您已购买过此商品"}'
--end

-- ========== 3. 检查用户购买额度 ==========
local maxQuota = 8
if userType == '2' then
    maxQuota = 2
end
local userPurchasedCount = redis.call('SCARD', userBoughtKey)
if userPurchasedCount >= maxQuota then
    return '{"success":0, "code":"QUOTA_LIMIT", "msg":"您的购买额度已用完"}'
end

-- ========== 4. 商品状态检查 ==========
local productStatus = redis.call('HGET', productKey, 'status')
if not productStatus or productStatus ~= '1' then
    return '{"success":0, "code":"PRODUCT_INVALID", "msg":"商品已下架"}'
end

-- ========== 5. 库存扣减 ==========
-- 判断是否热门商品（分片存储）
local isHot = redis.call('HGET', productKey, 'isHot')
local shardCount = tonumber(redis.call('HGET', productKey, 'shardCount')) or 1

if isHot == '1' and shardCount > 1 then
    -- 热门商品：随机选择一个分片
    local shardIndex = math.random(1, shardCount)
    stockKey = "stock:" .. productId .. ":" .. shardIndex
end
--扣减库存
local remaining = redis.call('DECR', stockKey)
if remaining < 0 then
    redis.call('INCR', stockKey)  -- 恢复库存
    return '{"success":0, "code":"STOCK_OUT", "msg":"库存不足"}'
end

-- ========== 6. 记录用户购买 ==========
redis.call('SADD', userBoughtKey, productId)
redis.call('EXPIRE', userBoughtKey, 86400)  -- 24小时过期

-- ========== 7. 缓存订单信息 ==========
local expireTimeMs = tonumber(timestamp) + 900000  -- 15分钟 = 900,000毫秒
redis.call('HSET', orderKey, 'userId', userId)
redis.call('HSET', orderKey, 'productId', productId)
redis.call('HSET', orderKey, 'price', price)
redis.call('HSET', orderKey, 'status', '0')
redis.call('HSET', orderKey, 'createTime', timestamp)
redis.call('HSET', orderKey, 'expireTime', expireTimeMs)
redis.call('HSET', orderKey, 'userType', userType)

redis.call('EXPIRE', orderKey, 900)  -- 15分钟

-- ========== 8. 加入超时队列（Sorted Set） ==========
-- Sorted Set的score是过期时间戳，value是订单号
redis.call('ZADD', timeoutQueueKey, expireTimeMs, orderNo)

-- ========== 9. 加入异步处理队列（List） ==========
-- 将订单信息JSON推入List，供后台Worker处理
--local orderDataJson = '{"orderNo":"' .. orderNo .. '","userId":' .. userId .. ',"productId":' .. productId .. ',"price":' .. price .. '}'
--redis.call('LPUSH', asyncQueueKey, orderDataJson)
local function escape_json(str)
    return string.gsub(str, '"', '\\"')
end
local orderDataJson = '{"orderNo":"' .. escape_json(orderNo) .. '","userId":"' .. 
                     escape_json(userId) .. '","productId":"' .. escape_json(productId) .. 
                     '","price":' .. price .. '}'
redis.call('LPUSH', asyncQueueKey, orderDataJson)

-- ========== 10. 返回成功 ==========
return '{"success":1, "orderNo":"' .. orderNo .. '", "price":' .. price .. ', "msg":"抢购成功","code":"200"}'