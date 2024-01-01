# 工程模板

## 简介

本项目是一个基于dotnet new的模板项目，用于快速创建一个基于 [netcorepal-cloud-framework](https://github.com/netcorepal/netcorepal-cloud-framework) 的项目。

项目支持`linux`、`windows`、`macOS`平台。

## 前提条件

1. 安装`.NET 8.0 SDK`或更高版本。

    SDK下载地址： <https://dot.net/download>

2. 拥有`Docker`环境，用于自动化单元测试和集成测试。

    `Docker Desktop`下载地址： <https://www.docker.com/products/docker-desktop/>

## 如何使用

安装模版与创建工程

``` bash
//安装模板
dotnet new -i  NetCorePal.Template

//创建项目
dotnet new netcorepal-web -n My.Project.Name

//进入项目目录
cd My.Project.Name

//构建项目
dotnet build

//运行测试
dotnet test
```



其它命令：

``` bash
//更新模板
dotnet new update
// or 
dotnet new --install <PACKAGE_ID>::<VERSION>

//卸载模板
dotnet new --uninstall NetCorePal.Template
```

## 支持特性（WIP）

+ 文件存储
  + [x] 本地文件
  + [ ] 阿里云对象存储
+ 配置管理
  + [x] 文件配置（json、ini、yaml）
  + [x] Kubernetes ConfigMap
  + [ ] Nacos
  + [ ] Apollo
  + [ ] AgileConfig
+ 数据库
  + [x] InMemory
  + [x] SqlServer
  + [x] MySql
  + [x] PostgreSql
  + [x] Sqlite
+ 消息队列
  + [x] RabbitMQ
  + [x] Kafka
  + [x] RedisStreams
  + [x] AzureServiceBus
  + [x] AmazonSQS
  + [x] Pulsar
+ 服务注册发现
  + [x] Kubernetes
  + [ ] Etcd
  + [ ] Consul
  + [ ] Zookeeper
  + [ ] Eureka
  + [ ] Nacos
+ API工具链
  + [x] Swagger
+ 远程调用
  + [ ] gRPC
  + [x] HttpClient
+ 实时通讯
  + [x] SignalR
  + [x] WebSocket
+ 缓存中间件
  + [x] Redis
+ 熔断限流
  + [ ] Polly
