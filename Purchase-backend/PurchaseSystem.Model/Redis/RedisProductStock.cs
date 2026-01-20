namespace PurchaseSystem.Model.Redis
{
    public class ProductCache
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public bool IsHot { get; set; }
        public int ShardCount { get; set; }
    }
}
