namespace PurchaseSystem.Model.Response
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string ImgSrc { get; set; }
    }
}
