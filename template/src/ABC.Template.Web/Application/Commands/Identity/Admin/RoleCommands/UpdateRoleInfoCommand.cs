using FluentValidation;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Domain;
using ABC.Template.Web.AppPermissions;
//#if (UseMongoDB)
using Microsoft.EntityFrameworkCore;
//#endif

namespace ABC.Template.Web.Application.Commands.Identity.Admin.RoleCommands;

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
//#if (UseMongoDB)
/// MongoDB 不支持通过导航属性保存跨集合实体，需要直接操作 RolePermissions 集合
public class UpdateRoleInfoCommandHandler(IRoleRepository roleRepository, ApplicationDbContext dbContext) : ICommandHandler<UpdateRoleInfoCommand>
{
    public async Task Handle(UpdateRoleInfoCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(request.RoleId, cancellationToken) ??
                   throw new KnownException($"未找到角色，RoleId = {request.RoleId}", ErrorCodes.RoleNotFound);
        role.UpdateRoleInfo(request.Name, request.Description);

        var permissionCodes = request.PermissionCodes.ToList();
        var existingPermissions = await dbContext.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        var existingPermissionMap = existingPermissions.ToDictionary(p => p.PermissionCode);
        var targetPermissionCodes = permissionCodes.ToHashSet();

        foreach (var permissionToRemove in existingPermissions.Where(ep => !targetPermissionCodes.Contains(ep.PermissionCode)).ToList())
        {
            dbContext.RolePermissions.Remove(permissionToRemove);
        }

        var existingPermissionCodes = existingPermissionMap.Keys.ToHashSet();
        foreach (var pc in permissionCodes.Where(pc => !existingPermissionCodes.Contains(pc)))
        {
            var (name, description) = PermissionMapper.GetPermissionInfo(pc);
            var permissionToAdd = new RolePermission(pc, name, description);
            await dbContext.RolePermissions.AddAsync(permissionToAdd, cancellationToken);
            dbContext.Entry(permissionToAdd).Property(nameof(RolePermission.RoleId)).CurrentValue = request.RoleId;
        }
    }
}
//#else
public class UpdateRoleInfoCommandHandler(IRoleRepository roleRepository) : ICommandHandler<UpdateRoleInfoCommand>
{
    public async Task Handle(UpdateRoleInfoCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(request.RoleId, cancellationToken) ??
                   throw new KnownException($"未找到角色，RoleId = {request.RoleId}", ErrorCodes.RoleNotFound);
        role.UpdateRoleInfo(request.Name, request.Description);

        var permissions = request.PermissionCodes.Select(perm =>
        {
            var (name, description) = PermissionMapper.GetPermissionInfo(perm);
            return new RolePermission(perm, name, description);
        });
        role.UpdateRolePermissions(permissions);
    }
}
//#endif

