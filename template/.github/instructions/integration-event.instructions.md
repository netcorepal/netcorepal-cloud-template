---
applyTo: "src/ABC.Template.Web/Application/IntegrationEvents/*.cs"
---

# 集成事件开发指南

## 开发原则

### 必须

- **事件定义**：
    - 必须使用 `record` 类型。
    - 包含跨服务通信所需的关键数据。
    - 使用过去式动词描述已发生的事情。
    - 事件应该是不可变的。
- **复杂类型**：如果需要复杂类型作为属性，则在同文件中定义，同样必须使用 `record` 类型。

### 必须不要

- **引用聚合**：不允许引用聚合。
- **敏感信息**：避免包含敏感或过于详细的内部信息。

## 文件命名规则

- 类文件应放置在 `src/ABC.Template.Web/Application/IntegrationEvents/` 目录下。
- 文件名格式为 `{Entity}{Action}IntegrationEvent.cs`。
- 每个集成事件一个文件。

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
