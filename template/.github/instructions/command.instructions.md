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
- 命令、验证器和处理器定义在同一文件中

## 开发规则

命令的定义应遵循以下规则：
- 无返回值命令实现 `ICommand` 接口
- 有返回值命令实现 `ICommand<TResponse>` 接口
- 必须为每个命令创建验证器，继承 `Validator<TCommand>`
- 命令处理器实现对应的 `ICommandHandler` 接口
- 使用 `record` 类型定义命令
- 框架自动注册命令处理器

## 代码示例

**文件**: `src/ABC.Template.Web/Application/Commands/CreateUserCommand.cs`

```csharp
using FluentValidation;
using NetCorePal.Extensions.Primitives;
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Web.Application.Commands;

public record CreateUserCommand(string Name, string Email) : ICommand<UserId>;

public class CreateUserCommandValidator : Validator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
    }
}

public class CreateUserCommandHandler(IUserRepository userRepository)
    : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (await userRepository.EmailExistsAsync(command.Email))
        {
            throw new KnownException("邮箱已存在");
        }
        
        var user = new User(command.Name, command.Email);
        await userRepository.AddAsync(user);
        return user.Id;
    }
}
```