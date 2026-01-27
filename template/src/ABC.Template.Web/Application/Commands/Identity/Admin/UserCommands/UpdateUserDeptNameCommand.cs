using FluentValidation;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 更新用户部门名称命令
/// </summary>
public record UpdateUserDeptNameCommand(UserId UserId, string DeptName) : ICommand<UserId>;

/// <summary>
/// 更新用户部门名称命令验证器
/// </summary>
public class UpdateUserDeptNameCommandValidator : AbstractValidator<UpdateUserDeptNameCommand>
{
    public UpdateUserDeptNameCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.DeptName).NotEmpty().WithMessage("部门名称不能为空");
    }
}

/// <summary>
/// 更新用户部门名称命令处理器
/// </summary>
public class UpdateUserDeptNameCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserDeptNameCommand, UserId>
{
    public async Task<UserId> Handle(UpdateUserDeptNameCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.UpdateDeptName(request.DeptName);

        return user.Id;
    }
}
