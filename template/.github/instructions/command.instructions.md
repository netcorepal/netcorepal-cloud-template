---
applyTo: "src/ABC.Template.Web/Application/Commands/**/*.cs"
---

# 命令开发指南

## 概述

命令表示用户想要执行的操作，遵循 CQRS 模式。本模板使用 MediatR 库实现命令处理，所有命令处理器会被框架自动注册。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Web/Application/Commands/{Module}/` 目录下
- 命令文件名格式为 `{Action}{Entity}Command.cs`
- 同一个命令及其对应的验证器和处理器定义在同一文件中
- 不同的命令放在不同文件中

## 开发规则

命令的定义应遵循以下规则：
- 无返回值命令实现 `ICommand` 接口
- 有返回值命令实现 `ICommand<TResponse>` 接口
- 必须为每个命令创建验证器，继承 `AbstractValidator<TCommand>`
- 命令处理器实现对应的 `ICommandHandler` 接口
- 使用 `record` 类型定义命令
- 框架自动注册命令处理器

## 命令处理器最佳实践

### 事务管理
- **不要手动调用SaveChanges**: 框架会自动在命令处理完成后调用SaveChanges
- **不要手动调用UpdateAsync**: 如果实体是从仓储取出，则会自动跟踪变更
- **依赖UnitOfWork模式**: 让框架管理事务边界

### 仓储方法使用
- **仅用于业务操作**: 仓储方法只在需要获取聚合根进行业务操作时使用
- **优先使用异步方法**: 所有仓储操作都应使用异步版本
- **正确的取消令牌传递**: 将CancellationToken传递给所有异步操作
- **体现业务意图**: 仓储方法名应该反映业务场景，而不是通用数据访问

### 数据访问原则
- 命令处理器使用仓储获取聚合根进行业务操作
- 如果只是为了检查数据存在性，考虑是否需要完整的聚合根
- 避免在命令处理器中进行复杂的查询操作

## 必要的using引用

命令文件中的必要引用已在GlobalUsings.cs中定义：
- `global using FluentValidation;` - 用于验证器
- `global using MediatR;` - 用于命令处理器接口
- `global using NetCorePal.Extensions.Primitives;` - 用于KnownException等

命令处理器中常需手动添加的引用：
- `using ABC.Template.Domain.AggregatesModel.{Aggregate};` - 聚合根引用
- `using ABC.Template.Infrastructure.Repositories;` - 仓储接口引用

因此在命令文件中无需重复添加GlobalUsings中已定义的using语句。

## 代码示例

**文件**: `src/ABC.Template.Web/Application/Commands/CreateUserCommand.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;

namespace ABC.Template.Web.Application.Commands;

public record CreateUserCommand(string Name, string Email) : ICommand<UserId>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("用户名不能为空")
            .MaximumLength(50)
            .WithMessage("用户名不能超过50个字符");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("邮箱不能为空")
            .EmailAddress()
            .WithMessage("邮箱格式不正确")
            .MaximumLength(100)
            .WithMessage("邮箱不能超过100个字符");
    }
}

public class CreateUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (await userRepository.EmailExistsAsync(command.Email, cancellationToken))
        {
            throw new KnownException("邮箱已存在");
        }
        
        var user = new User(command.Name, command.Email);
        await userRepository.AddAsync(user, cancellationToken);
        return user.Id;
    }
}
```

## 命令处理器示例对比

### ❌ 错误的做法
```csharp
public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    var user = new User(request.Name, request.Email);
    
    userRepository.Add(user); // 应该使用异步方法
    await userRepository.UnitOfWork.SaveChangesAsync(cancellationToken); // 不应手动调用
    
    return user.Id;
}
```

### ✅ 正确的做法
```csharp
public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    var user = new User(request.Name, request.Email);
    
    await userRepository.AddAsync(user, cancellationToken); // 使用异步方法
    // 框架会自动调用SaveChanges
    
    return user.Id;
}
```
