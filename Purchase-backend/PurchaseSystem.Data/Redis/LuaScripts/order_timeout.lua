-- 文件名: order_timeout.lua
-- 功能：处理超时订单，释放库存
-- 参数：orderNo, currentTimestamp
-- 返回：处理结果JSON

local orderNo = ARGV[1]
local currentTimestamp = tonumber(ARGV[2])

-- ========== 键定义 ==========
local orderKey = "order:" .. orderNo
local timeoutQueueKey = "queue:order:timeout"
local stockRestoreQueueKey = "queue:stock:restore"  -- 库存恢复队列

-- ========== 1. 获取订单信息 ==========
local orderData = redis.call('HGETALL', orderKey)
if #orderData == 0 then
    return '{"success":0, "msg":"订单不存在"}'
end

-- 解析订单信息
local orderInfo = {}
for i = 1, #orderData, 2 do
    orderInfo[orderData[i]] = orderData[i+1]
end

-- ========== 2. 检查订单状态 ==========
local status = orderInfo['status']
if status ~= '0' then  -- 不是待支付状态
    -- 从超时队列移除
    redis.call('ZREM', timeoutQueueKey, orderNo)
    return '{"success":0, "msg":"订单状态不是待支付"}'
end

-- ========== 3. 检查是否真的超时 ==========
local expireTime = tonumber(orderInfo['expireTime'])
if expireTime > currentTimestamp then
    return '{"success":0, "msg":"订单尚未超时"}'
end

-- ========== 4. 更新订单状态为超时 ==========
redis.call('HSET', orderKey, 'status', '3')  -- 3=超时关闭
redis.call('EXPIRE', orderKey, 3600)  -- 设置1小时过期，留给对账

-- ========== 5. 从用户购买记录中移除 ==========
local userId = orderInfo['userId']
local productId = orderInfo['productId']
local userBoughtKey = "purchase:user:" .. userId .. ":products"
redis.call('SREM', userBoughtKey, productId)

-- ========== 6. 释放库存（加入恢复队列） ==========
-- 注意：这里不直接恢复库存，而是加入队列，防止高并发时库存不一致
local restoreData = '{"productId":' .. productId .. ',"orderNo":"' .. orderNo .. '","timestamp":' .. currentTimestamp .. '}'
redis.call('LPUSH', stockRestoreQueueKey, restoreData)

-- ========== 7. 从超时队列中移除 ==========
redis.call('ZREM', timeoutQueueKey, orderNo)

-- ========== 8. 记录处理日志 ==========
local logKey = "log:order:timeout:" .. os.date("%Y%m%d")
redis.call('LPUSH', logKey, orderNo)

return '{"success":1, "msg":"订单超时处理成功", "orderNo":"' .. orderNo .. '", "productId":' .. productId .. '}'