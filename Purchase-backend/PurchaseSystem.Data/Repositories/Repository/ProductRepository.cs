using PurchaseSystem.Data.Repositories.IRepository;
using System.Data;
using PurchaseSystem.Model.Entities;
using Dapper;

namespace PurchaseSystem.Data.Repositories.Repository
{
    public class ProductRepository : BaseRepository<PsProduct>, IProductRepository
    {
        public ProductRepository(IDbConnection connection) : base(connection, "Ps_Products")
        {
        }

        public async Task<bool> UpdateStockAsync(int productId, int stock)
        {
            var sql = "UPDATE Products SET Stock = @Stock WHERE Id = @ProductId";
            int count = await _connection.ExecuteAsync(sql, new { ProductId = productId, Stock = stock });
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
