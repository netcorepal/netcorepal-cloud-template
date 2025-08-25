---
applyTo: "src/ABC.Template.Web/Application/IntegrationEvents/*.cs"
---

# 集成事件开发指南

## 概述

集成事件表示跨服务边界的重要业务事件，用于实现微服务之间的异步通信。集成事件通过消息队列传递，确保服务间的松耦合。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Web/Application/IntegrationEvents/` 目录下
- 文件名格式为 `{Entity}{Action}IntegrationEvent.cs`
- 每个集成事件一个文件

## 开发规则

集成事件的定义应遵循以下规则：
- 必须使用 `record` 类型
- 不允许引用聚合
- 如果需要复杂类型作为属性，则在同文件中定义，同样必须使用 `record` 类型
- 使用过去式动词描述已发生的事情
- 包含跨服务通信所需的关键数据
- 避免包含敏感或过于详细的内部信息
- 事件应该是不可变的

## 代码示例

**文件**: `src/ABC.Template.Web/Application/IntegrationEvents/UserCreatedIntegrationEvent.cs`

```csharp
using NetCorePal.Extensions.Domain;
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Web.Application.IntegrationEvents;

public record UserCreatedIntegrationEvent(
    UserId UserId, 
    string Name, 
    string Email, 
    DateTime CreatedTime);
```
