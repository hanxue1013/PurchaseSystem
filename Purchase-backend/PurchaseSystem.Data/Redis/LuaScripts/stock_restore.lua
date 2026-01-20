-- 文件名: stock_restore.lua
-- 功能：安全恢复库存
-- 参数：productId, orderNo

local productId = ARGV[1]
local orderNo = ARGV[2]

-- ========== 键定义 ==========
local productKey = "product:info:" .. productId
local stockKey = "stock:" .. productId

-- ========== 1. 检查商品信息 ==========
local isHot = redis.call('HGET', productKey, 'isHot')
local shardCount = tonumber(redis.call('HGET', productKey, 'shardCount')) or 1

-- ========== 2. 恢复库存 ==========
if isHot == '1' and shardCount > 1 then
    -- 热门商品：随机选择一个分片恢复
    local shardIndex = math.random(1, shardCount)
    stockKey = "stock:" .. productId .. ":" .. shardIndex
end

-- 原子性增加库存
redis.call('INCR', stockKey)

-- ========== 3. 记录库存恢复日志 ==========
local logKey = "log:stock:restore:" .. os.date("%Y%m%d")
redis.call('LPUSH', logKey, '{"productId":' .. productId .. ',"orderNo":"' .. orderNo .. '","time":"' .. os.date("%Y-%m-%d %H:%M:%S") .. '"}')

return '{"success":1, "msg":"库存恢复成功", "productId":' .. productId .. ', "orderNo":"' .. orderNo .. '"}'