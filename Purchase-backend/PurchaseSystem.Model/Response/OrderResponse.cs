namespace PurchaseSystem.Model.Response
{
    public class OrderResult
    {
        public long OrderNo { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
