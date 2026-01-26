using ABC.Template.Domain.DomainEvents.RoleEvents;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.Application.Queries;

namespace ABC.Template.Web.Application.DomainEventHandlers;

/// <summary>
/// 角色信息变更领域事件处理器 - 用于更新用户角色名称
/// </summary>
public class RoleInfoChangedDomainEventHandlerForUpdateUserRoleName(IMediator mediator, UserQuery userQuery) : IDomainEventHandler<RoleInfoChangedDomainEvent>
{
    public async Task Handle(RoleInfoChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var role = domainEvent.Role;
        var roleId = role.Id;
        var newRoleName = role.Name;

        // 查询所有拥有该角色的用户ID
        var userIds = await userQuery.GetUserIdsByRoleIdAsync(roleId, cancellationToken);

        // 遍历用户ID，通过Command更新每个用户的角色名称
        foreach (var userId in userIds)
        {
            var command = new UpdateUserRoleNameCommand(userId, roleId, newRoleName);
            await mediator.Send(command, cancellationToken);
        }
    }
}
