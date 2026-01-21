using FluentValidation;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 密码重置命令
/// </summary>
public record PasswordResetCommand(UserId UserId, string Password) : ICommand<UserId>;

/// <summary>
/// 密码重置命令验证器
/// </summary>
public class PasswordResetCommandValidator : AbstractValidator<PasswordResetCommand>
{
    public PasswordResetCommandValidator(UserQuery userQuery)
    {
        RuleFor(u => u.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
    }
}

/// <summary>
/// 密码重置命令处理器
/// </summary>
public class PasswordResetCommandHandler(IUserRepository userRepository) : ICommandHandler<PasswordResetCommand, UserId>
{
    public async Task<UserId> Handle(PasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) 
                   ?? throw new KnownException($"用户不存在，UserId={request.UserId}", ErrorCodes.UserNotFound);
        user.PasswordReset(request.Password);
        return user.Id;
    }
}

