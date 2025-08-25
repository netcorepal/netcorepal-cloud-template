````instructions
---
applyTo: "src/ABC.Template.Domain/AggregatesModel/**/*.cs"
---

# 聚合与强类型ID开发指南

## 概述

聚合根是 DDD 中的核心概念，代表一组相关对象的根实体，负责维护业务规则和数据一致性。在本模板中，所有聚合根都继承自 `Entity<TId>` 并实现 `IAggregateRoot` 接口。同时，本模板使用强类型ID提供类型安全，避免了不同实体ID之间的混淆。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Domain/AggregatesModel/{AggregateName}Aggregate/` 目录下
- 例如 `src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs`
- 每个聚合在独立文件夹中
- 聚合根类名与文件名一致
- 强类型ID与聚合根定义在同一文件中

## 聚合根开发规则

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

## 强类型ID开发规则

强类型ID的定义应遵循以下规则：
- 使用 `IInt64StronglyTypedId` 或 `IGuidStronglyTypedId` 接口
- 使用 `partial record` 声明，让框架生成具体实现
- 必须是public类型
## 强类型ID生成规则

强类型ID的生成应遵循以下规则：
- **Domain层**: 聚合根构造函数中不应手动设置ID值
- **Infrastructure层**: 通过EF Core值生成器自动生成ID
- **测试环境**: 可以使用构造函数手动创建ID用于测试
- 使用 `IInt64StronglyTypedId` 或 `IGuidStronglyTypedId` 接口
- 使用 `partial record` 声明，让框架生成具体实现
- 必须是public类型
- 与聚合根在同一个文件中定义
- 命名格式为 `{EntityName}Id`

## 强类型ID使用规则

在代码中使用强类型ID时应遵循：
- 在API DTO中直接使用强类型ID类型
- 避免访问 `.Value` 属性获取内部值
- 依赖框架的隐式转换进行类型转换
- 在测试中使用构造函数创建实例：`new UserId(123)`
- 在查询和比较时直接使用强类型ID

## 代码示例

**基本聚合根定义**

文件: `src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs`

```csharp
namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

// 强类型ID定义 - 与聚合根在同一文件中
public partial record UserId : IGuidStronglyTypedId;

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

**不同类型的强类型ID示例**

```csharp
// 使用Int64作为内部类型
public partial record UserId : IInt64StronglyTypedId;

// 使用Guid作为内部类型
public partial record ProductId : IGuidStronglyTypedId;
```

**在API中使用强类型ID**

```csharp
// DTO定义 - 直接使用强类型ID
public record CreateUserResponseDto(UserId UserId);
public record GetUserRequestDto(UserId UserId);

// 端点实现
public override async Task HandleAsync(GetUserRequestDto req, CancellationToken ct)
{
    // 直接使用强类型ID，无需转换
    var user = await _userRepository.GetAsync(req.UserId, ct);
    // ...
}
```

**在仓储中使用强类型ID**

```csharp
public async Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
{
    // 直接用于查询，框架会自动处理转换
    return await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
}
```

**在测试中使用强类型ID**

```csharp
[Fact]
public void Test_CreateUser()
{
    // 在测试中使用构造函数创建
    var userId = new UserId(123);
    var user = new User("test", "test@example.com");
    
    // 注意：在Domain层测试中，ID由值生成器生成
    // 测试应该专注于业务逻辑而非ID生成
    Assert.NotNull(user);
    Assert.Equal("test", user.Name);
}
```

## 强类型ID生成示例

### ❌ 错误示例
```csharp
// 错误：在聚合根中手动生成ID
public User(string name, string email)
{
    Id = UserId.New(); // 此方法不存在
    Id = new UserId(Guid.NewGuid()); // 不应在Domain层手动生成
    Name = name;
    Email = email;
}
```

### ✅ 正确示例
```csharp
// 正确：让值生成器自动生成ID
public User(string name, string email)
{
    // 不设置ID，由EF Core值生成器生成
    Name = name;
    Email = email;
    this.AddDomainEvent(new UserCreatedDomainEvent(this));
}
```

## 常见错误

以下是使用强类型ID时应该避免的错误：

```csharp
// ❌ 错误：尝试访问Value属性
var longValue = userId.Value; // 编译错误

// ❌ 错误：手动创建嵌套构造函数
var userId = new UserId(new UserId(123)); // 不需要嵌套

// ❌ 错误：在DTO中使用原始类型
public record GetUserRequestDto(long UserId); // 应该使用UserId类型

// ✅ 正确：直接使用强类型ID
public record GetUserRequestDto(UserId UserId);

// ✅ 正确：在测试中创建
var userId = new UserId(123);

// ✅ 正确：直接比较和查询
if (user.Id == userId) { ... }
```

## 框架特性

NetCorePal强类型ID框架提供了以下特性：
- 自动的JSON序列化/反序列化
- EF Core值转换器自动配置
- 隐式类型转换支持
- 类型安全的比较操作
- 自动生成ToString()、GetHashCode()等方法