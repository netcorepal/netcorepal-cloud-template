---
applyTo: "test/**/*.cs"
---

# 单元测试开发指南

## 开发原则

### 必须

- **测试模式**：使用 AAA 模式：Arrange、Act、Assert。
- **测试范围**：
    - 一个测试方法只测试一个场景。
    - 测试正常流程和异常流程。
    - 验证领域事件的发布。
    - 确保聚合根的业务规则始终满足（不变量）。
    - 验证状态变化的正确性。
    - 测试输入的边界值。
- **命名规范**：测试方法命名清晰表达测试意图：`{Method}_{Scenario}_{ExpectedBehavior}`。
- **数据驱动**：使用 `Theory` 和 `InlineData` 测试多个输入值。
- **强类型ID**：
    - 使用构造函数创建测试用的强类型 ID 实例：`new UserId(123)`。
    - 测试时直接比较强类型 ID，无需转换。
- **时间处理**：
    - 避免严格的时间比较 `>`，建议使用 `>=`。
    - 使用相对时间比较而非绝对时间比较。
- **领域事件**：
    - 使用 `GetDomainEvents()` 方法获取发布的事件。
    - 验证事件的类型和数量。

### 必须不要

- **时间比较**：不要使用严格的时间比较（如 `==` 或 `>`），因为执行时间会有微小差异。

## 文件命名规则

- 领域层测试：`test/ABC.Template.Domain.Tests/{EntityName}Tests.cs`。
- Web 层测试：`test/ABC.Template.Web.Tests/{Feature}Tests.cs`。
- 基础设施层测试：`test/ABC.Template.Infrastructure.Tests/{Component}Tests.cs`。

## 代码示例

**基本聚合根测试**

```csharp
public class UserTests
{
    [Fact]
    public void User_Constructor_ShouldCreateValidUser()
    {
        // Arrange
        var name = "张三";
        var email = "zhangsan@example.com";

        // Act
        var user = new User(name, email);

        // Assert
        Assert.Equal(name, user.Name);
        Assert.Equal(email, user.Email);
        Assert.False(user.IsDeleted);
        Assert.True(user.CreateTime <= DateTimeOffset.UtcNow);
        
        // 验证领域事件
        var domainEvents = user.GetDomainEvents();
        Assert.Single(domainEvents);
        Assert.IsType<UserCreatedDomainEvent>(domainEvents.First());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void User_Constructor_WithInvalidName_ShouldThrowException(string? name)
    {
        // Arrange
        var email = "test@example.com";

        // Act & Assert
        Assert.Throws<KnownException>(() => new User(name!, email));
    }
}
```

**时间相关测试**

```csharp
[Fact]
public void UpdateEmail_ShouldUpdateTimestamp()
{
    // Arrange
    var user = new User("张三", "old@example.com");
    var originalUpdateTime = user.UpdateTime;
    
    // 确保时间差异
    Thread.Sleep(1);

    // Act
    user.UpdateEmail("new@example.com");

    // Assert
    Assert.Equal("new@example.com", user.Email);
    Assert.True(user.UpdateTime >= originalUpdateTime);
    
    // 验证领域事件
    var domainEvents = user.GetDomainEvents();
    Assert.Equal(2, domainEvents.Count); // 创建事件 + 更新事件
    Assert.IsType<UserEmailUpdatedDomainEvent>(domainEvents.Last());
}
```

**强类型ID测试**

```csharp
[Fact]
public void UserId_ShouldWorkCorrectly()
{
    // Arrange & Act
    var userId1 = new UserId(123);
    var userId2 = new UserId(123);
    var userId3 = new UserId(456);

    // Assert
    Assert.Equal(userId1, userId2);
    Assert.NotEqual(userId1, userId3);
    Assert.Equal("123", userId1.ToString());
}
```

**业务规则测试**

```csharp
[Fact]
public void ChangeEmail_OnDeletedUser_ShouldThrowException()
{
    // Arrange
    var user = new User("张三", "test@example.com");
    user.Delete();

    // Act & Assert
    var exception = Assert.Throws<KnownException>(() => 
        user.UpdateEmail("new@example.com"));
    Assert.Equal("无法修改已删除用户的邮箱", exception.Message);
}

[Fact]
public void Delete_AlreadyDeletedUser_ShouldThrowException()
{
    // Arrange
    var user = new User("张三", "test@example.com");
    user.Delete();

    // Act & Assert
    Assert.Throws<KnownException>(() => user.Delete());
}
```

**领域事件详细测试**

```csharp
[Fact]
public void User_BusinessOperations_ShouldPublishCorrectEvents()
{
    // Arrange
    var user = new User("张三", "old@example.com");
    user.ClearDomainEvents(); // 清除构造函数事件
    
    // Act
    user.UpdateEmail("new@example.com");
    user.UpdateName("李四");

    // Assert
    var events = user.GetDomainEvents();
    Assert.Equal(2, events.Count);
    
    var emailEvent = events.OfType<UserEmailUpdatedDomainEvent>().Single();
    Assert.Equal(user, emailEvent.User);
    
    var nameEvent = events.OfType<UserNameUpdatedDomainEvent>().Single();
    Assert.Equal(user, nameEvent.User);
}
```

### 测试数据工厂示例

使用Builder模式或Factory方法创建测试数据：

```csharp
public static class TestDataFactory
{
    public static User CreateValidUser(string name = "测试用户", string email = "test@example.com")
    {
        return new User(name, email);
    }
    
    public static UserId CreateUserId(long value = 123)
    {
        return new UserId(value);
    }
}

// 在测试中使用
var user = TestDataFactory.CreateValidUser();
var userId = TestDataFactory.CreateUserId(456);
```
