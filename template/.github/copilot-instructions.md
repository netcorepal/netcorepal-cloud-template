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
- 创建查询与查询处理器
- 创建Endpoints
- 创建领域事件处理器
- 创建集成事件
- 创建集成事件转换器
- 创建集成事件处理器

## 详细开发指南

对于具体的开发工作，请参考以下详细指令文件：

### 聚合与领域层
- **聚合根开发**: 参考 `.github/instructions/aggregate.instructions.md`
- **领域事件定义**: 参考 `.github/instructions/domain-event.instructions.md`

### 数据访问层
- **仓储实现**: 参考 `.github/instructions/repository.instructions.md`
- **实体配置**: 参考 `.github/instructions/entity-configuration.instructions.md`
- **数据库上下文**: 参考 `.github/instructions/dbcontext.instructions.md`

### 应用服务层
- **命令处理**: 参考 `.github/instructions/command.instructions.md`
- **查询处理**: 参考 `.github/instructions/query.instructions.md`
- **领域事件处理**: 参考 `.github/instructions/domain-event-handler.instructions.md`

### API表现层
- **API端点**: 参考 `.github/instructions/endpoint.instructions.md`

### 集成事件处理
- **集成事件**: 参考 `.github/instructions/integration-event.instructions.md`
- **集成事件转换器**: 参考 `.github/instructions/integration-event-converter.instructions.md`
- **集成事件处理器**: 参考 `.github/instructions/integration-event-handler.instructions.md`

### 测试
- **单元测试**: 参考 `.github/instructions/unit-testing.instructions.md`

### 最佳实践
- **通用最佳实践**: 参考 `.github/instructions/best-practices.instructions.md`

## 核心开发原则

### 文件组织
- **聚合根** → `src/ABC.Template.Domain/AggregatesModel/{AggregateFolder}/`
- **领域事件** → `src/ABC.Template.Domain/DomainEvents/`
- **仓储** → `src/ABC.Template.Infrastructure/Repositories/`
- **实体配置** → `src/ABC.Template.Infrastructure/EntityConfigurations/`
- **命令与命令处理器** → `src/ABC.Template.Web/Application/Commands/`
- **查询与查询处理器** → `src/ABC.Template.Web/Application/Queries/`
- **API端点** → `src/ABC.Template.Web/Endpoints/`
- **领域事件处理器** → `src/ABC.Template.Web/Application/DomainEventHandlers/`
- **集成事件** → `src/ABC.Template.Web/Application/IntegrationEvents/`
- **集成事件转换器** → `src/ABC.Template.Web/Application/IntegrationEventConverters/`
- **集成事件处理器** → `src/ABC.Template.Web/Application/IntegrationEventHandlers/`

### 强制性要求
- ✅ 所有聚合根使用强类型ID，且**不手动赋值ID**（依赖EF值生成器）
- ✅ 所有命令都要有对应的验证器
- ✅ 领域事件在聚合根状态改变时发布
- ✅ 遵循分层架构依赖关系 (Web → Infrastructure → Domain)
- ✅ 使用KnownException处理已知业务异常
- ✅ 命令处理器**不调用SaveChanges**（框架自动处理）
- ✅ 仓储必须使用**异步方法**（GetAsync、AddAsync等）

### 关键技术要求
- **验证器**: 必须继承 `AbstractValidator<T>` 而不是 `Validator<T>`
- **领域事件处理器**: 实现 `Handle()` 方法而不是 `HandleAsync()`
- **FastEndpoints**: 使用构造函数注入 `IMediator`，使用 `Send.OkAsync()` 和 `.AsResponseData()`
- **强类型ID**: 直接使用类型，避免 `.Value` 属性，依赖隐式转换
- **仓储**: 通过构造函数参数访问 `ApplicationDbContext`，所有操作必须异步
- **ID生成**: 使用EF Core值生成器，聚合根构造函数不设置ID

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
public async Task<OrderId> Handle(OrderPaidCommand request, CancellationToken cancellationToken)
{
    var order = await orderRepository.GetAsync(request.OrderId, cancellationToken) ??
                throw new KnownException($"未找到订单，OrderId = {request.OrderId}");
    order.OrderPaid();
    return order.Id;
}
```

**框架集成：**
- `KnownException` 会被框架自动转换为合适的HTTP状态码
- 异常消息会直接返回给客户端
- 支持本地化和错误码定制

## 常见using引用指南

### GlobalUsings.cs配置
各层的常用引用已在GlobalUsings.cs中全局定义：

**Web层** (`src/ABC.Template.Web/GlobalUsings.cs`):
- `global using FluentValidation;` - 验证器
- `global using MediatR;` - 命令处理器  
- `global using NetCorePal.Extensions.Primitives;` - KnownException等
- `global using FastEndpoints;` - API端点
- `global using NetCorePal.Extensions.Dto;` - ResponseData
- `global using NetCorePal.Extensions.Domain;` - 领域事件处理器

**Infrastructure层** (`src/ABC.Template.Infrastructure/GlobalUsings.cs`):
- `global using Microsoft.EntityFrameworkCore;` - EF Core
- `global using Microsoft.EntityFrameworkCore.Metadata.Builders;` - 实体配置
- `global using NetCorePal.Extensions.Primitives;` - 基础类型

**Domain层** (`src/ABC.Template.Domain/GlobalUsings.cs`):
- `global using NetCorePal.Extensions.Domain;` - 领域基础类型
- `global using NetCorePal.Extensions.Primitives;` - 强类型ID等

**Tests层** (`test/*/GlobalUsings.cs`):
- `global using Xunit;` - 测试框架
- `global using NetCorePal.Extensions.Primitives;` - 测试中的异常处理

### 常见手动using引用
当GlobalUsings未覆盖时，需要手动添加：

**查询处理器**:
```csharp
using ABC.Template.Domain.AggregatesModel.{AggregateFolder};
using ABC.Template.Infrastructure;
```

**实体配置**:
```csharp
using ABC.Template.Domain.AggregatesModel.{AggregateFolder};
```

**端点**:
```csharp
using ABC.Template.Domain.AggregatesModel.{AggregateFolder};
using ABC.Template.Web.Application.Commands.{FeatureFolder};
using ABC.Template.Web.Application.Queries.{FeatureFolder};
```
