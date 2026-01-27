---
applyTo: "src/TestAdminProject.Web/Application/IntegrationEventConverters/*.cs"
---

# 集成事件转换器开发指南

## 开发原则

### 必须

- **转换器定义**：
    - 必须实现 `IIntegrationEventConverter<TDomainEvent, TIntegrationEvent>` 接口。
    - 转换器负责从领域事件创建集成事件。
    - 集成事件使用 `record` 类型定义。
- **注册**：框架自动注册转换器。

### 必须不要

- **直接发布**：不要直接发布集成事件，应通过转换器。

## 文件命名规则

- 转换器应放置在 `src/TestAdminProject.Web/Application/IntegrationEventConverters/` 目录下。
- 转换器文件名格式为 `{Entity}{Action}IntegrationEventConverter.cs`。

## 代码示例

**文件**: `src/TestAdminProject.Web/Application/IntegrationEventConverters/UserCreatedIntegrationEventConverter.cs`

```csharp
using TestAdminProject.Domain.DomainEvents;
using TestAdminProject.Web.Application.IntegrationEvents;

namespace TestAdminProject.Web.Application.IntegrationEventConverters;

public class UserCreatedIntegrationEventConverter 
    : IIntegrationEventConverter<UserCreatedDomainEvent, UserCreatedIntegrationEvent>
{
    public UserCreatedIntegrationEvent Convert(UserCreatedDomainEvent domainEvent)
    {
        var user = domainEvent.User;
        return new UserCreatedIntegrationEvent(
            user.Id, 
            user.Name, 
            user.Email, 
            DateTime.UtcNow);
    }
}
```