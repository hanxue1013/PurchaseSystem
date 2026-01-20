using Microsoft.AspNetCore.Mvc;
using PurchaseSystem.Core.Services.IService;
using PurchaseSystem.Model.Entities;
using PurchaseSystem.Model.Request;
using PurchaseSystem.Model.Response;

namespace PurchaseSystem.API.Controllers
{
    /// <summary>
    /// 商品管理
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetAll()
        {
            ApiResponse result = new ApiResponse()
            {
                IsSuccess = true
            };
            try
            {
                var products = await _productService.GetAllProductsAsync();
                result.Data = products;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
            }

            return Ok(result);
        }

        [HttpGet("GetProduct")]
        public async Task<IActionResult> GetById(int id)
        {
            ApiResponse result = new ApiResponse()
            {
                IsSuccess = true
            };
            try
            {
                var product = await _productService.GetProductAsync(id);
                result.Data = product;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
            }

            return Ok(result);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> Add(PsProduct product)
        {
            ApiResponse result = new ApiResponse()
            {
                IsSuccess = true
            };
            try
            {
                product.ShardCount = (product.Stock + 1000 - 1) / 1000;
                int count = await _productService.AddProductAsync(product);
                result.Data = count;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
            }

            return Ok(result);
        }

        [HttpPut("UpdateStock")]
        public async Task<IActionResult> UpdateStock(UpdateStockRequest request)
        {
            ApiResponse result = new ApiResponse()
            {
                IsSuccess = true
            };
            try
            {
                var flag = await _productService.UpdateStockAsync(request.productId, request.newStock);
                result.Data = flag;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                result.Message = e.Message;
            }

            return Ok(result);
        }
    }
}
