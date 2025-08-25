````instructions
---
applyTo: "src/ABC.Template.Domain/AggregatesModel/**/*.cs"
---

# 聚合开发指南

## 概述

聚合根是 DDD 中的核心概念，代表一组相关对象的根实体，负责维护业务规则和数据一致性。在本模板中，所有聚合根都继承自 `Entity<TId>` 并实现 `IAggregateRoot` 接口。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Domain/AggregatesModel/{AggregateName}Aggregate/` 目录下
- 例如 `src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs`
- 每个聚合在独立文件夹中
- 聚合根类名与文件名一致
- 强类型ID与聚合根定义在同一文件中

## 开发规则

聚合根的定义应遵循以下规则：
- 聚合内必须有一个且只有一个聚合根
- 命名不需要带后缀Aggregate
- 必须继承 `Entity<TId>` 并实现 `IAggregateRoot` 接口
- 必须使用强类型ID，推荐使用 `IGuidStronglyTypedId`
- 必须有 protected 无参构造器供 EF Core 使用
- 状态改变时发布领域事件，使用 `this.AddDomainEvent()`
- 所有属性使用 `private set`

子实体的定义应遵循以下规则：
- 必须是 `public` 类
- 必须有一个无参构造器
- 必须有一个强类型ID，推荐使用 `IGuidStronglyTypedId`
- 必须继承自 `Entity<TId>`，并实现 `IEntity` 接口
- 聚合内允许多个子实体

## 代码示例

**文件**: `src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs`

```csharp
namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

public partial record UserId : IGuidStronglyTypedId;

public class User : Entity<UserId>, IAggregateRoot
{
    protected User() { }
    
    public User(string name, string email)
    {
        Id = UserId.New();
        Name = name;
        Email = email;
        this.AddDomainEvent(new UserCreatedDomainEvent(this));
    }

    #region Properties

    public string Name { get; private set; }
    public string Email { get; private set; }

    #endregion

    #region Methods

    public void ChangeEmail(string email)
    {
        Email = email;
        this.AddDomainEvent(new UserEmailChangedDomainEvent(this));
    }

    #endregion
}
```