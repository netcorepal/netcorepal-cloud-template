---
applyTo: "src/TestAdminProject.Web/Application/IntegrationEventHandlers/*.cs"
---

# 集成事件处理器开发指南

## 开发原则

### 必须

- **处理器定义**：
    - 必须实现 `IIntegrationEventHandler<T>` 接口。
    - 实现方法：`public Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken)`。
    - 每个集成事件可以对应多个处理器，每个处理器对应特定的业务目的。
    - 使用主构造函数注入所需的服务。
- **业务逻辑**：
    - 处理来自其他服务的集成事件。
    - 主要用于跨服务的数据同步和业务协调。
    - 通过发送 Command 来操作聚合，而不是直接操作。
- **注册**：框架自动注册事件处理器。

### 必须不要

- **直接操作**：不要直接操作聚合，应通过 Command。

## 文件命名规则

- 类文件应放置在 `src/TestAdminProject.Web/Application/IntegrationEventHandlers/` 目录下。
- 文件名格式为 `{IntegrationEvent}HandlerFor{Action}.cs`，其中 `{Action}` 需要准确描述该 Handler 的目的。
- 一个文件仅包含一个集成事件处理器。

## 代码示例

**文件**: `src/TestAdminProject.Web/Application/IntegrationEventHandlers/UserCreatedIntegrationEventHandlerForSendNotifyEmail.cs`

```csharp
using TestAdminProject.Web.Application.IntegrationEvents;
using TestAdminProject.Web.Application.Commands.Users;

namespace TestAdminProject.Web.Application.IntegrationEventHandlers;

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