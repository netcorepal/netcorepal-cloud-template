using ABC.Template.Domain.AggregatesModel.OrderAggregate;

namespace ABC.Template.Web.Application.IntegrationEvents;

public record OrderPaidIntegrationEvent(OrderId OrderId);
