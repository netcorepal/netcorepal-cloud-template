---
applyTo: "src/ABC.Template.Domain/AggregatesModel/**/*.cs"
---

# 聚合与强类型ID开发指南

## 开发原则

### 必须

- **聚合根定义**：
    - 聚合内必须有一个且只有一个聚合根。
    - 必须继承 `Entity<TId>` 并实现 `IAggregateRoot` 接口。
    - 必须有 `protected` 无参构造器供 EF Core 使用。
    - 所有属性使用 `private set`，并显式设置默认值。
    - 状态改变时发布领域事件，使用 `this.AddDomainEvent()`。
    - `Deleted` 属性表示软删除状态。
    - `RowVersion` 属性用于乐观并发控制。
- **强类型ID定义**：
    - 必须使用 `IInt64StronglyTypedId` 或 `IGuidStronglyTypedId`，优先使用 `IGuidStronglyTypedId`。
    - 必须使用 `partial record` 声明，让框架生成具体实现。
    - 必须是 `public` 类型。
    - 必须与聚合/实体在同一个文件中定义。
    - 命名格式必须为 `{EntityName}Id`。
- **子实体定义**：
    - 必须是 `public` 类。
    - 必须有一个无参构造器。
    - 必须有一个强类型ID，推荐使用 `IGuidStronglyTypedId`。
    - 必须继承自 `Entity<TId>`，并实现 `IEntity` 接口。
    - 聚合内允许多个子实体。

### 必须不要

- **依赖关系**: 聚合之间不相互依赖，避免直接引用其他聚合根，聚合之间也不共享子实体，使用领域事件或领域服务进行交互。
- **命名**：聚合根类名不需要带后缀 `Aggregate`。
- **ID设置**：无需手动设置ID的值，由 EF Core 值生成器自动生成。

## 文件命名规则

- 类文件应放置在 `src/ABC.Template.Domain/AggregatesModel/{AggregateName}Aggregate/` 目录下。
- 例如 `src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs`。
- 每个聚合在独立文件夹中。
- 聚合根类名与文件名一致。
- 强类型ID与聚合根定义在同一文件中。

## 代码示例

文件: `src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs`

```csharp
using ABC.Template.Domain.DomainEvents; // 必需：引用领域事件

namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

// 强类型ID定义 - 与聚合根在同一文件中
public partial record UserId : IGuidStronglyTypedId;

// 聚合根定义
public class User : Entity<UserId>, IAggregateRoot
{
    protected User() { }
    
    public User(string name, string email)
    {
        // 不手动设置ID，由EF Core值生成器自动生成
        Name = name;
        Email = email;
        this.AddDomainEvent(new UserCreatedDomainEvent(this));
    }

    #region Properties

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Deleted Deleted { get; private set; } = new(); //默认false
    public RowVersion RowVersion { get; private set; } = new RowVersion(0);

    #endregion

    #region Methods

    // ...existing code...
    public void ChangeEmail(string email)
    {
        Email = email;
        this.AddDomainEvent(new UserEmailChangedDomainEvent(this));
    }

    #endregion
}
```