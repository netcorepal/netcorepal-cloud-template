using FluentValidation;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Infrastructure;
using TestAdminProject.Infrastructure.Repositories;
using TestAdminProject.Web.Application.Queries;
using TestAdminProject.Domain;

namespace TestAdminProject.Web.Application.Commands.Identity.Admin.RoleCommands;

/// <summary>
/// 更新角色信息命令
/// </summary>
/// <param name="RoleId">角色ID</param>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
/// <param name="PermissionCodes">权限代码列表</param>
public record UpdateRoleInfoCommand(RoleId RoleId, string Name, string Description, IEnumerable<string> PermissionCodes) : ICommand;

/// <summary>
/// 更新角色信息命令验证器
/// </summary>
public class UpdateRoleInfoCommandValidator : AbstractValidator<UpdateRoleInfoCommand>
{
    public UpdateRoleInfoCommandValidator(RoleQuery roleQuery)
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

/// <summary>
/// 更新角色信息命令处理器
/// </summary>
public class UpdateRoleInfoCommandHandler(IRoleRepository roleRepository) : ICommandHandler<UpdateRoleInfoCommand>
{
    public async Task Handle(UpdateRoleInfoCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(request.RoleId, cancellationToken) ??
                   throw new KnownException($"未找到角色，RoleId = {request.RoleId}", ErrorCodes.RoleNotFound);
        role.UpdateRoleInfo(request.Name, request.Description);

        var permissions = request.PermissionCodes.Select(perm => new RolePermission(perm));
        role.UpdateRolePermissions(permissions);
    }
}

