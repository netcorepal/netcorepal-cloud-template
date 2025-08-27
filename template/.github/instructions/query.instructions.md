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

## 查询处理器最佳实践

### 数据访问
- **直接访问DbContext**: 查询处理器应直接注入和使用ApplicationDbContext
- **避免使用仓储**: 仓储方法仅用于命令处理器的业务操作
- **优化查询性能**: 使用投影(Select)、过滤(WhereIf)、排序(OrderByIf)、分页(ToPagedDataAsync)等优化性能
- **异步操作**: 所有数据库操作都应使用异步版本
- **正确的取消令牌传递**: 将CancellationToken传递给所有异步操作
- **只读操作**: 查询不应修改任何数据状态

### 条件查询最佳实践
- **使用WhereIf**: 根据条件动态添加过滤条件，避免繁琐的if-else判断
- **使用OrderByIf/ThenByIf**: 根据参数动态排序，支持多字段排序
- **使用ToPagedDataAsync**: 自动处理分页逻辑，返回完整的分页信息
- **确保默认排序**: 在动态排序时始终提供默认排序字段，确保结果稳定性

### 分页查询

- 使用`PagedData<T>`类型包装分页结果
- 使用`ToPagedData<T>`扩展方法将查询结果转换为分页数据

下面展示了框架内置的分页对象和扩展方法的定义：
```
public class PagedData<T>
{
    /// <summary>
    /// 构造分页数据
    /// </summary>
    /// <param name="items">分页的数据</param>
    /// <param name="total">总数据条数</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">每页条数</param>
    public PagedData(IEnumerable<T> items, int total, int pageIndex, int pageSize)
    {
        Items = items;
        Total = total;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    /// <summary>
    /// 表示一个空的 <see cref="PagedData{T}"/> 实例。
    /// </summary>
    public static PagedData<T> Empty => new([], 0, 0, 0);

    /// <summary>
    /// 分页数据
    /// </summary>
    public IEnumerable<T> Items { get; private set; }

    /// <summary>
    /// 数据总数
    /// </summary>
    public int Total { get; private set; }

    /// <summary>
    /// 当前页码，从1开始
    /// </summary>
    public int PageIndex { get; private set; }

    /// <summary>
    /// 每页数据条数
    /// </summary>
    public int PageSize { get; private set; }
}

//扩展方法定义
public static PagedData<T> ToPagedData<T>(
        this IQueryable<T> query,
        int pageIndex = 1,
        int pageSize = 10,
        bool countTotal = false)
```

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

public record GetUserListQuery(int PageIndex = 1, int PageSize = 20, string? SearchName = null, string? SortBy = null, bool Desc = false) : IQuery<PagedData<UserListItemDto>>;

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

public class GetUserListQueryHandler(ApplicationDbContext context) : IQueryHandler<GetUserListQuery, PagedData<UserListItemDto>>
{
    public async Task<PagedData<UserListItemDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var query = context.Users.AsQueryable();
        
        // 使用 WhereIf 进行条件过滤
        query = query.WhereIf(!string.IsNullOrWhiteSpace(request.SearchName), 
                             x => x.Name.Contains(request.SearchName!));

        // 使用 OrderByIf 进行条件排序
        var orderedQuery = query
            .OrderByIf(request.SortBy == "name", x => x.Name, request.Desc)
            .ThenByIf(request.SortBy == "email", x => x.Email, request.Desc)
            .ThenByIf(string.IsNullOrEmpty(request.SortBy), x => x.Id); // 默认排序

        // 使用 ToPagedData 进行分页
        return await orderedQuery
            .Select(u => new UserListItemDto(u.Id, u.Name, u.Email))
            .ToPagedDataAsync(request.PageIndex, request.PageSize, cancellationToken);
    }
}
public record UserListItemDto(
    UserId Id,
    string Name,
    string Email
);
```

## 框架扩展方法

### WhereIf - 条件过滤

使用 `WhereIf` 方法可以根据条件动态添加 Where 子句，避免编写冗长的条件判断代码：

```csharp
// 传统写法
var query = context.Users.AsQueryable();
if (!string.IsNullOrWhiteSpace(searchName))
{
    query = query.Where(x => x.Name.Contains(searchName));
}
if (isActive.HasValue)
{
    query = query.Where(x => x.IsActive == isActive.Value);
}

