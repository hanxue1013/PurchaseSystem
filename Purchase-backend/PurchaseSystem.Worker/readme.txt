PurchaseSystem.Worker/
├── Services/            # 后台服务
│   ├── OrderTimeoutService.cs    # 订单超时处理
│   ├── OrderSyncService.cs       # 订单数据同步
│   └── StockSyncService.cs       # 库存同步
├── Jobs/               # 定时任务
│   ├── TimeoutOrderJob.cs        # 超时订单扫描
│   └── DataSyncJob.cs            # 数据同步任务
└── Program.cs          # 启动入口