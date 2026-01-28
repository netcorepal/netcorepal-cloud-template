using ABC.Template.Domain.AggregatesModel.DeptAggregate;

namespace ABC.Template.Domain.DomainEvents.DeptEvents;

/// <summary>
/// 部门信息变更领域事件
/// </summary>
public record DeptInfoChangedDomainEvent(Dept Dept) : IDomainEvent;
