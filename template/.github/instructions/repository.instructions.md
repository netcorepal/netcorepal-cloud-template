---
applyTo: "src/ABC.Template.Infrastructure/Repositories/*.cs"
---

# 仓储开发指南

## 概述

仓储模式封装了聚合根的持久化逻辑，提供类似集合的访问接口。本模板中仓储接口定义在领域层，实现在基础设施层，遵循依赖倒置原则。

## 重要设计原则

- 每个聚合根对应一个仓储
- 仓储接口和实现应放置在 `src/ABC.Template.Infrastructure/Repositories/` 目录下
- 接口和实现定义在同一个文件中，文件名格式为 `{AggregateName}Repository.cs`
- 接口必须继承 `IRepository<TEntity, TKey>`
- 实现必须继承 `RepositoryBase<TEntity, TKey, TDbContext>`
- 仓储类会被自动注册到依赖注入容器中，无需手动注册
- 默认基类已经实现了一组常用方法，如无必要，尽量不要定义新的仓储方法

## 常见错误排查

### 依赖注入错误
**错误**: `未能找到类型或命名空间名"IDiaryRepository"`
**原因**: 在 Domain 层定义了仓储接口，或缺少引用
**解决**: 
- 将仓储接口定义在 Infrastructure 层
- 在使用仓储的地方添加 `using ABC.Template.Infrastructure.Repositories;`

### 自动注册相关
**错误**: 仓储未注册到 DI 容器
**原因**: 期望手动注册仓储
**解决**: 
- Infrastructure 层的 `AddRepositories()` 已自动注册所有仓储
- 无需在 Program.cs 中手动注册仓储

## 必要的using引用

仓储文件中的必要引用已在GlobalUsings.cs中定义：
- `global using Microsoft.EntityFrameworkCore;` - 用于EF Core扩展方法

因此在仓储文件中无需重复添加这些using语句。

## DbContext访问说明

- 通过构造函数参数访问 `ApplicationDbContext`
- 使用 `context.EntitySetName` 访问具体的DbSet
- 基类没有提供公开的 `DbSet` 或 `Context` 属性

## 代码示例

**文件**: `src/ABC.Template.Infrastructure/Repositories/UserRepository.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Infrastructure.Repositories;

// 接口和实现定义在同一文件中
public interface IUserRepository
{
    /// <summary>
    /// 根据邮箱获取用户
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，如果不存在则返回null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}
```

## 框架默认实现了下列方法，无需额外实现
```csharp
/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepository<TEntity> where TEntity : notnull, Entity, IAggregateRoot
{
    /// <summary>
    /// 获取工作单元对象
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
    /// <summary>
    /// 添加一个实体到仓储
    /// </summary>
    /// <param name="entity">要添加的实体对象</param>
    /// <returns></returns>
    TEntity Add(TEntity entity);
    /// <summary>
    /// 添加实体到仓储
    /// </summary>
    /// <param name="entity">要添加的实体对象</param>
    /// <param name="cancellationToken">取消操作token</param>
    /// <returns></returns>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// 批量添加实体到仓储
    /// </summary>
    /// <param name="entities">要添加的实体集合</param>
    /// <returns></returns>
    void AddRange(IEnumerable<TEntity> entities);
    /// <summary>
    /// 附加一个实体到仓储,并将其状态设置为未更改，如果实体没有Id，则状态会被设置为Added
    /// </summary>
    /// <param name="entity"></param>
    void Attach(TEntity entity);
    /// <summary>
    /// 附加一组实体到仓储,并将其状态设置为未更改，如果实体没有Id，则状态会被设置为Added
    /// </summary>
    /// <param name="entities"></param>
    void AttachRange(IEnumerable<TEntity> entities);
    /// <summary>
    /// 批量添加实体到仓储的异步版本
    /// </summary>
    /// <param name="entities">要添加的实体集合</param>
    /// <param name="cancellationToken">取消操作token</param>
    /// <returns></returns>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">要更新的实体对象</param>
    /// <returns></returns>
    TEntity Update(TEntity entity);
    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity">要更新的实体对象</param>
    /// <param name="cancellationToken">取消操作token</param>
    /// <returns></returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="entity">要删除的实体对象</param>
    /// <returns></returns>
    bool Remove(Entity entity);
    /// <summary>
    /// 要删除的实体对象
    /// </summary>
    /// <param name="entity">要删除的实体对象</param>
    /// <returns></returns>
    Task<bool> RemoveAsync(Entity entity);
}

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IRepository<TEntity, TKey> : IRepository<TEntity>
    where TEntity : notnull, Entity<TKey>, IAggregateRoot
    where TKey : notnull
{
    /// <summary>
    /// 根据主键删除实体
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns></returns>
    int DeleteById(TKey id);
    /// <summary>
    /// 根据主键删除实体
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="cancellationToken">取消操作token</param>
    /// <returns></returns>
    Task<int> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns></returns>
    TEntity? Get(TKey id);
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="cancellationToken">取消操作token</param>
    /// <returns></returns>
    Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);
}
```