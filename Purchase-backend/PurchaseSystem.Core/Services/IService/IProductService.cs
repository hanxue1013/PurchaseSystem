using PurchaseSystem.Model.Entities;
using PurchaseSystem.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseSystem.Core.Services.IService
{
    public interface IProductService
    {
        Task<ProductDTO> GetProductAsync(int productId);

        Task<List<ProductDTO>> GetAllProductsAsync();

        Task<int> AddProductAsync(PsProduct product);

        Task<bool> UpdateStockAsync(int productId, int newStock);

        Task<bool> PreloadStockToRedisAsync(int productId);
    }
}
