---
applyTo: "src/ABC.Template.Web/Application/IntegrationEventConverters/*.cs"
---

# 集成事件转换器开发指南

## 概述

集成事件转换器负责将领域事件转换为集成事件，用于跨服务通信。这是推荐的做法，替代了直接发布集成事件的方式，确保了事件发布的一致性和可靠性。

## 文件与目录

类文件命名应遵循以下规则：
- 转换器应放置在 `src/ABC.Template.Web/Application/IntegrationEventConverters/` 目录下
- 转换器文件名格式为 `{Entity}{Action}IntegrationEventConverter.cs`

## 开发规则

集成事件转换器的定义应遵循以下规则：
- 必须实现 `IIntegrationEventConverter<TDomainEvent, TIntegrationEvent>` 接口
- 转换器负责从领域事件创建集成事件
- 集成事件使用 `record` 类型定义
- 框架自动注册转换器

## 代码示例

**文件**: `src/ABC.Template.Web/Application/IntegrationEventConverters/UserCreatedIntegrationEventConverter.cs`

```csharp
using ABC.Template.Domain.DomainEvents;
using ABC.Template.Web.Application.IntegrationEvents;

namespace ABC.Template.Web.Application.IntegrationEventConverters;

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