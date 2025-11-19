---
applyTo: "src/ABC.Template.Web/Application/Commands/**/*.cs"
---

# 命令与命令处理器开发指南

## 开发原则

### 必须

- **命令定义**：
    - 使用 `record` 类型定义命令。
    - 无返回值命令实现 `ICommand` 接口。
    - 有返回值命令实现 `ICommand<TResponse>` 接口。
    - 必须为每个命令创建验证器，继承 `AbstractValidator<TCommand>`。
    - 命令处理器实现对应的 `ICommandHandler` 接口。
    - 命令处理器中必须通过仓储存取聚合数据。
    - 优先使用异步方法，所有仓储操作都应使用异步版本。
    - 将 `CancellationToken` 传递给所有异步操作。
    - 使用主构造函数注入所需的仓储或服务。
- **异常处理**：
    - 使用 `KnownException` 处理业务异常。
    - 业务验证失败时抛出明确的错误信息。

### 必须不要

- **事务管理**：
    - 不要手动调用 `SaveChanges`，框架会自动在命令处理完成后调用。
    - 不要手动调用 `UpdateAsync`，如果实体是从仓储取出，则会自动跟踪变更。
- **仓储使用**：
    - 避免在命令处理器中进行复杂的查询操作（应使用查询端）。
    - 仓储方法名不应是通用数据访问，而应体现业务意图。
- **重复引用**：无需重复添加 `GlobalUsings` 中已定义的 `using` 语句。

## 文件命名规则

- 类文件应放置在 `src/ABC.Template.Web/Application/Commands/{Module}s/` 目录下（以模块复数形式命名，避免命名空间与聚合类名冲突）。
- 命令文件名格式为 `{Action}{Entity}Command.cs`。
- 同一个命令及其对应的验证器和处理器定义在同一文件中。
- 不同的命令放在不同文件中。

## 代码示例

**文件**: `src/ABC.Template.Web/Application/Commands/Users/CreateUserCommand.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;

namespace ABC.Template.Web.Application.Commands.Users;

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
