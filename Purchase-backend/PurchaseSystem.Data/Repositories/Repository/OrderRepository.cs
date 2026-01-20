using Dapper;
using PurchaseSystem.Data.Database;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Model.Entities;
using System.Data;

namespace PurchaseSystem.Data.Repositories.Repository
{
    /// <summary>
    /// 订单仓储实现
    /// </summary>
    public class OrderRepository : BaseRepository<PsOrder>, IOrderRepository
    {
        public OrderRepository(IDbConnection connection) : base(connection, "Ps_Orders") { }

        // 实现BaseRepository需要的具体方法
        public override async Task<int> AddAsync(PsOrder entity)
        {
            var sql = @"
                INSERT INTO Orders (OrderNo, UserId, UserName, UserType, ProductId, ProductName, 
                                   Quantity, TotalAmount, Status, CreateTime)
                VALUES (@OrderNo, @UserId, @UserName, @UserType, @ProductId, @ProductName, 
                       @Quantity, @TotalAmount, @Status, @CreateTime);
                SELECT SCOPE_IDENTITY();";

            return await _connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public override async Task<bool> UpdateAsync(PsOrder entity)
        {
            var sql = @"
                UPDATE Orders 
                SET Status = @Status,
                    PayTime = @PayTime,
                    TransactionId = @TransactionId
                WHERE Id = @Id";

            var rowsAffected = await _connection.ExecuteAsync(sql, entity);
            return rowsAffected > 0;
        }

        public async Task<PsOrder> GetByOrderNoAsync(long orderNo)
        {
            var sql = "SELECT * FROM Orders WHERE OrderNo = @OrderNo";
            return await _connection.QueryFirstOrDefaultAsync<PsOrder>(sql, new { OrderNo = orderNo });
        }

        public async Task<bool> UpdateStatusAsync(long orderNo, int status, string transactionId = null)
        {
            var sql = @"
                UPDATE Orders 
                SET Status = @Status,
                    PayTime = CASE WHEN @Status = 1 THEN GETDATE() ELSE PayTime END,
                    TransactionId = ISNULL(@TransactionId, TransactionId)
                WHERE OrderNo = @OrderNo";

            var rowsAffected = await _connection.ExecuteAsync(sql,
                new { OrderNo = orderNo, Status = status, TransactionId = transactionId });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsByOrderNoAsync(long orderNo)
        {
            var sql = "SELECT 1 FROM Orders WHERE OrderNo = @OrderNo";
            var result = await _connection.ExecuteScalarAsync<int?>(sql, new { OrderNo = orderNo });
            return result.HasValue;
        }

        public async Task<int> GetUserTodayPurchaseCountAsync(int userId)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM Orders 
                WHERE UserId = @UserId 
                  AND CONVERT(date, CreateTime) = CONVERT(date, GETDATE())";

            return await _connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        }
    }
}
