using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseSystem.Data.Redis.Interfaces
{
    public interface IRedisService
    {
        Task PreloadStockAsync(int productId, int totalStock, int shardCount = 1);
    }
}
