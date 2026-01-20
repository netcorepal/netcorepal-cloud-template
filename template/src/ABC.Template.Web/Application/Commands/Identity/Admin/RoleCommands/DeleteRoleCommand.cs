//#if (UseAdmin)
using FluentValidation;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.RoleCommands;

/// <summary>
/// 删除角色命令
/// </summary>
/// <param name="RoleId">角色ID</param>
public record DeleteRoleCommand(RoleId RoleId) : ICommand;

/// <summary>
/// 删除角色命令验证器
/// </summary>
public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("角色ID不能为空");
    }
}

/// <summary>
/// 删除角色命令处理器
/// </summary>
public class DeleteRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new KnownException("角色不存在", ErrorCodes.RoleNotFound);
        }

        // 检查是否为管理员用户，防止删除管理员
        if (role.Name.ToLower() == "admin")
        {
            throw new KnownException("不能删除管理员角色", ErrorCodes.CannotDeleteAdminRole);
        }
        role.SoftDelete();
    }
}
//#endif
