# 工程模板

[![Release Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-template/release.yml?label=release%20build)](https://github.com/netcorepal/netcorepal-cloud-template/actions/workflows/release.yml)
[![Preview Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-template/dotnet.yml?label=preview%20build)](https://github.com/netcorepal/netcorepal-cloud-template/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NetCorePal.Template.svg)](https://www.nuget.org/packages/NetCorePal.Template)
[![NuGet Version](https://img.shields.io/nuget/vpre/NetCorePal.Template?label=nuget-pre)](https://www.nuget.org/packages/NetCorePal.Template)
[![MyGet Version](https://img.shields.io/myget/netcorepal/vpre/NetCorePal.Template?label=myget-nightly)](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Template)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/netcorepal/netcorepal-cloud-template/blob/main/LICENSE)

## 简介

本项目是一个基于dotnet new的模板项目，用于快速创建一个基于 [netcorepal-cloud-framework](https://github.com/netcorepal/netcorepal-cloud-framework) 的项目。

项目支持`linux`、`windows`、`macOS`平台。

## 前提条件

1. 安装`.NET 10 SDK`（必需）。根据创建项目时选择的目标框架版本（`--Framework` 参数），您可能还需要安装对应的 SDK 版本：

    - 使用 `--Framework net8.0` 时：需要安装 `.NET 8.0 SDK`
    - 使用 `--Framework net9.0` 时：需要安装 `.NET 9.0 SDK`
    - 使用 `--Framework net10.0` 时：仅需 `.NET 10 SDK`（默认）

    SDK下载地址： <https://dot.net/download>

2. 拥有`Docker`环境，用于自动化单元测试和集成测试。

    推荐工具：
    
    - **Docker Desktop**： <https://www.docker.com/products/docker-desktop/>
    - **Rancher Desktop**： <https://github.com/rancher-sandbox/rancher-desktop>
    - **OrbStack**： <https://orbstack.dev/download>

3. 如果使用 Aspire（使用 `--UseAspire` 参数创建项目时），需要配置 Aspire 环境。请按以下步骤操作：

    **步骤 1：安装 Aspire SDK**

    访问 [Aspire CLI 安装指南](https://aspire.dev/get-started/install-cli/) 按照官方文档安装 Aspire SDK。

    **步骤 2：更新工作负载**

    ```shell
    dotnet workload update
    ```

    **步骤 3：信任 HTTPS 开发证书**

    ```shell
    dotnet dev-certs https --trust
    ```

## 如何使用

安装模板

``` shell
dotnet new install NetCorePal.Template
```

安装Preview版本

``` shell
dotnet new install NetCorePal.Template::<package-version>"

# 例如
dotnet new install NetCorePal.Template::3.3.0
```

安装Nightly版本

``` shell
dotnet new install NetCorePal.Template::<package-version> --add-source "https://www.myget.org/F/netcorepal/api/v3/index.json"
```


创建项目

```shell
dotnet new netcorepal-web -n My.Project.Name
```

### 命令参数说明

模板支持多个参数来定制生成的项目，您可以根据需要选择不同的技术栈：

#### 基本语法

```shell
dotnet new netcorepal-web -n <项目名称> [参数选项]
```

#### 获取帮助

```shell
# 查看所有可用的参数和选项
dotnet new netcorepal-web --help

# 查看所有已安装的模板
dotnet new list
```

#### 可用参数

| 参数 | 短参数 | 说明 | 可选值 | 默认值 |
|------|--------|------|--------|--------|
| `--Framework` | `-F` | 目标 .NET 框架版本 | `net8.0`, `net9.0`, `net10.0` | `net10.0` |
| `--Database` | `-D` | 数据库提供程序 | `MySql`, `SqlServer`, `PostgreSQL`, `Sqlite`, `GaussDB`, `DMDB`, `MongoDB` | `MySql` |
| `--MessageQueue` | `-M` | 消息队列提供程序 | `RabbitMQ`, `Kafka`, `AzureServiceBus`, `AmazonSQS`, `NATS`, `RedisStreams`, `Pulsar` | `RabbitMQ` |
| `--UseAspire` | `-U` | 启用 Aspire Dashboard 支持 | `true`, `false` | `false` |
| `--IncludeCopilotInstructions` | `-I` | 是否包含 Copilot 指令文件 | `true`, `false` | `true` |
| `--UseAdmin` | | 是否包含 Admin 功能（用户、角色、部门管理、权限系统、前端） | `true`, `false` | `false` |

#### 使用示例

```shell
# 使用默认配置（.NET 10.0 + MySQL + RabbitMQ）
dotnet new netcorepal-web -n My.Project.Name

# 使用 .NET 8.0 框架
dotnet new netcorepal-web -n My.Project.Name --Framework net8.0
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -F net8.0

# 使用 SQL Server 数据库
dotnet new netcorepal-web -n My.Project.Name --Database SqlServer
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -D SqlServer

# 使用 PostgreSQL 数据库
dotnet new netcorepal-web -n My.Project.Name --Database PostgreSQL

# 使用 SQLite 数据库（轻量级文件数据库，适合开发和测试）
dotnet new netcorepal-web -n My.Project.Name --Database Sqlite
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -D Sqlite

# 使用 MongoDB 数据库（NoSQL 文档数据库）
dotnet new netcorepal-web -n My.Project.Name --Database MongoDB
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -D MongoDB

# 使用 Kafka 消息队列
dotnet new netcorepal-web -n My.Project.Name --MessageQueue Kafka
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -M Kafka

# 组合使用多个参数（推荐使用短参数）
dotnet new netcorepal-web -n My.Project.Name -F net8.0 -D PostgreSQL -M Kafka

# 使用云服务（Azure Service Bus）
dotnet new netcorepal-web -n My.Project.Name -M AzureServiceBus

# 使用 Redis Streams 作为消息队列
dotnet new netcorepal-web -n My.Project.Name -M RedisStreams

# 启用 Aspire Dashboard 支持（用于可观测性和编排）
dotnet new netcorepal-web -n My.Project.Name --UseAspire
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -U

# 组合使用 Aspire 与其他选项
dotnet new netcorepal-web -n My.Project.Name -F net9.0 -D PostgreSQL -U

# 排除 Copilot 指令文件（不包含 .github 目录中的 copilot-instructions）
dotnet new netcorepal-web -n My.Project.Name --IncludeCopilotInstructions false
# 或使用短参数
dotnet new netcorepal-web -n My.Project.Name -I false
```

> **提示：** 创建项目后，请根据选择的数据库和消息队列配置，使用对应的基础设施初始化脚本来启动所需的服务。详细说明请参考生成项目中的 `scripts/README.md` 文件。

### 关于 Admin 功能

当启用 `--UseAdmin` 选项时，模板会包含以下功能：

+ **用户管理**：用户的创建、更新、删除、查询等功能
+ **角色管理**：角色的创建、更新、删除、权限分配等功能
+ **部门管理**：部门的创建、更新、删除、树形结构管理等功能
+ **权限系统**：基于角色的权限控制（RBAC），支持权限码定义和验证
+ **JWT 认证**：完整的 JWT Token 认证和刷新 Token 机制
+ **前端界面**：基于 Vue 3 + Vben Admin 的管理后台界面

启用 Admin 功能后，系统会自动创建默认的管理员账户（用户名：`admin`，密码：`123456`）。

### 关于 Aspire Dashboard

当启用 `--UseAspire` 选项时，模板会生成以下额外的项目：

+ **AppHost 项目**：用于应用程序的编排和启动，集成了 Aspire Dashboard
+ **ServiceDefaults 项目**：包含共享的服务配置，如 OpenTelemetry、服务发现、健康检查等

启用 Aspire Dashboard 后，可以通过运行 AppHost 项目来启动应用程序并访问 Dashboard：

```shell
cd src/My.Project.Name.AppHost
dotnet run
```

Aspire Dashboard 提供了以下功能：

+ **分布式追踪**：可视化查看请求在不同服务间的流转
+ **指标监控**：实时监控应用程序的性能指标
+ **日志聚合**：集中查看所有服务的日志
+ **服务健康检查**：监控各服务的健康状态
+ **资源管理**：查看和管理应用程序使用的资源

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

## 本地开发

如果您需要基于源码开发或调试本模板，可以按以下步骤操作：

### 克隆仓库

```bash
git clone https://github.com/netcorepal/netcorepal-cloud-template.git
cd netcorepal-cloud-template
```

### 从本地源码安装模板

#### 方式一：直接从源码目录安装（推荐）

`dotnet new install` 支持直接从包含模板的源码目录安装，无需构建和打包：

```bash
# 建议先清理旧文件以避免冲突
git clean -fdX

# 在项目根目录直接安装（推荐）
dotnet new install . --force
```

`--force` 参数可以强制安装或更新模板，无需先卸载旧版本。这种方式最简单快捷，特别适合本地开发和调试。

修改模板文件后，重新运行安装命令即可更新：

```bash
# 修改模板文件后，直接重新安装
dotnet new install . --force
```

#### 方式二：从构建的包安装

如果需要测试完整的打包流程，可以先构建再安装：

```bash
# 恢复依赖
dotnet restore

# 构建项目
dotnet build

# 打包模板
dotnet pack

# 从生成的 nupkg 文件安装
dotnet new install ./bin/Debug/NetCorePal.Template.1.0.0.nupkg

# 或者使用通配符安装
dotnet new install ./bin/Debug/*.nupkg
```

### 测试本地模板

安装后，您可以像使用正式版本一样创建项目来测试模板：

```bash
# 使用本地安装的模板创建项目
dotnet new netcorepal-web -n TestProject

# 进入测试项目目录
cd TestProject

# 构建和测试
dotnet build
dotnet test
```

### 查看已安装的模板

```bash
# 列出所有已安装的模板
dotnet new list

# 查看模板详细信息
dotnet new netcorepal-web --help
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

## IDE 代码片段配置

本模板提供了丰富的代码片段，帮助您快速生成常用的代码结构。

![代码片段演示](docs/snippets.gif)

### Visual Studio 配置

运行以下 PowerShell 命令自动安装代码片段：

```powershell
cd vs-snippets
.\Install-VSSnippets.ps1
```

或者手动安装：

1. 打开 Visual Studio
2. 转到 `工具` > `代码片段管理器`
3. 导入 `vs-snippets/NetCorePalTemplates.snippet` 文件

### VS Code 配置

VS Code 的代码片段已预配置在 `.vscode/csharp.code-snippets` 文件中，打开项目时自动生效。

### JetBrains Rider 配置

Rider 用户可以直接使用 `ABC.Template.sln.DotSettings` 文件中的 Live Templates 配置。

### 可用的代码片段

#### NetCorePal (ncp) 快捷键

| 快捷键 | 描述 | 生成内容 |
|--------|------|----------|
| `ncpcmd` | NetCorePal 命令 | ICommand 实现(含验证器和处理器) |
| `ncpcmdres` | 命令(含返回值) | ICommand&lt;Response&gt; 实现 |
| `ncpar` | 聚合根 | Entity&lt;Id&gt; 和 IAggregateRoot |
| `ncprepo` | NetCorePal 仓储 | IRepository 接口和实现 |
| `ncpie` | 集成事件 | IntegrationEvent 和处理器 |
| `ncpdeh` | 域事件处理器 | IDomainEventHandler 实现 |
| `ncpiec` | 集成事件转换器 | IIntegrationEventConverter |
| `ncpde` | 域事件 | IDomainEvent 记录 |

#### Endpoint (ep) 快捷键

| 快捷键 | 描述 | 生成内容 |
|--------|------|----------|
| `epp` | FastEndpoint(NCP风格) | 完整的垂直切片实现 |
| `epreq` | 仅请求端点 | Endpoint&lt;Request&gt; |
| `epres` | 仅响应端点 | EndpointWithoutRequest&lt;Response&gt; |
| `epdto` | 端点 DTOs | Request 和 Response 类 |
| `epval` | 端点验证器 | Validator&lt;Request&gt; |
| `epmap` | 端点映射器 | Mapper&lt;Request, Response, Entity&gt; |
| `epfull` | 完整端点切片 | 带映射器的完整实现 |
| `epsum` | 端点摘要 | Summary&lt;Endpoint, Request&gt; |
| `epnoreq` | 无请求端点 | EndpointWithoutRequest |
| `epreqres` | 请求响应端点 | Endpoint&lt;Request, Response&gt; |
| `epdat` | 端点数据 | 静态数据类 |

## 代码分析可视化

框架提供了强大的代码流分析和可视化功能，帮助开发者直观地理解DDD架构中的组件关系和数据流向。

### 🎯 核心特性

+ **自动代码分析**：通过源生成器自动分析代码结构，识别控制器、命令、聚合根、事件等组件
+ **多种图表类型**：支持架构流程图、命令链路图、事件流程图、类图等多种可视化图表
+ **交互式HTML可视化**：生成完整的交互式HTML页面，内置导航和图表预览功能
+ **一键在线编辑**：集成"View in Mermaid Live"按钮，支持一键跳转到在线编辑器

### 🚀 快速开始

安装命令行工具来生成独立的HTML文件：

```bash
# 安装全局工具
dotnet tool install -g NetCorePal.Extensions.CodeAnalysis.Tools

# 进入项目目录并生成可视化文件
cd My.Project.Name/src/My.Project.Name.Web
netcorepal-codeanalysis generate --output architecture.html
```

### ✨ 主要功能

+ **交互式HTML页面**：
  + 左侧树形导航，支持不同图表类型切换
  + 内置Mermaid.js实时渲染
  + 响应式设计，适配不同设备
  + 专业的现代化界面

+ **一键在线编辑**：
  + 每个图表右上角的"View in Mermaid Live"按钮
  + 智能压缩算法优化URL长度
  + 自动跳转到[Mermaid Live Editor](https://mermaid.live/)
  + 支持在线编辑、导出图片、生成分享链接

### 📖 详细文档

完整的使用说明和示例请参考：

+ [代码流分析文档](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-flow-analysis/)
+ [代码分析工具文档](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-analysis-tools/)

## 支持特性

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
  + [x] SqlServer
  + [x] MySql
  + [x] PostgreSQL
  + [x] Sqlite
  + [x] GaussDB
  + [x] DMDB
  + [x] MongoDB
+ 消息队列
  + [x] RabbitMQ
  + [x] Kafka
  + [x] NATS
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
  + [x] Polly

备注：未打勾表示该模板未集成该特性，但可手动添加。