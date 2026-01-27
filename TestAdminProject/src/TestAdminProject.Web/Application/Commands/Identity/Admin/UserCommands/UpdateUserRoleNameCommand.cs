using FluentValidation;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Infrastructure.Repositories;
using TestAdminProject.Domain;

namespace TestAdminProject.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 更新用户角色名称命令
/// </summary>
public record UpdateUserRoleNameCommand(UserId UserId, RoleId RoleId, string RoleName) : ICommand<UserId>;

/// <summary>
/// 更新用户角色名称命令验证器
/// </summary>
public class UpdateUserRoleNameCommandValidator : AbstractValidator<UpdateUserRoleNameCommand>
{
    public UpdateUserRoleNameCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("角色ID不能为空");
        RuleFor(x => x.RoleName).NotEmpty().WithMessage("角色名称不能为空");
    }
}

/// <summary>
/// 更新用户角色名称命令处理器
/// </summary>
public class UpdateUserRoleNameCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserRoleNameCommand, UserId>
{
    public async Task<UserId> Handle(UpdateUserRoleNameCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.UpdateRoleInfo(request.RoleId, request.RoleName);

        return user.Id;
    }
}
