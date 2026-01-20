using System.Text.Json.Serialization;

namespace PurchaseSystem.Model.Entities
{
    /// <summary>
    /// 订单实体
    /// </summary>
    public class PsOrder : BaseEntity
    {
        [JsonPropertyName("orderNo")]
        public long OrderNo { get; set; }           // 订单号（雪花算法）
        [JsonPropertyName("userId")]
        public int UserId { get; set; }             // 用户ID
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }          // 商品ID
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;      // 购买数量
        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }    // 总金额
        [JsonPropertyName("status")]
        public int Status { get; set; }             // 0:待支付 1:已支付 2:已取消 3:超时关闭
        [JsonPropertyName("payTime")]
        public DateTime? PayTime { get; set; }      // 支付时间
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }   // 支付交易号
    }
}
