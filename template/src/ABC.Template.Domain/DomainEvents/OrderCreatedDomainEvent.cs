using ABC.Template.Domain.AggregatesModel.OrderAggregate;

namespace ABC.Template.Domain.DomainEvents
{
    public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;
}
