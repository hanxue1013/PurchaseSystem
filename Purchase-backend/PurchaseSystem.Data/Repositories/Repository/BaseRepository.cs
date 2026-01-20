using Dapper;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Model.Entities;
using System.Data;
using System.Text.Json.Serialization;

namespace PurchaseSystem.Data.Repositories.Repository
{
    /// <summary>
    /// 基础仓储
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly IDbConnection _connection;
        protected readonly string _tableName;

        protected BaseRepository(IDbConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            var sql = $"SELECT * FROM {_tableName} ORDER BY CreateTime DESC";
            return await _connection.QueryAsync<T>(sql);
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyNameAttribute)))
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var values = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values}); SELECT SCOPE_IDENTITY()";

            return await _connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(JsonPropertyNameAttribute)))
                .Select(p => $"{p.Name} = @{p.Name}");

            var setClause = string.Join(", ", properties);
            var sql = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

            var affectedRows = await _connection.ExecuteAsync(sql, entity);
            return affectedRows > 0;
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var sql = $"DELETE FROM {_tableName} WHERE Id = @Id";
            var affectedRows = await _connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}
