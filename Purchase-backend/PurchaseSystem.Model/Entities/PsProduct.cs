using System.Text.Json.Serialization;

namespace PurchaseSystem.Model.Entities
{
    public class PsProduct : BaseEntity
    {
        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; }
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
        [JsonPropertyName("stock")]
        public int Stock { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("imgSrc")]
        public string ImgSrc { get; set; }
        [JsonPropertyName("isHot")]
        public bool IsHot { get; set; }
        [JsonPropertyName("shardCount")]
        public int ShardCount { get; set; } = 1; // 库存分片数
        [JsonPropertyName("status")]
        public int Status { get; set; } = 1; // 1:上架 0:下架
        [JsonPropertyName("updateTime")]
        public DateTime? UpdateTime { get; set; }
    }
}
