namespace PurchaseSystem.Model.Redis
{
    public class RedisOrderInfo
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Status { get; set; }
    }

    public class RedisStockResult
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; }
        public long OrderNo { get; set; }
        public decimal? Amount { get; set; }
        public string Message { get; set; }
    }

    public class RedisTimeoutResult
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
        public string OrderNo { get; set; }
        public int ProductId { get; set; }
    }

    public class RedisRestoreResult
    {
        public bool Success { get; set; }
        public string Msg { get; set; }
        public int ProductId { get; set; }
        public string OrderNo { get; set; }
    }

    // DTO类
    public class RedisResultWrapper
    {
        public int success { get; set; }
        public string orderNo { get; set; }
        public string price { get; set; }
        public string msg { get; set; }
        public string code { get; set; }
    }

    // DTO类
    public class AsyncOrderData
    {
        public string OrderNo { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
    }

    public class StockRestoreData
    {
        public int ProductId { get; set; }
        public long OrderNo { get; set; }
        public long Timestamp { get; set; }
    }
}
