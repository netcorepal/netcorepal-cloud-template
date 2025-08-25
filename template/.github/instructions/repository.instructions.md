---
applyTo: "src/ABC.Template.Infrastructure/Repositories/*.cs"
---

# 仓储开发指南

## 概述

仓储模式封装了聚合根的持久化逻辑，提供类似集合的访问接口。本模板中仓储接口定义在领域层，实现在基础设施层，遵循依赖倒置原则。

## 重要设计原则

**仓储接口定义位置：**
- 仓储接口和实现都应该定义在 Infrastructure 层
- 不要在 Domain 层定义仓储接口
- 使用 `AddRepositories()` 自动注册，无需手动注册

**仓储 vs 查询的职责分离：**
- **仓储方法**：只用于命令处理器中需要获取聚合进行业务操作的场景
- **查询(Query)**：用于纯粹的数据读取，应该直接使用Query模式访问DbContext

**设计决策指导：**
- 如果需要获取聚合根来执行业务逻辑 → 使用仓储方法
- 如果只是为了展示数据或统计信息 → 使用Query直接访问DbContext
- 仓储方法应该体现业务意图，而不是通用的数据访问

## 文件与目录

类文件命名应遵循以下规则：
- 仓储接口和实现应放置在 `src/ABC.Template.Infrastructure/Repositories/` 目录下
- 文件名格式为 `{EntityName}Repository.cs`
- 接口和实现定义在同一个文件中

## 开发规则

仓储的定义应遵循以下规则：
- 接口和实现都定义在 Infrastructure 层的同一文件中
- 接口必须继承 `IRepository<TEntity, TKey>`（如果需要继承基接口）
- 实现必须继承 `RepositoryBase<TEntity, TKey, TDbContext>`（如果需要继承基类）
- 或者直接定义接口，用构造函数注入 `ApplicationDbContext`
- 方法名反映业务意图，使用异步方法
- 每个聚合根对应一个仓储
- 框架自动注册仓储实现

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
    
    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="user">用户实体</param>
    Task AddAsync(User user);
}

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
    
    public async Task AddAsync(User user)
    {
        await context.Users.AddAsync(user);
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