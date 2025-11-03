---
applyTo: "src/ABC.Template.Domain/DomainEvents/*.cs"
---

# 领域事件开发指南
## 概述

领域事件表示在业务领域中发生的重要事情，用于实现聚合之间的协作通信。当聚合根的状态发生变化时，应该发布相应的领域事件。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在src/ABC.Template.Domain/DomainEvents目录下
- 为每个聚合添加一个领域事件文件
- 文件名格式为 `{Aggregate}DomainEvents.cs`
- 一个领域事件文件中可以包含多个领域事件

## 开发规则

领域事件的定义应遵循以下规则：
- 必须使用 `record` 类型
- 必须标记接口`IDomainEvent`，无需额外实现
- 无额外信息传递需求时，将聚合作为构造函数参数
- 使用过去式动词描述已发生的事情
- 格式：`{Entity}{Action}DomainEvent`
- 例如：`UserCreatedDomainEvent`、`OrderPaidDomainEvent`、`ProductUpdatedDomainEvent`

## 代码示例

**文件**: `src/ABC.Template.Domain/DomainEvents/UserDomainEvents.cs`

```csharp
using ABC.Template.Domain.Aggregates.UserAggregate;

namespace ABC.Template.Domain.DomainEvents;

public record UserCreatedDomainEvent(User User) : IDomainEvent;

public record UserEmailChangedDomainEvent(User User) : IDomainEvent;
```