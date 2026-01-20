using PurchaseSystem.Model.Entities;
using PurchaseSystem.Model.Response;

namespace PurchaseSystem.Core.Services.IService
{
    /// <summary>
    /// 商品服务接口
    /// </summary>
    public interface IProductService
    {
        Task<ProductDTO> GetProductAsync(int productId);

        Task<List<ProductDTO>> GetAllProductsAsync();

        Task<int> AddProductAsync(PsProduct product);

        Task<bool> UpdateStockAsync(int productId, int newStock);

        Task<bool> PreloadStockToRedisAsync(int productId);
    }
}
