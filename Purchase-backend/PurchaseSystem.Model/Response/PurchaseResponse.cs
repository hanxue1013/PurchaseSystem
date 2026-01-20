namespace PurchaseSystem.Model.Response
{
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public long? OrderNo { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public decimal Amount { get; set; }
    }
}
