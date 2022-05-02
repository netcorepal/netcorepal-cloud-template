using ABC.Extensions.Domain;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;

namespace ABC.Template.Domain.DomainEvents
{
    public class OrderCreatedDomainEvent : IDomainEvent
    {
        public OrderCreatedDomainEvent(Order order)
        {
            Order = order;
        }

        public Order Order { get; }

    }
}
