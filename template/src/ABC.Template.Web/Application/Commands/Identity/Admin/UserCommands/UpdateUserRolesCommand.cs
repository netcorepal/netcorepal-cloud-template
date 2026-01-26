using FluentValidation;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 更新用户角色命令
/// </summary>
public record UpdateUserRolesCommand(UserId UserId, List<AssignAdminUserRoleQueryDto> RolesToBeAssigned) : ICommand;

/// <summary>
/// 更新用户角色命令验证器
/// </summary>
public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
    }
}

/// <summary>
/// 更新用户角色命令处理器
/// </summary>
public class UpdateUserRolesCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserRolesCommand>
{
    public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken)
                   ?? throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        List<UserRole> roles = [];

        foreach (var assignAdminUserRoleDto in request.RolesToBeAssigned)
        {
            roles.Add(new UserRole(assignAdminUserRoleDto.RoleId, assignAdminUserRoleDto.RoleName));
        }

        user.UpdateRoles(roles);
    }
}
