# 工程模板

``` bat
//安装模板
dotnet new -i  ABC.Template  --nuget-source https://www.myget.org/F/netcorepal/api/v3/index.json

//更新模板
dotnet new --update-check 
dotnet new --update-apply 
// or 
dotnet new --install <PACKAGE_ID>::<VERSION>

//卸载模板
dotnet new --uninstall ABC.Template


//创建项目

dotnet new abcweb -n My.Project.Name -o my-project-folder-name
```


## 支持特性

+ 文件存储
    - [x] 本地文件
    - [ ] 阿里云对象存储
+ 配置管理
    - [ ] 文件配置（json、ini、yaml）
    - [x] Kubernetes ConfigMap
    - [ ] Nacos
    - [ ] Apollo
+ 数据库
    - [x] InMemory
    - [ ] SqlServer
    - [x] MySql
    - [ ] PostgreSql
    - [ ] Sqlite
+ 消息队列
    - [x] RabbitMQ
    - [ ] Kafka
    - [ ] RocketMQ
    - [ ] RedisStreams
    - [ ] AzureServiceBus
    - [ ] AmazonSQS
    - [ ] Pulsar
+ 服务注册发现
    - [x] Kubernetes
    - [ ] Etcd
    - [ ] Consul
    - [ ] Zookeeper
    - [ ] Eureka
    - [ ] Nacos
+ API工具链
    - [x] Swagger
+ 远程调用
    - [ ] gRPC
    - [x] HttpClient
+ 实时通讯
    - [x] SignalR
    - [ ] WebSocket
+ 缓存中间件
    - [ ] Redis
+ 熔断限流
    - [ ] Polly