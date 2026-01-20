using PurchaseSystem.Core.Services.IService;
using PurchaseSystem.Data.Redis.Interfaces;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Model.Entities;
using PurchaseSystem.Model.Response;

namespace PurchaseSystem.Core.Services.Service
{
    /// <summary>
    /// 商品服务实现
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRedisService _redisService;

        public ProductService(IProductRepository productRepository, IRedisService redisService)
        {
            _productRepository = productRepository;
            _redisService = redisService;
        }

        public async Task<List<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDTO).ToList();
        }

        public async Task<ProductDTO> GetProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return MapToDTO(product);
        }

        public async Task<int> AddProductAsync(PsProduct product)
        {
            int result = await _productRepository.AddAsync(product);
            return result;
        }

        public async Task<bool> UpdateStockAsync(int productId, int newStock)
        {
            // 1. 获取原商品
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return false;

            // 2. 更新数据库库存
            product.Stock = newStock;
            var success = await _productRepository.UpdateAsync(product);

            // 3. 同步更新Redis库存（如果Redis中有缓存）
            if (success)
            {
                await _redisService.PreloadStockAsync(productId, newStock, product.ShardCount);
            }

            return success;
        }

        public async Task<bool> PreloadStockToRedisAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return false;

            await _redisService.PreloadStockAsync(productId, product.Stock, product.ShardCount);
            return true;
        }

        private ProductDTO MapToDTO(PsProduct product)
        {
            if (product == null) return null;

            return new ProductDTO
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Stock = product.Stock,
                Price = product.Price,
                ImgSrc = product.ImgSrc
            };
        }
    }
}
