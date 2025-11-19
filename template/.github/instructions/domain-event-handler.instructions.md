---
applyTo: "src/ABC.Template.Web/Application/DomainEventHandlers/*.cs"
---

# 领域事件处理器开发指南

## 开发原则

### 必须

- **处理器定义**：
    - 必须实现 `IDomainEventHandler<T>` 接口。
    - 实现方法：`public Task Handle(TEvent domainEvent, CancellationToken cancellationToken)`。
    - 每个文件仅包含一个事件处理器。
    - 使用主构造函数注入所需的仓储或服务。
    - **命名规范**：按 `{DomainEvent}DomainEventHandlerFor{Action}` 命名，语义清晰、单一目的。
- **业务逻辑**：
    - 在框架会将处理器中的命令作为事务的一部分执行。
    - 通过发送 Command（`IMediator.Send`）驱动聚合变化，而不是直接操作聚合或数据库。
    - 仅访问与该领域事件直接相关的数据或服务。
    - 尊重事务与取消：使用 `async/await`，传递并尊重 `CancellationToken`。

### 必须不要

- **直接操作**：
    - 不直接通过仓储或 DbContext 修改数据（始终通过命令）。
- **逻辑混合**：
    - 不在一个文件中放多个处理器或混合不同事件的逻辑。
- **性能与异常**：
    - 不执行长时间阻塞操作，耗时操作应放置在集成事件处理器中。
    - 不吞掉异常或忽略 `CancellationToken`。

## 文件命名规则

- 类文件应放置在 `src/ABC.Template.Web/Application/DomainEventHandlers/` 目录下。
- 文件名格式为 `{DomainEvent}DomainEventHandlerFor{Action}.cs`，其中 `{Action}` 准确描述该 Handler 的目的。
- 示例：`OrderCreatedDomainEventHandlerForSetPaymentInfo.cs`。
- 类命名：`{DomainEvent}DomainEventHandlerFor{Action}`。

## 代码示例
```csharp
using ABC.Template.Web.Application.Commands;
using ABC.Template.Web.Domain.DomainEvents;
public class OrderCreatedDomainEventHandlerForSetPaymentInfo(IMediator mediator) :   
   IDomainEventHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 通过发送命令操作聚合，而不是直接操作服务
        var command = new SetPaymentInfoCommand(domainEvent.OrderId, domainEvent.PaymentInfo);
        await mediator.Send(command, cancellationToken);
    }
}
```