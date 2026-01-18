using Microsoft.AspNetCore.Mvc;

namespace PurchaseSystem.API.Controllers
{
    /// <summary>
    /// 支付回调
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(ILogger<PaymentController> logger)
        {
            _logger = logger;
        }
    }
}
