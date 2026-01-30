using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PurchaseSystem.Data.Redis.Interfaces;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Model.Entities;
using PurchaseSystem.Model.Redis;
using System.Text.Json;

namespace PurchaseSystem.Worker.Services
{
    public class AsyncOrderWorker : BackgroundService
    {
        private readonly IRedisService _redisService;
        private readonly IOrderRepository _orderRepository;

        public AsyncOrderWorker(IRedisService redisService)
        {
            _redisService = redisService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Console.WriteLine("异步订单处理Worker启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. 处理异步订单队列（写入数据库）
                    await ProcessAsyncOrders();

                    // 2. 处理超时订单队列
                    await ProcessTimeoutOrders();

                    // 3. 处理库存恢复队列
                    await ProcessStockRestore();

                    // 休眠1秒
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("异步处理异常:{Message}", ex.Message);
                    await Task.Delay(5000, stoppingToken); // 异常后等待5秒
                }
            }
        }

        private async Task ProcessAsyncOrders()
        {
            // 每次处理最多10个订单
            for (int i = 0; i < 10; i++)
            {
                var orderJson = await _redisService.GetAsyncOrderFromQueueAsync();
                if (string.IsNullOrEmpty(orderJson)) break;

                try
                {
                    var orderData = JsonSerializer.Deserialize<AsyncOrderData>(orderJson);

                    // 创建订单实体
                    var order = new PsOrder
                    {
                        OrderNo = long.Parse(orderData.OrderNo),
                        UserId = orderData.UserId,
                        ProductId = orderData.ProductId,
                        Quantity = 1,
                        TotalAmount = orderData.Price,
                        Status = 0, // 待支付
                        CreateTime = DateTime.Now
                    };

                    // 写入数据库
                    await _orderRepository.AddAsync(order);

                    Console.WriteLine("异步创建订单成功: OrderNo={OrderNo}", orderData.OrderNo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("异步创建订单失败: OrderJson={OrderJson}" + ex.Message, orderJson);
                    // 可以加入重试队列
                }
            }
        }

        private async Task ProcessTimeoutOrders()
        {
            var orderNos = await _redisService.GetTimeoutOrdersAsync(20);

            foreach (var orderNoStr in orderNos)
            {
                if (long.TryParse(orderNoStr, out var orderNo))
                {
                    try
                    {
                        var result = await _redisService.ProcessTimeoutOrderAsync(orderNo);
                        if (result.Success)
                        {
                            Console.WriteLine("超时订单处理成功: OrderNo={OrderNo}", orderNo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("超时订单处理失败: OrderNo={OrderNo} ERROR:" + ex.Message, orderNo);
                    }
                }
            }
        }

        private async Task ProcessStockRestore()
        {
            // 每次处理最多5个库存恢复任务
            for (int i = 0; i < 5; i++)
            {
                var taskJson = await _redisService.GetStockRestoreTaskAsync();
                if (string.IsNullOrEmpty(taskJson)) break;

                try
                {
                    var taskData = JsonSerializer.Deserialize<StockRestoreData>(taskJson);
                    var result = await _redisService.RestoreStockAsync(
                        taskData.ProductId, taskData.OrderNo);

                    if (result.Success)
                    {
                        Console.WriteLine("库存恢复成功: ProductId={ProductId}, OrderNo={OrderNo}",
                            taskData.ProductId, taskData.OrderNo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("库存恢复失败: TaskJson={TaskJson} ERROR:", taskJson);
                }
            }
        }
    }
}
