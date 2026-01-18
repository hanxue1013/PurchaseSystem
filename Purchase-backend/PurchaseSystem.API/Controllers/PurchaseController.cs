using Microsoft.AspNetCore.Mvc;

namespace PurchaseSystem.API.Controllers
{
    /// <summary>
    /// 抢购接口（核心）
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(ILogger<PurchaseController> logger)
        {
            _logger = logger;
        }
    }
}
