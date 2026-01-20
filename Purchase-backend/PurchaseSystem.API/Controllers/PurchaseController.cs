using Microsoft.AspNetCore.Mvc;
using PurchaseSystem.Core.Services.IService;
using PurchaseSystem.Model.Request;
using PurchaseSystem.Model.Response;

namespace PurchaseSystem.API.Controllers
{
    /// <summary>
    /// 抢购接口（核心）
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpPost("grab")]
        public async Task<IActionResult> GrabProduct([FromBody] GrabRequest request)
        {
            if (request == null || request.UserId <= 0 || request.ProductId <= 0)
            {
                return Ok(ApiResponse.Error("参数无效"));
            }

            // 限流检查（可以放在中间件中）
            var result = await _purchaseService.GrabProductAsync(request.UserId, request.ProductId);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.Success(result, "抢购成功"));
            }
            else
            {
                return Ok(ApiResponse.Error(result.Message));
            }
        }
    }
}
