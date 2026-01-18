namespace PurchaseSystem.Model.Request
{
    public class UpdateStockRequest
    {
        public int productId { get; set; }
        public int newStock { get; set; }
    }
}
