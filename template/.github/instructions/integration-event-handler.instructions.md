---
applyTo: "src/ABC.Template.Web/Application/IntegrationEventHandlers/*.cs"
---

# 集成事件处理器开发指南

## 概述

集成事件处理器负责处理来自其他服务的集成事件，实现跨服务的业务协调。处理器通过 CAP 框架接收消息队列中的集成事件。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Web/Application/IntegrationEventHandlers/` 目录下
- 文件名格式为 `{IntegrationEvent}HandlerFor{Action}.cs` ,其中{Action}需要准确描述该Handler的目的
- 每个集成事件对应一个处理器文件

## 开发规则

集成事件处理器的定义应遵循以下规则：
- 必须实现 `IIntegrationEventHandler<T>` 接口
- 每个集成事件可以对应多个处理器，每个处理器对应特定的业务目的
- 处理来自其他服务的集成事件
- 主要用于跨服务的数据同步和业务协调
- 通过发送Command来操作聚合，而不是直接操作
- 使用依赖注入获取所需服务
- 框架自动注册事件处理器

## 代码示例

**文件**: `src/ABC.Template.Web/Application/IntegrationEventHandlers/UserCreatedIntegrationEventHandlerForSendNotifyEmail.cs`

```csharp
using NetCorePal.Extensions.Domain;
using ABC.Template.Web.Application.IntegrationEvents;
using ABC.Template.Web.Application.Commands;
using MediatR;

namespace ABC.Template.Web.Application.IntegrationEventHandlers;

public class UserCreatedIntegrationEventHandlerForSendNotifyEmail(
    ILogger<UserCreatedIntegrationEventHandlerForSendNotifyEmail> logger,
    IMediator mediator)
    : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public async Task HandleAsync(UserCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("发送用户创建通知邮件：{UserId}", integrationEvent.UserId);
        
        // 通过Command发送通知邮件
        var command = new SendWelcomeEmailCommand(
            integrationEvent.UserId,
            integrationEvent.Email, 
            integrationEvent.Name);
            
        await mediator.Send(command, cancellationToken);
    }
}
```