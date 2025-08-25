# Copilot 编程助手指南

## 仓库概述

这是一个基于 NetCorePal Cloud Framework 的 **领域驱动设计 (DDD) 模板**，用于构建 ASP.NET Core 应用程序。该仓库实现了严格的 DDD 战术模式，提供了完整的开发工作流指导。

**核心技术栈：**
- .NET 9.0 SDK 
- ASP.NET Core Web API
- Entity Framework Core + MySQL/Pomelo
- MediatR (CQRS 模式)
- FastEndpoints (最小化 API)

## 项目结构

```
ABC.Template.sln
├── src/
│   ├── ABC.Template.Domain/         # 领域层 - 聚合根、实体、领域事件
│   ├── ABC.Template.Infrastructure/ # 基础设施层 - EF配置、仓储实现
│   └── ABC.Template.Web/           # 表现层 - API、应用服务
└── test/                            # 测试项目
    ├── ABC.Template.Domain.UnitTests/         # 领域层测试项目
    ├── ABC.Template.Infrastructure.UnitTests/ # 基础设施层测试项目
    └── ABC.Template.Web.UnitTests/           # 表现层测试项目
```

**分层依赖关系：** Web → Infrastructure → Domain (严格单向依赖)

## 开发主要工作项目

- 创建聚合
- 定义领域事件
- 创建仓储
- 配置实体映射
- 创建命令与命令处理器
- 创建Endpoints
- 创建领域事件处理器
- 创建集成事件
- 创建集成事件转换器
- 创建集成事件处理器

## 核心开发原则

### 文件组织
- **聚合根** → `src/ABC.Template.Domain/AggregatesModel/{AggregateFolder}/`
- **领域事件** → `src/ABC.Template.Domain/DomainEvents/`
- **仓储** → `src/ABC.Template.Infrastructure/Repositories/`
- **实体配置** → `src/ABC.Template.Infrastructure/EntityConfigurations/`
- **命令与命令处理器** → `src/ABC.Template.Web/Application/Commands/`
- **API端点** → `src/ABC.Template.Web/Endpoints/`
- **领域事件处理器** → `src/ABC.Template.Web/Application/DomainEventHandlers/`
- **集成事件** → `src/ABC.Template.Web/Application/IntegrationEvents/`
- **集成事件转换器** → `src/ABC.Template.Web/Application/IntegrationEventConverters/`
- **集成事件处理器** → `src/ABC.Template.Web/Application/IntegrationEventHandlers/`

### 强制性要求
- ✅ 所有聚合根使用强类型ID
- ✅ 所有命令都要有对应的验证器
- ✅ 领域事件在聚合根状态改变时发布
- ✅ 遵循分层架构依赖关系 (Web → Infrastructure → Domain)
- ✅ 使用KnownException处理已知业务异常

## 异常处理原则

### KnownException使用规范
在需要抛出业务异常的地方，必须使用 `KnownException` 而不是普通的 `Exception`：

**正确示例：**
```csharp
// 在聚合根中
public void OrderPaid()
{
    if (Paid)
    {
        throw new KnownException("Order has been paid");
    }
    // 业务逻辑...
}

// 在命令处理器中
public async Task Handle(OrderPaidCommand request, CancellationToken cancellationToken)
{
    var order = await orderRepository.GetAsync(request.OrderId, cancellationToken) ??
                throw new KnownException($"未找到订单，OrderId = {request.OrderId}");
    order.OrderPaid();
}
```

**框架集成：**
- `KnownException` 会被框架自动转换为合适的HTTP状态码
- 异常消息会直接返回给客户端
- 支持本地化和错误码定制
