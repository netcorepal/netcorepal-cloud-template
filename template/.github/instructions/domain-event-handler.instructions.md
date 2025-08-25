---
applyTo: "src/ABC.Template.Web/Application/DomainEventHandlers/*.cs"
---

# 领域事件处理器开发指南

## 概述

领域事件处理器负责处理聚合根发布的领域事件，实现跨聚合的业务协调和副作用处理。处理器在事务边界内执行，确保数据一致性。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Web/Application/DomainEventHandlers/` 目录下
- 文件名格式为 `{DomainEvent}HandlerFor{Action}.cs` ,其中{Action}需要准确描述该Handler的目的
- 每个领域事件对应一个处理器文件

## 开发规则

领域事件处理器的定义应遵循以下规则：
- 必须实现 `IDomainEventHandler<T>` 接口
- 实现 `Handle(TEvent domainEvent, CancellationToken cancellationToken)` 方法
- 每个领域事件可以对应多个处理器，每个处理器对应特定的业务目的
- 在事务边界内执行，确保数据一致性
- 主要用于跨聚合协调和副作用处理
- 通过发送Command来操作聚合，而不是直接操作
- 使用依赖注入获取所需服务
- 框架自动注册事件处理器

## 必要的using引用

领域事件处理器文件中的必要引用已在GlobalUsings.cs中定义：
- `global using NetCorePal.Extensions.Domain;` - 用于IDomainEventHandler接口
- `global using MediatR;` - 用于发送其他命令

因此在领域事件处理器文件中无需重复添加这些using语句。

## 代码示例

**文件**: `src/ABC.Template.Web/Application/DomainEventHandlers/UserCreatedDomainEventHandlerForCreateScoreAccount.cs`

```csharp
using ABC.Template.Domain.DomainEvents;
using ABC.Template.Web.Application.Commands;

namespace ABC.Template.Web.Application.DomainEventHandlers;

public class UserCreatedDomainEventHandlerForCreateScoreAccount(
    ILogger<UserCreatedDomainEventHandlerForCreateScoreAccount> logger,
    IMediator mediator)
    : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var user = domainEvent.User;
        logger.LogInformation("为新用户创建积分账户：{UserId}", user.Id);
        
        // 通过发送Command来创建积分账户
        var command = new CreateScoreAccountCommand(user.Id, user.Name);
        await mediator.Send(command, cancellationToken);
    }
}
```