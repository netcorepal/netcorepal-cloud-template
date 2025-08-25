---
applyTo: "src/ABC.Template.Infrastructure/Repositories/*.cs"
---

# 仓储开发指南

## 概述

仓储模式封装了聚合根的持久化逻辑，提供类似集合的访问接口。本模板中仓储接口定义在领域层，实现在基础设施层，遵循依赖倒置原则。

## 文件与目录

类文件命名应遵循以下规则：
- 仓储接口和实现应放置在 `src/ABC.Template.Infrastructure/Repositories/` 目录下
- 文件名格式为 `{EntityName}Repository.cs`
- 接口和实现定义在同一个文件中

## 开发规则

仓储的定义应遵循以下规则：
- 接口必须继承 `IRepository<TEntity, TKey>`
- 实现必须继承 `RepositoryBase<TEntity, TKey, TDbContext>`
- 接口和实现定义在同一文件中
- 方法名反映业务意图，使用异步方法
- 每个聚合根对应一个仓储
- 框架自动注册仓储实现

## 代码示例

**文件**: `src/ABC.Template.Infrastructure/Repositories/UserRepository.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User, UserId>
{
    #region 添加额外需要的方法
    Task<User?> GetByEmailAsync(string email);
    #endregion
}

public class UserRepository(ApplicationDbContext dbContext) : Repository<User, UserId, ApplicationDbContext>, IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
    }
}
```


## 框架功能

框架默认实现了下列方法，无需额外实现
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