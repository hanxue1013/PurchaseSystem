using PurchaseSystem.Model.Entities;

namespace PurchaseSystem.Data.Repositories.IRepository
{
    public interface IProductRepository : IRepository<PsProduct>
    {
        Task<bool> UpdateStockAsync(int productId, int stock);
    }
}
