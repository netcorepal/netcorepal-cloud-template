---
applyTo: "src/ABC.Template.Web/Application/Queries/**/*.cs"
---

# 查询开发指南

## 概述

查询负责数据检索，遵循CQRS模式。查询应该是无副作用的，只读取数据不修改状态。本模板使用 MediatR 库实现查询处理，所有查询处理器会被框架自动注册。

## 重要设计原则

**查询 vs 仓储的职责分离：**
- **查询处理器**：用于纯粹的数据读取，应该直接访问DbContext
- **仓储方法**：只用于命令处理器中需要获取聚合进行业务操作的场景

**查询设计指导：**
- 查询是为了展示数据，不涉及业务逻辑 → 直接使用DbContext
- 查询可以跨聚合、跨表进行复杂的数据组合
- 查询可以使用投影(Projection)和匿名类型优化性能
- 避免在查询中调用仓储方法

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Web/Application/Queries/{Module}/` 目录下
- 查询文件名格式为 `{Action}{Entity}Query.cs`
- 查询、验证器、处理器和DTO定义在同一文件中

## 开发规则

查询的定义应遵循以下规则：
- 查询实现 `IQuery<TResponse>` 接口
- 必须为每个查询创建验证器，继承 `AbstractValidator<TQuery>`
- 查询处理器实现 `IQueryHandler<TQuery, TResponse>` 接口
- 使用 `record` 类型定义查询和DTO
- 直接使用ApplicationDbContext进行数据访问
- 框架自动注册查询处理器

## 查询处理器最佳实践

### 数据访问
- **直接访问DbContext**: 查询处理器应直接注入和使用ApplicationDbContext
- **避免使用仓储**: 仓储方法仅用于命令处理器的业务操作
- **优化查询性能**: 使用投影(Select)、过滤(Where)、分页等优化性能
- **异步操作**: 所有数据库操作都应使用异步版本
- **正确的取消令牌传递**: 将CancellationToken传递给所有异步操作
- **只读操作**: 查询不应修改任何数据状态

### 跨聚合查询
- 查询可以跨多个聚合根进行数据组合
- 使用EF Core的Join、Include等方法进行关联查询
- 使用匿名类型或DTO进行数据投影

### 异常处理
- 使用 `KnownException` 处理业务异常
- 对于未找到的资源，根据业务需求决定是否抛出异常

## 必要的using引用

查询文件中的必要引用已在GlobalUsings.cs中定义：
- `global using FluentValidation;` - 用于验证器
- `global using MediatR;` - 用于查询处理器接口
- `global using NetCorePal.Extensions.Primitives;` - 用于KnownException等

因此在查询文件中无需重复添加这些using语句。

## 代码示例

**文件**: `src/ABC.Template.Web/Application/Queries/User/GetUserQuery.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure;

namespace ABC.Template.Web.Application.Queries.User;

public record GetUserQuery(UserId UserId) : IQuery<UserDto>;

public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("用户ID不能为空");
    }
}

public class GetUserQueryHandler(ApplicationDbContext context) : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Where(x => x.Id == request.UserId)
            .Select(x => new UserDto(x.Id, x.Name, x.Email))
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new KnownException($"未找到用户，UserId = {request.UserId}");

        return user;
    }
}

public record UserDto(UserId Id, string Name, string Email);
```

**分页查询示例**: `src/ABC.Template.Web/Application/Queries/User/GetUserListQuery.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure;

namespace ABC.Template.Web.Application.Queries.User;

public record GetUserListQuery(int PageIndex = 1, int PageSize = 20, string? SearchName = null) : IQuery<UserListDto>;

public class GetUserListQueryValidator : AbstractValidator<GetUserListQuery>
{
    public GetUserListQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage("页码必须大于0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("每页大小必须在1-100之间");
    }
}

public class GetUserListQueryHandler(ApplicationDbContext context) : IQueryHandler<GetUserListQuery, UserListDto>
{
    public async Task<UserListDto> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var query = context.Users.AsQueryable();
        
        // 应用搜索条件
        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            query = query.Where(x => x.Name.Contains(request.SearchName));
        }

        // 获取总数
        var totalCount = await query.CountAsync(cancellationToken);

        // 分页和投影
        var users = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto(u.Id, u.Name, u.Email))
            .ToListAsync(cancellationToken);

        return new UserListDto(
            users,
            totalCount,
            request.PageIndex,
            request.PageSize,
            (int)Math.Ceiling((double)totalCount / request.PageSize)
        );
    }
}

public record UserListDto(
    List<UserListItemDto> Items,
    int TotalCount,
    int PageIndex,
    int PageSize,
    int TotalPages
);

public record UserListItemDto(
    UserId Id,
    string Name,
    string Email
);
```

## 强类型ID处理

- 在查询和DTO中直接使用强类型ID类型，如 `UserId`、`OrderId`
- 避免使用 `.Value` 属性访问内部值
- 依赖框架的隐式转换处理类型转换

## 常见错误排查

### Entity Framework 扩展方法错误
**错误**: `IQueryable<T>"未包含"CountAsync"的定义`
**错误**: `IQueryable<T>"未包含"ToListAsync"的定义`
**错误**: `IQueryable<T>"未包含"FirstOrDefaultAsync"的定义`
**原因**: 缺少 Entity Framework Core 的 using 引用
**解决**: 在查询文件中添加 `using Microsoft.EntityFrameworkCore;`

### DbContext 访问错误
**错误**: 在查询处理器中使用仓储方法
**原因**: 职责混淆，查询应直接使用 DbContext
**解决**: 
- 查询处理器直接注入 `ApplicationDbContext`
- 使用 `context.EntitySetName` 直接访问数据
- 避免调用仓储方法

### 投影和性能优化
**建议**: 使用 `Select()` 投影避免查询不需要的字段
**建议**: 合理使用 `Where()`、`Take()`、`Skip()` 进行过滤和分页

## 框架特性

查询处理器享有以下框架特性：
- 自动依赖注入注册
- 自动验证器执行
- 自动异常处理和转换
- 自动性能监控和日志记录
