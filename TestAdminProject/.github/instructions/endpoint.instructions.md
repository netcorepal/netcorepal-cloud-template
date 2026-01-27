---
applyTo: "src/TestAdminProject.Web/Endpoints/**/*.cs"
---

# Endpoint 开发指南

## 开发原则

### 必须

- **端点定义**：
    - 继承对应的 `Endpoint` 基类。
    - 必须为每个 Endpoint 单独定义请求 DTO 和响应 DTO。
    - 请求 DTO、响应 DTO 与端点定义在同一文件中。
    - 不同的 Endpoint 放在不同文件中。
    - 使用 `ResponseData<T>` 包装响应数据。
    - 使用主构造函数注入依赖的服务，如 `IMediator`。
- **配置与实现**：
    - 使用特性方式配置路由和权限：`[HttpPost("/api/...")]`、`[AllowAnonymous]` 等。
    - 在 `HandleAsync()` 方法中处理业务逻辑。
    - 使用构造函数注入 `IMediator` 发送命令或查询。
    - 使用 `Send.OkAsync()`、`Send.CreatedAsync()`、`Send.NoContentAsync()` 发送响应。
    - 使用 `.AsResponseData()` 扩展方法创建响应数据。
- **强类型ID处理**：
    - 在 DTO 中直接使用强类型 ID 类型，如 `UserId`、`OrderId`。
    - 依赖框架的隐式转换处理类型转换。
- **引用**：
    - 引用 `FastEndpoints` 和 `Microsoft.AspNetCore.Authorization`。

### 必须不要

- **配置方式**：使用属性特性而不是 `Configure()` 方法来配置端点。
- **强类型ID**：避免使用 `.Value` 属性访问内部值。

## 文件命名规则

- 类文件应放置在 `src/TestAdminProject.Web/Endpoints/{Module}/` 目录下。
- 端点文件名格式为 `{Action}{Entity}Endpoint.cs`。
- 请求 DTO、响应 DTO 与端点定义在同一文件中。

## 代码示例

**文件**: `src/TestAdminProject.Web/Endpoints/User/CreateUserEndpoint.cs`

```csharp
using FastEndpoints;
using TestAdminProject.Web.Application.Commands;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Authorization;

namespace TestAdminProject.Web.Endpoints.User;

public record CreateUserRequestDto(string Name, string Email);

public record CreateUserResponseDto(UserId UserId);

[Tags("Users")]
[HttpPost("/api/users")]
[AllowAnonymous]
public class CreateUserEndpoint(IMediator mediator) : Endpoint<CreateUserRequestDto, ResponseData<CreateUserResponseDto>>
{
    public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
    {
        var command = new CreateUserCommand(req.Name, req.Email);
        var userId = await mediator.Send(command, ct);

        await Send.OkAsync(new CreateUserResponseDto(userId).AsResponseData(), cancellation: ct);
    }
}
```

### 更多端点响应示例

#### 创建资源的端点
```csharp
public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
{
    var command = new CreateUserCommand(req.Name, req.Email);
    var userId = await mediator.Send(command, ct);
    
    // ...existing code...
    var response = new CreateUserResponseDto(userId);
    await Send.CreatedAsync(response.AsResponseData(), ct);
}
```

#### 查询资源的端点  
```csharp
public override async Task HandleAsync(GetUserRequestDto req, CancellationToken ct)
{
    var query = new GetUserQuery(req.UserId);
    var user = await mediator.Send(query, ct);
    
    await Send.OkAsync(user.AsResponseData(), ct);
}
```

#### 更新资源的端点
```csharp
public override async Task HandleAsync(UpdateUserRequestDto req, CancellationToken ct)
{
    var command = new UpdateUserCommand(req.UserId, req.Name, req.Email);
    await mediator.Send(command, ct);
    
    await Send.NoContentAsync(ct);
}
```

## 配置方式

端点使用属性模式配置，不使用 `Configure()` 方法：

```csharp
[Tags("ModuleName")]
[HttpPost("/api/resource")]
[AllowAnonymous]
public class CreateResourceEndpoint(IMediator mediator) : Endpoint<CreateRequest, ResponseData<CreateResponse>>
{
    // 实现
}
```
