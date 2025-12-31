# ABC.Template

## 环境准备

### 使用 Aspire（推荐）

如果您的项目启用了 Aspire 支持（使用 `--UseAspire` 参数创建），只需要 Docker 环境即可，无需手动配置各种基础设施服务。

```bash
# 仅需确保 Docker 环境运行
docker version

# 直接运行 AppHost 项目，Aspire 会自动管理所有依赖服务
cd src/ABC.Template.AppHost
dotnet run
```

Aspire 会自动为您：
- 启动和管理数据库容器（MySQL、SQL Server、PostgreSQL、MongoDB 等）
- 启动和管理消息队列容器（RabbitMQ、Kafka、NATS 等）
- 启动和管理 Redis 容器
- 提供统一的 Aspire Dashboard 界面查看所有服务状态
- 自动配置服务间的连接字符串和依赖关系

访问 Aspire Dashboard（通常在 http://localhost:15888）可以查看和管理所有服务。

### 推荐方式：使用初始化脚本（不使用 Aspire 时）

如果您没有启用 Aspire，项目提供了完整的基础设施初始化脚本，支持快速搭建开发环境：

#### 使用 Docker Compose（推荐）
```bash
# 进入脚本目录
cd scripts

# 启动默认基础设施 (MySQL + Redis + RabbitMQ)
docker-compose up -d

# 使用 SQL Server 替代 MySQL
docker-compose --profile sqlserver up -d

# 使用 PostgreSQL 替代 MySQL  
docker-compose --profile postgres up -d

# 使用 Kafka 替代 RabbitMQ
docker-compose --profile kafka up -d

# 停止所有服务
docker-compose down

# 停止并删除数据卷（完全清理）
docker-compose down -v
```

#### 使用初始化脚本
```bash
# Linux/macOS
cd scripts
./init-infrastructure.sh

# Windows PowerShell
cd scripts
.\init-infrastructure.ps1

# 清理环境
./clean-infrastructure.sh        # Linux/macOS
.\clean-infrastructure.ps1       # Windows
```

### 手动方式：单独运行 Docker 容器

如果需要手动控制每个容器，可以使用以下命令：

```bash
# Redis
docker run --restart unless-stopped --name netcorepal-redis -p 6379:6379 -v netcorepal_redis_data:/data -d redis:7.2-alpine redis-server --appendonly yes --databases 1024

# MySQL
docker run --restart unless-stopped --name netcorepal-mysql -p 3306:3306 -e MYSQL_ROOT_PASSWORD=123456 -e MYSQL_CHARACTER_SET_SERVER=utf8mb4 -e MYSQL_COLLATION_SERVER=utf8mb4_unicode_ci -e TZ=Asia/Shanghai -v netcorepal_mysql_data:/var/lib/mysql -d mysql:8.0

# RabbitMQ
docker run --restart unless-stopped --name netcorepal-rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest -v netcorepal_rabbitmq_data:/var/lib/rabbitmq -d rabbitmq:3.12-management-alpine
```

### 服务访问信息

启动后，可以通过以下地址访问各个服务：

- **Redis**: `localhost:6379`
- **MySQL**: `localhost:3306` (root/123456)  
- **RabbitMQ AMQP**: `localhost:5672` (guest/guest)
- **RabbitMQ 管理界面**: http://localhost:15672 (guest/guest)
- **SQL Server**: `localhost:1433` (sa/Test123456!)
- **PostgreSQL**: `localhost:5432` (postgres/123456)
- **Kafka**: `localhost:9092`
- **Kafka UI**: http://localhost:8080

## IDE 代码片段配置

本模板提供了丰富的代码片段，帮助您快速生成常用的代码结构。

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

更多详细配置请参考：[vs-snippets/README.md](vs-snippets/README.md)

## 依赖对框架与组件

+ [NetCorePal Cloud Framework](https://github.com/netcorepal/netcorepal-cloud-framework)
+ [ASP.NET Core](https://github.com/dotnet/aspnetcore)
+ [EFCore](https://github.com/dotnet/efcore)
+ [CAP](https://github.com/dotnetcore/CAP)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [FluentValidation](https://docs.fluentvalidation.net/en/latest)
+ [Swashbuckle.AspNetCore.Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## 数据库迁移

```shell
# 安装工具  SEE： https://learn.microsoft.com/zh-cn/ef/core/cli/dotnet#installing-the-tools
dotnet tool install --global dotnet-ef --version 9.0.0

# 强制更新数据库
dotnet ef database update -p src/ABC.Template.Infrastructure 

# 创建迁移 SEE：https://learn.microsoft.com/zh-cn/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
dotnet ef migrations add InitialCreate -p src/ABC.Template.Infrastructure 
```

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
cd src/ABC.Template.Web
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

## 关于监控

这里使用了`prometheus-net`作为与基础设施prometheus集成的监控方案，默认通过地址 `/metrics` 输出监控指标。

更多信息请参见：[https://github.com/prometheus-net/prometheus-net](https://github.com/prometheus-net/prometheus-net)


