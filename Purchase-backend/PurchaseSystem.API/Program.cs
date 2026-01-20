using PurchaseSystem.API.Middleware;
using PurchaseSystem.Core.Services.IService;
using PurchaseSystem.Core.Services.Service;
using PurchaseSystem.Data.Redis.Interfaces;
using PurchaseSystem.Data.Redis.Services;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Data.Repositories.Repository;
using PurchaseSystem.Worker.Services;
using StackExchange.Redis;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server数据库连接
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// 注册仓储
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

//  注册Redis服务
builder.Services.AddSingleton<IRedisService>(sp =>
    new RedisService(builder.Configuration.GetConnectionString("RedisConnection")));


// 注册业务服务
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 注册后台Worker（如果需要）
builder.Services.AddHostedService<AsyncOrderWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 全局异常处理
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

// CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// 10. 启动日志
app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("🚀 抢购系统启动成功！");
    Console.WriteLine("📊 请确保已执行以下操作：");
    Console.WriteLine("   1. Redis数据预热完成");
    Console.WriteLine("   2. 商品库存已加载到Redis");
    Console.WriteLine("   3. 数据库连接正常");
});

app.Run();
