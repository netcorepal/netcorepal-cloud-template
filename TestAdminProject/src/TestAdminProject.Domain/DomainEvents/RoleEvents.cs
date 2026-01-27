using TestAdminProject.Domain.AggregatesModel.RoleAggregate;

namespace TestAdminProject.Domain.DomainEvents.RoleEvents;

public record RoleInfoChangedDomainEvent(Role Role) : IDomainEvent;
public record RolePermissionChangedDomainEvent(Role Role) : IDomainEvent;
