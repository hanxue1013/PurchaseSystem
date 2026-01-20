using PurchaseSystem.Model.Response;

namespace PurchaseSystem.Core.Services.IService
{
    /// <summary>
    /// 抢购服务接口
    /// </summary>
    public interface IPurchaseService
    {
        Task<ApiResponse> GrabProductAsync(int userId, int productId);
    }
}
