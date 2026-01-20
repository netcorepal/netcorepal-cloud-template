//#if (UseAdmin)
using ABC.Template.Domain.AggregatesModel.RoleAggregate;

namespace ABC.Template.Domain.DomainEvents.RoleEvents;

public record RoleInfoChangedDomainEvent(Role Role) : IDomainEvent;
public record RolePermissionChangedDomainEvent(Role Role) : IDomainEvent;
//#endif
