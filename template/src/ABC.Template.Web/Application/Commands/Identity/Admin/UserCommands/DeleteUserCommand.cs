using FluentValidation;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 删除用户命令
/// </summary>
/// <param name="UserId">用户ID</param>
public record DeleteUserCommand(UserId UserId) : ICommand;

/// <summary>
/// 删除用户命令验证器
/// </summary>
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
    }
}

/// <summary>
/// 删除用户命令处理器
/// </summary>
public class DeleteUserCommandHandler(IUserRepository userRepository) : ICommandHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.SoftDelete();
    }
}
