using PurchaseSystem.Model.Entities;

namespace PurchaseSystem.Data.Repositories.IRepository
{
    /// <summary>
    /// 订单仓储接口
    /// </summary>
    public interface IOrderRepository : IRepository<PsOrder>
    {
        Task<PsOrder> GetByOrderNoAsync(long orderNo);
        Task<bool> UpdateStatusAsync(long orderNo, int status, string transactionId = null);
        Task<bool> ExistsByOrderNoAsync(long orderNo);
        Task<int> GetUserTodayPurchaseCountAsync(int userId);
    }
}
