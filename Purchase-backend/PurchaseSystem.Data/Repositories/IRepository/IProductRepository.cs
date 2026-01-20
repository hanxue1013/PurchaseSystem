using PurchaseSystem.Model.Entities;

namespace PurchaseSystem.Data.Repositories.IRepository
{
    /// <summary>
    /// 商品仓储接口
    /// </summary>
    public interface IProductRepository : IRepository<PsProduct>
    {
        Task<bool> UpdateStockAsync(int productId, int stock);
    }
}
