//#if (UseDemoCode)
using ABC.Template.Domain.DomainEvents;
using ABC.Template.Web.Application.IntegrationEvents;

namespace ABC.Template.Web.Application.IntegrationEventConverters;

public class OrderPaidIntegrationEventConverter
    : IIntegrationEventConverter<OrderPaidDomainEvent, OrderPaidIntegrationEvent>
{
    public OrderPaidIntegrationEvent Convert(OrderPaidDomainEvent domainEvent)
    {
        return new OrderPaidIntegrationEvent(domainEvent.Order.Id);
    }
}
//#endif
