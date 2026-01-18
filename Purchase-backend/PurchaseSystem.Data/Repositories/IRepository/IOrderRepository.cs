using PurchaseSystem.Model.Entities;

namespace PurchaseSystem.Data.Repositories.IRepository
{
    public interface IOrderRepository : IRepository<PsOrder>
    {
        Task UpdateStatusAsync(long orderId, int status);
        Task<bool> ExistsAsync(long orderId);
    }
}
