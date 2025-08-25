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
- 使用 MediatR 发送命令或查询

## 代码示例

**文件**: `src/ABC.Template.Web/Endpoints/User/CreateUserEndpoint.cs`

```csharp
using FastEndpoints;
using NetCorePal.Extensions.Dto;
using ABC.Template.Web.Application.Commands;
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Web.Endpoints.User;

public record CreateUserRequestDto(string Name, string Email);

public record CreateUserResponseDto(UserId UserId);

public class CreateUserEndpoint : Endpoint<CreateUserRequestDto, ResponseData<CreateUserResponseDto>>
{
    public override void Configure()
    {
        Post("/api/users");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
    {
        var command = new CreateUserCommand(req.Name, req.Email);
        var userId = await new GetRequiredService<IMediator>().SendAsync(command);
        var response = new CreateUserResponseDto(userId);
        await SendOkAsync(response.AsResponseData(), ct);
    }
}
```
