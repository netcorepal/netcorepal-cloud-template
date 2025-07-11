# ABC.Template

## 环境准备

```
docker run --restart always --name mysql -v /mnt/d/docker/mysql/data:/var/lib/mysql -e MYSQL_ROOT_PASSWORD=123456 -p 3306:3306 -d mysql:latest

docker run --restart always -d --hostname node1 --name rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3-management

docker run --restart always --name redis -v /mnt/d/docker/redis:/data -p 6379:6379 -d redis:5.0.7 redis-server

```

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


