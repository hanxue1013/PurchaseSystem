PurchaseSystem.Data/
├── Repositories/         # 数据仓储
│   ├── IRepository/       # 仓储接口
│   │   ├── IOrderRepository.cs
│   │   └── IProductRepository.cs
│   └── Repositorie/        # SQL Server实现
│       ├── OrderRepository.cs
│       └── ProductRepository.cs
├── Redis/               # Redis操作
│   ├── Interfaces/
│   │   ├── IRedisService.cs
│   │   └── IRedisQueueService.cs
│   ├── Services/
│   │   ├── RedisService.cs         # 基础操作
│   │   └── RedisQueueService.cs    # 队列操作
│   ├── LuaScripts/      # Lua脚本
│   │   ├── StockDeduct.lua         # 库存扣减脚本
│   │   └── StockRestore.lua        # 库存恢复脚本
├── Database/            # 数据库相关
│   ├── DbConnectionFactory.cs    # 连接工厂
│   └── SqlHelper.cs              # Dapper扩展