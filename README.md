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

1. 安装`.NET 9.0 SDK`或更高版本。

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
