---
applyTo: "src/ABC.Template.Domain/DomainEvents/*.cs"
---

# 领域事件开发指南

## 开发原则

### 必须

- **事件定义**：
    - 必须使用 `record` 类型。
    - 必须标记接口 `IDomainEvent`，无需额外实现。
    - 无额外信息传递需求时，将聚合作为构造函数参数。
- **命名规范**：
    - 使用过去式动词描述已发生的事情。
    - 格式：`{Entity}{Action}DomainEvent`。
    - 例如：`UserCreatedDomainEvent`、`OrderPaidDomainEvent`、`ProductUpdatedDomainEvent`。

### 必须不要

- **复杂逻辑**：领域事件本身不应包含业务逻辑，仅作为数据载体。

## 文件命名规则

- 类文件应放置在 `src/ABC.Template.Domain/DomainEvents` 目录下。
- 为每个聚合添加一个领域事件文件。
- 文件名格式为 `{Aggregate}DomainEvents.cs`。
- 一个领域事件文件中可以包含多个领域事件。

## 代码示例

**文件**: `src/ABC.Template.Domain/DomainEvents/UserDomainEvents.cs`

```csharp
using ABC.Template.Domain.Aggregates.UserAggregate;

namespace ABC.Template.Domain.DomainEvents;

public record UserCreatedDomainEvent(User User) : IDomainEvent;

public record UserEmailChangedDomainEvent(User User) : IDomainEvent;
```