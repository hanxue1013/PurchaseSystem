using System;
namespace PurchaseSystem.Preheat
{
    public class UserData
    {
        public int Id { get; set; }
        public int UserType { get; set; }
    }

    public class ProductData
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public bool IsHot { get; set; }
        public int ShardCount { get; set; }
        public int Status { get; set; }
    }
}
