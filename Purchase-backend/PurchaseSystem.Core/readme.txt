PurchaseSystem.Core/
├── Services/             # 业务服务
│   ├── IService/       # 服务接口
│   │   ├── IPurchaseService.cs     # 抢购服务
│   │   ├── IPaymentService.cs      # 支付服务
│   │   └── IProductService.cs      # 商品服务
│   └── Service/  # 服务实现
│       ├── PurchaseService.cs      # 核心抢购逻辑
│       ├── PaymentService.cs
│       └── ProductService.cs
├── Interfaces/           # 核心接口
│   ├── IStockManager.cs  # 库存管理器接口
│   └── IOrderManager.cs  # 订单管理器接口
└── Common/               # 公共组件
    ├── Exceptions/       # 业务异常
    └── Constants/        # 常量定义