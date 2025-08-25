---
applyTo: "src/ABC.Template.Web/Endpoints/**/*.cs"
---

# Endpoint 开发指南

## 概述

FastEndpoints 是推荐的 API 端点实现方式，提供了比传统 MVC Controller 更好的性能和开发体验。每个端点都应该有独立的文件。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Web/Endpoints/{Module}/` 目录下
- 端点文件名格式为 `{Action}{Entity}Endpoint.cs`
- 请求DTO、响应DTO与端点定义在同一文件中

## 开发规则

端点的定义应遵循以下规则：
- 继承对应的 `Endpoint` 基类
- 必须为每个Endpoint单独定义请求DTO和响应DTO
- 请求DTO、响应DTO与端点定义在同一文件中
- 使用 `ResponseData<T>` 包装响应数据
- 在 `Configure()` 方法中配置路由和权限
- 在 `HandleAsync()` 方法中处理业务逻辑
- 使用构造函数注入 `IMediator` 发送命令或查询
- 使用 `Send.OkAsync()` 发送成功响应
- 使用 `.AsResponseData()` 扩展方法创建响应数据

## FastEndpoints响应方法

### 常用响应方法
- `Send.OkAsync()` - 发送200 OK响应
- `Send.CreatedAsync()` - 发送201 Created响应  
- `Send.NoContentAsync()` - 发送204 No Content响应

### 响应数据包装
- 使用`ResponseData<T>`包装响应数据
- 使用`.AsResponseData()`扩展方法创建包装

## 必要的using引用

端点文件中的必要引用：
- `using FastEndpoints;` - 用于端点基类

## 强类型ID处理

- 在DTO中直接使用强类型ID类型，如 `UserId`、`OrderId`
- 避免使用 `.Value` 属性访问内部值
- 依赖框架的隐式转换处理类型转换

## 代码示例

**文件**: `src/ABC.Template.Web/Endpoints/User/CreateUserEndpoint.cs`

```csharp
using FastEndpoints;
using ABC.Template.Web.Application.Commands;
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Web.Endpoints.User;

public record CreateUserRequestDto(string Name, string Email);

public record CreateUserResponseDto(UserId UserId);

public class CreateUserEndpoint : Endpoint<CreateUserRequestDto, ResponseData<CreateUserResponseDto>>
{
    private readonly IMediator _mediator;

    public CreateUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "创建用户";
            s.Description = "创建一个新用户";
        });
    }
    
    public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
    {
        var command = new CreateUserCommand(req.Name, req.Email);
        var userId = await _mediator.Send(command, ct);
        
        await Send.OkAsync(new CreateUserResponseDto(userId).AsResponseData(), ct);
    }
}
```

## 端点响应示例

### ✅ 创建资源的端点
```csharp
public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
{
    var command = new CreateUserCommand(req.Name, req.Email);
    var userId = await mediator.Send(command, ct);
    
    var response = new CreateUserResponseDto(userId);
    await Send.CreatedAsync(response.AsResponseData(), ct);
}
```

### ✅ 查询资源的端点  
```csharp
public override async Task HandleAsync(GetUserRequestDto req, CancellationToken ct)
{
    var query = new GetUserQuery(req.UserId);
    var user = await mediator.Send(query, ct);
    
    await Send.OkAsync(user.AsResponseData(), ct);
}
```

### ✅ 更新资源的端点
```csharp
public override async Task HandleAsync(UpdateUserRequestDto req, CancellationToken ct)
{
    var command = new UpdateUserCommand(req.UserId, req.Name, req.Email);
    await mediator.Send(command, ct);
    
    await Send.NoContentAsync(ct);
}
```

## 常见错误排查

### FastEndpoints 基类选择错误
**错误**: `非泛型 类型"Endpoint"不能与类型参数一起使用`
**原因**: 缺少 `using FastEndpoints;` 引用
**解决**: 添加必要的 using 引用

### 响应方法不存在错误
**错误**: `当前上下文中不存在名称"Send"`
**原因**: 基类选择错误或缺少 using 引用
**解决**: 
- 检查基类是否正确
- 有请求有响应：`Endpoint<TRequest, TResponse>`
- 有请求无响应：`Endpoint<TRequest, EmptyResponse>`
- 无请求有响应：`EndpointWithoutRequest<TResponse>`

### 属性配置错误
**错误**: `未能找到类型或命名空间名"HttpPostAttribute"`
**原因**: 缺少 `using Microsoft.AspNetCore.Authorization;` 引用
**解决**: 添加 `using Microsoft.AspNetCore.Authorization;`

## 配置方式

端点使用属性模式配置，不使用 `Configure()` 方法：

```csharp
[Tags("ModuleName")]
[HttpPost("/api/resource")]
[AllowAnonymous]
public class CreateResourceEndpoint : Endpoint<CreateRequest, ResponseData<CreateResponse>>
{
    // 实现
}
```
