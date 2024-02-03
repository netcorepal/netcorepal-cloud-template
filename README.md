# 工程模板

[![Release Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-template/release.yml?label=release%20build)](https://github.com/netcorepal/netcorepal-cloud-template/actions/workflows/release.yml)
[![Preview Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-template/dotnet.yml?label=preview%20build)](https://github.com/netcorepal/netcorepal-cloud-template/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NetCorePal.Template.svg)](https://www.nuget.org/packages/NetCorePal.Template)
[![MyGet Preview](https://img.shields.io/myget/netcorepal/vpre/NetCorePal.Template?label=preview)](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Template)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/netcorepal/netcorepal-cloud-template/blob/main/LICENSE)

## 简介

本项目是一个基于dotnet new的模板项目，用于快速创建一个基于 [netcorepal-cloud-framework](https://github.com/netcorepal/netcorepal-cloud-framework) 的项目。

项目支持`linux`、`windows`、`macOS`平台。

## 前提条件

1. 安装`.NET 8.0 SDK`或更高版本。

    SDK下载地址： <https://dot.net/download>

2. 拥有`Docker`环境，用于自动化单元测试和集成测试。

    `Docker Desktop`下载地址： <https://www.docker.com/products/docker-desktop/>

## 如何使用

安装模板

``` shell
dotnet new install NetCorePal.Template
```

安装Preview版本

``` shell
dotnet new install NetCorePal.Template::<package-version> --add-source "https://www.myget.org/F/netcorepal/api/v3/index.json"
```


创建项目

```shell
dotnet new netcorepal-web -n My.Project.Name
```

进入项目目录

```shell
cd My.Project.Name
```

构建项目

```shell
dotnet build
```

运行测试

```shell
dotnet test
```

## 其它命令

更新模板

``` shell
dotnet new update
```

or

```shell
dotnet new install NetCorePal.Template::<VERSION>
```

卸载模板

```shell
dotnet new uninstall NetCorePal.Template
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
