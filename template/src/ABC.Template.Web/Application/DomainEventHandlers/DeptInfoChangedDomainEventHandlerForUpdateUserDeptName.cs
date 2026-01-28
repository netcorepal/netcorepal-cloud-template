using ABC.Template.Domain.DomainEvents.DeptEvents;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.Application.Queries;

namespace ABC.Template.Web.Application.DomainEventHandlers;

/// <summary>
/// 部门信息变更领域事件处理器 - 用于更新用户部门名称
/// </summary>
public class DeptInfoChangedDomainEventHandlerForUpdateUserDeptName(IMediator mediator, UserQuery userQuery) : IDomainEventHandler<DeptInfoChangedDomainEvent>
{
    public async Task Handle(DeptInfoChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var dept = domainEvent.Dept;
        var deptId = dept.Id;
        var newDeptName = dept.Name;

        // 查询所有属于该部门的用户ID
        var userIds = await userQuery.GetUserIdsByDeptIdAsync(deptId, cancellationToken);

        // 遍历用户ID，通过Command更新每个用户的部门名称
        foreach (var userId in userIds)
        {
            var command = new UpdateUserDeptNameCommand(userId, newDeptName);
            await mediator.Send(command, cancellationToken);
        }
    }
}
