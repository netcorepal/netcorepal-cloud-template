---
applyTo: "test/**/*.cs"
---

# 单元测试开发指南

## 概述

单元测试确保领域模型的正确性和业务规则的有效性。本模板使用xUnit测试框架，重点测试聚合根的业务逻辑和领域事件。

## 文件与目录

测试文件命名应遵循以下规则：
- 领域层测试：`test/ABC.Template.Domain.Tests/{EntityName}Tests.cs`
- Web层测试：`test/ABC.Template.Web.Tests/{Feature}Tests.cs`
- 基础设施层测试：`test/ABC.Template.Infrastructure.Tests/{Component}Tests.cs`

## 开发规则

单元测试应遵循以下规则：
- 使用AAA模式：Arrange、Act、Assert
- 一个测试方法只测试一个场景
- 测试方法命名清晰表达测试意图：`{Method}_{Scenario}_{ExpectedBehavior}`
- 使用Theory和InlineData测试多个输入值
- 测试正常流程和异常流程
- 验证领域事件的发布

## 需要测试的组件类型

应为以下组件类型编写单元测试：

### 1. 聚合与实体
- **测试重点**：业务规则验证、状态变更、领域事件发布
- **测试场景**：构造函数验证、业务方法执行、边界条件处理
- **验证内容**：属性值正确性、领域事件数量和类型、异常抛出

### 2. 命令处理器
- **测试重点**：业务逻辑执行、验证器集成、仓储交互
- **测试场景**：正常流程处理、业务异常处理、验证失败场景
- **验证内容**：返回值正确性、聚合状态变更、异常类型和消息

### 3. 领域事件处理器
- **测试重点**：事件处理逻辑、副作用执行、其他服务调用
- **测试场景**：事件处理成功、处理异常、依赖服务交互
- **验证内容**：处理结果正确性、服务调用参数、状态变更

### 4. 集成事件转换器
- **测试重点**：领域事件到集成事件的转换逻辑
- **测试场景**：各种领域事件类型转换、数据映射正确性
- **验证内容**：转换后数据完整性、字段映射准确性

### 5. 集成事件处理器
- **测试重点**：跨服务业务逻辑、外部系统集成
- **测试场景**：事件处理成功、外部服务异常、重试机制
- **验证内容**：处理结果、外部调用参数、异常处理

### 6. Endpoint
- **测试重点**：API契约、请求响应映射、错误处理
- **测试场景**：正常请求处理、参数验证、异常响应
- **验证内容**：响应数据结构、HTTP状态码、错误消息格式

## 必要的using引用

测试文件需要包含以下引用：
- `global using Xunit;` - 在GlobalUsings.cs中定义
- `global using NetCorePal.Extensions.Primitives;` - 用于KnownException

## 时间相关测试

处理时间敏感的测试时：
- 避免严格的时间比较 `>`，建议使用 `>=`
- 在时间敏感操作间添加 `Thread.Sleep(1)` 确保时间差异
- 使用相对时间比较而非绝对时间比较

## 强类型ID测试

- 使用构造函数创建测试用的强类型ID实例：`new UserId(123)`
- 测试时直接比较强类型ID，无需转换
- 验证ID的相等性和不等性

## 领域事件测试

- 使用 `GetDomainEvents()` 方法获取发布的事件
- 验证事件的类型和数量
- 测试事件发布的时机和条件

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

## 测试数据准备

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

## 常见测试模式

1. **测试不变量**: 确保聚合根的业务规则始终满足
2. **测试状态转换**: 验证状态变化的正确性
3. **测试边界条件**: 测试输入的边界值
4. **测试异常场景**: 确保异常情况得到正确处理
5. **测试事件发布**: 验证领域事件在正确时机发布
