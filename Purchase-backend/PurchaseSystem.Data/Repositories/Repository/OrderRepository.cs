using Dapper;
using PurchaseSystem.Data.Database;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Model.Entities;
using System.Data;

namespace PurchaseSystem.Data.Repositories.Repository
{
    public class OrderRepository : BaseRepository<PsOrder>, IOrderRepository
    {
        protected OrderRepository(IDbConnection connection, string tableName) : base(connection, "Ps_Order")
        {
        }

        public async Task UpdateStatusAsync(long orderId, int status)
        {
            var sql = @"
                UPDATE Orders 
                SET Status = @Status, 
                    PayTime = CASE WHEN @Status = 1 THEN GETDATE() ELSE PayTime END
                WHERE Id = @OrderId";

            await _connection.ExecuteAsync(sql, new { OrderId = orderId, Status = status });
        }

        public async Task<bool> ExistsAsync(long orderId)
        {
            var sql = "SELECT 1 FROM Orders WHERE Id = @OrderId";
            var result = await _connection.ExecuteScalarAsync<int?>(sql, new { OrderId = orderId });
            return result.HasValue;
        }
    }
}
