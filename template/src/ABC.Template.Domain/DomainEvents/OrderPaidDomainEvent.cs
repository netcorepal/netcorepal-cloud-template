using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Domain;

namespace ABC.Template.Domain.DomainEvents;

public record OrderPaidDomainEvent(Order Order) : IDomainEvent;