using ABC.Template.Domain.AggregatesModel.OrderAggregate;

namespace ABC.Template.Web.Application.IntegrationEventHandlers
{
    public record OrderPaidIntegrationEvent(OrderId OrderId);
}
