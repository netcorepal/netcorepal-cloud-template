---
applyTo: "src/ABC.Template.Web/Application/Queries/**/*.cs"
---

# 查询开发指南

## 开发原则

### 必须

- **查询定义**：
    - 查询实现 `IQuery<TResponse>` 接口。
    - 必须为每个查询创建验证器，继承 `AbstractValidator<TQuery>`。
    - 查询处理器实现 `IQueryHandler<TQuery, TResponse>` 接口。
    - 使用 `record` 类型定义查询和 DTO。
    - 框架默认会过滤掉软删除的数据（`Deleted(true)` 标记的数据）。
- **数据访问**：
    - 直接使用 `ApplicationDbContext` 进行数据访问。
    - 所有数据库操作都应使用异步版本。
    - 将 `CancellationToken` 传递给所有异步操作。
- **性能优化**：
    - 使用投影 (`Select`)、过滤 (`WhereIf`)、排序 (`OrderByIf`)、分页 (`ToPagedDataAsync`) 等优化性能。
    - 分页查询使用 `PagedData<T>` 类型包装分页结果。
    - 确保默认排序，在动态排序时始终提供默认排序字段。

### 必须不要

- **仓储使用**：避免在查询中调用仓储方法（仓储仅用于命令处理器）。
- **关联查询**: 不要跨聚合使用Join关联查询，应该通过多次查询再组合的方式实现。
- **副作用**：查询不应修改任何数据状态。
- **同步操作**：避免使用同步数据库操作。

## 文件命名规则

- 类文件应放置在 `src/ABC.Template.Web/Application/Queries/{Module}s/` 目录下。
- 查询文件名格式为 `{Action}{Entity}Query.cs`。
- 查询、验证器、处理器和 DTO 定义在同一文件中。

## 代码示例

**文件**: `src/ABC.Template.Web/Application/Queries/Users/GetUserQuery.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure;
using Microsoft.EntityFrameworkCore; // 必需：用于EF Core扩展方法

namespace ABC.Template.Web.Application.Queries.Users;

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

### 分页查询示例

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
            .ToPagedDataAsync(request.PageIndex, request.PageSize, cancellationToken: cancellationToken);
    }
}
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

### ToPagedDataAsync - 分页数据

使用 `ToPagedDataAsync` 方法可以自动处理分页逻辑，返回 `PagedData<T>` 类型：

```csharp
// 基本分页用法 - 默认会查询总数
var pagedResult = await query
    .Select(u => new UserListItemDto(u.Id, u.Name, u.Email))
    .ToPagedDataAsync(pageIndex, pageSize, cancellationToken: cancellationToken);

// 性能优化版本 - 不查询总数（适用于不需要显示总页数的场景）
var pagedResult = await query
    .Select(u => new UserListItemDto(u.Id, u.Name, u.Email))
    .ToPagedDataAsync(pageIndex, pageSize, countTotal: false, cancellationToken);

// 使用 IPageRequest 接口的版本
var pagedResult = await query
    .ToPagedDataAsync(pageRequest, cancellationToken);

// PagedData<T> 包含以下属性：
// - Items: IEnumerable<T> - 当前页数据
// - Total: int - 总记录数
// - PageIndex: int - 当前页码
// - PageSize: int - 每页大小
```

**重要的using引用**：
```csharp
using Microsoft.EntityFrameworkCore;  // 用于EF Core扩展方法
// NetCorePal.Extensions.AspNetCore 已在GlobalUsings.cs中全局引用
```