// 使用 WhereIf 的简化写法
var query = context.Users
    .WhereIf(!string.IsNullOrWhiteSpace(searchName), x => x.Name.Contains(searchName!))
    .WhereIf(isActive.HasValue, x => x.IsActive == isActive!.Value);
```

### OrderByIf / ThenByIf - 条件排序

使用 `OrderByIf` 和 `ThenByIf` 方法可以根据条件动态添加排序：

```csharp
// 复杂的动态排序示例
var orderedQuery = context.Users
    .OrderByIf(sortBy == "name", x => x.Name, desc)
    .ThenByIf(sortBy == "email", x => x.Email, desc)
    .ThenByIf(sortBy == "createTime", x => x.CreateTime, desc)
    .ThenByIf(string.IsNullOrEmpty(sortBy), x => x.Id); // 默认排序

// 参数说明：
// - condition: 条件表达式，为 true 时才应用排序
// - predicate: 排序字段表达式
// - desc: 可选参数，是否降序排序，默认为 false（升序）
```

### ToPagedData - 分页数据

使用 `ToPagedDataAsync` 方法可以自动处理分页逻辑，返回 `PagedData<T>` 类型：

```csharp
// 自动处理分页
var pagedResult = await query
    .Select(u => new UserListItemDto(u.Id, u.Name, u.Email))
    .ToPagedDataAsync(pageIndex, pageSize, cancellationToken);

// PagedData<T> 包含以下属性：
// - Items: List<T> - 当前页数据
// - TotalCount: int - 总记录数
// - PageIndex: int - 当前页码
// - PageSize: int - 每页大小
// - TotalPages: int - 总页数
// - HasPreviousPage: bool - 是否有上一页
// - HasNextPage: bool - 是否有下一页
```

## 完整的分页查询示例

```csharp
public class GetProductListQueryHandler(ApplicationDbContext context) : IQueryHandler<GetProductListQuery, PagedData<ProductListItemDto>>
{
    public async Task<PagedData<ProductListItemDto>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
    {
        return await context.Products
            // 条件过滤
            .WhereIf(!string.IsNullOrWhiteSpace(request.Name), x => x.Name.Contains(request.Name!))
            .WhereIf(request.CategoryId.HasValue, x => x.CategoryId == request.CategoryId!.Value)
            .WhereIf(request.MinPrice.HasValue, x => x.Price >= request.MinPrice!.Value)
            .WhereIf(request.MaxPrice.HasValue, x => x.Price <= request.MaxPrice!.Value)
            .WhereIf(request.IsActive.HasValue, x => x.IsActive == request.IsActive!.Value)
            // 动态排序
            .OrderByIf(request.SortBy == "name", x => x.Name, request.Desc)
            .ThenByIf(request.SortBy == "price", x => x.Price, request.Desc)
            .ThenByIf(request.SortBy == "createTime", x => x.CreateTime, request.Desc)
            .ThenByIf(string.IsNullOrEmpty(request.SortBy), x => x.Id) // 默认排序确保结果稳定
            // 数据投影
            .Select(p => new ProductListItemDto(
                p.Id, 
                p.Name, 
                p.Price, 
                p.CategoryName, 
                p.IsActive,
                p.CreateTime))
            // 分页处理
            .ToPagedDataAsync(request.PageIndex, request.PageSize, cancellationToken);
    }
}
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
**建议**: 合理使用 `WhereIf()`、`OrderByIf()`、`ToPagedDataAsync()` 进行条件过滤、排序和分页
**建议**: 在使用动态排序时，始终提供默认排序字段确保结果稳定性

### 框架扩展方法错误
**错误**: `IQueryable<T>"未包含"WhereIf"的定义`
**错误**: `IQueryable<T>"未包含"OrderByIf"的定义`
**错误**: `IQueryable<T>"未包含"ToPagedDataAsync"的定义`
**原因**: 缺少 NetCorePal.Extensions.Dto 的 using 引用
**解决**: 确保项目引用了 NetCorePal.Extensions.Dto，该引用通常已在 GlobalUsings.cs 中全局定义

## 框架特性

查询处理器享有以下框架特性：
- 自动依赖注入注册
- 自动验证器执行
- 自动异常处理和转换
- 自动性能监控和日志记录
