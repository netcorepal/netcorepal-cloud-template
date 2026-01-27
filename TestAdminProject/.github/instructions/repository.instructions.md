---
applyTo: "src/TestAdminProject.Infrastructure/Repositories/*.cs"
---

# 仓储开发指南

## 开发原则

### 必须

- **仓储定义**：
    - 每个聚合根对应一个仓储。
    - 接口必须继承 `IRepository<TEntity, TKey>`。
    - 实现必须继承 `RepositoryBase<TEntity, TKey, TDbContext>`。
    - 接口和实现定义在同一个文件中。
- **注册**：仓储类会被自动注册到依赖注入容器中，无需手动注册。
- **实现**：
    - 使用 `DbContext` 属性访问当前的 `DbContext` 实例。
    - 在自定义仓储方法中，使用 `DbContext.EntitySetName` 访问具体的 DbSet。

### 必须不要

- **冗余方法**：默认基类已经实现了一组常用方法，如无必要，尽量不要定义新的仓储方法。
- **重复引用**：无需重复添加 `Microsoft.EntityFrameworkCore` 引用（已在 `GlobalUsings.cs` 中定义）。

## 文件命名规则

- 仓储接口和实现应放置在 `src/TestAdminProject.Infrastructure/Repositories/` 目录下。
- 文件名格式为 `{AggregateName}Repository.cs`。

## 代码示例

**文件**: `src/TestAdminProject.Infrastructure/Repositories/AdminUserRepository.cs`

```csharp
using TestAdminProject.Domain.AggregatesModel.AdminUserAggregate;

namespace TestAdminProject.Infrastructure.Repositories;

// 接口和实现定义在同一文件中
public interface IAdminUserRepository : IRepository<AdminUser, AdminUserId>
{
    /// <summary>
    /// 根据用户名获取管理员
    /// </summary>
    Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}

public class AdminUserRepository(ApplicationDbContext context) : 
    RepositoryBase<AdminUser, AdminUserId, ApplicationDbContext>(context), IAdminUserRepository
{
    public async Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbContext.AdminUsers.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    // ...existing code...
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbContext.AdminUsers.AnyAsync(x => x.Username == username, cancellationToken);
    }
}
```

## 框架默认实现的方法

框架的 `IRepository<TEntity>` 和 `IRepository<TEntity, TKey>` 接口已实现以下方法，无需在自定义仓储中重复实现：

### 基础操作
- `IUnitOfWork UnitOfWork` - 获取工作单元对象
- `TEntity Add(TEntity entity)` - 添加实体
- `Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)` - 异步添加实体
- `void AddRange(IEnumerable<TEntity> entities)` - 批量添加实体
- `Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)` - 异步批量添加实体
- `void Attach(TEntity entity)` - 附加实体（状态设为未更改）
- `void AttachRange(IEnumerable<TEntity> entities)` - 批量附加实体
- `TEntity Update(TEntity entity)` - 更新实体
- `Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)` - 异步更新实体
- `bool Delete(Entity entity)` - 删除实体
- `Task DeleteAsync(Entity entity)` - 异步删除实体

### 主键操作（仅 IRepository<TEntity, TKey>）
- `TEntity? Get(TKey id)` - 根据主键获取实体
- `Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default)` - 异步根据主键获取实体
- `int DeleteById(TKey id)` - 根据主键删除实体
- `Task<int> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)` - 异步根据主键删除实体