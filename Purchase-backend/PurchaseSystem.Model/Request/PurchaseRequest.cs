namespace PurchaseSystem.Model.Request
{
    public class GrabRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class PaymentCallbackRequest
    {
        public long OrderNo { get; set; }
        public string TransactionId { get; set; }
    }
}
