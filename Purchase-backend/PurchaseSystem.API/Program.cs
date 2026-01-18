using PurchaseSystem.API.Middleware;
using PurchaseSystem.Core.Services.IService;
using PurchaseSystem.Core.Services.Service;
using PurchaseSystem.Data.Redis.Interfaces;
using PurchaseSystem.Data.Redis.Services;
using PurchaseSystem.Data.Repositories.IRepository;
using PurchaseSystem.Data.Repositories.Repository;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 数据库连接
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// 注册仓储
builder.Services.AddScoped<IProductRepository, ProductRepository>();

//  注册Redis服务
builder.Services.AddSingleton<IRedisService>(sp =>
    new RedisService(builder.Configuration.GetConnectionString("RedisConnection")));


// 注册业务服务
builder.Services.AddScoped<IProductService, ProductService>();

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

// CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
