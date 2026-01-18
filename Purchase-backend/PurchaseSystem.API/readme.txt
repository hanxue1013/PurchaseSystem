PurchaseSystem.API/
├── Controllers/           # 控制器
│   ├── ProductController.cs      # 商品相关（查询、列表）
│   ├── PurchaseController.cs     # 抢购接口（核心）
│   └── PaymentController.cs      # 支付回调
├── Middleware/           # 中间件
│   └── RateLimitMiddleware.cs    # 限流中间件
└── Program.cs            # 启动配置