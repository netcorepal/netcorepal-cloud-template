---
applyTo: "src/ABC.Template.Domain/AggregatesModel/**/*.cs"
---

# 强类型ID使用指南

## 概述

强类型ID提供类型安全，避免了不同实体ID之间的混淆。本模板使用NetCorePal框架的强类型ID实现，支持Int64和Guid类型。

## 文件与目录

强类型ID应该遵循以下规则：
- 与对应的聚合根定义在同一文件中
- 使用 `partial record` 定义
- 命名格式为 `{EntityName}Id`

## 开发规则

强类型ID的定义应遵循以下规则：
- 使用 `IInt64StronglyTypedId` 或 `IGuidStronglyTypedId` 接口
- 使用 `partial record` 声明，让框架生成具体实现
- 必须是public类型
- 与聚合根在同一个文件中定义

## 使用规则

在代码中使用强类型ID时应遵循：
- 在API DTO中直接使用强类型ID类型
- 避免访问 `.Value` 属性获取内部值
- 依赖框架的隐式转换进行类型转换
- 在测试中使用构造函数创建实例：`new UserId(123)`
- 在查询和比较时直接使用强类型ID

## 代码示例

**强类型ID定义**

```csharp
// 文件: src/ABC.Template.Domain/AggregatesModel/UserAggregate/User.cs
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

// 使用Int64作为内部类型
public partial record UserId : IInt64StronglyTypedId;

// 或者使用Guid作为内部类型
public partial record ProductId : IGuidStronglyTypedId;

public class User : Entity<UserId>, IAggregateRoot
{
    // 聚合根实现...
}
```

**在API中使用**

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

**在仓储中使用**

```csharp
public async Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
{
    // 直接用于查询，框架会自动处理转换
    return await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
}
```

**在测试中使用**

```csharp
[Fact]
public void Test_CreateUser()
{
    // 在测试中使用构造函数创建
    var userId = new UserId(123);
    var user = new User("test", "test@example.com");
    
    Assert.Equal(userId, user.Id);
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
