using ABC.Template.Web.Application.Commands;
using ABC.Template.Web.Application.IntegrationEvents;

namespace ABC.Template.Web.Application.IntegrationEventHandlers;

public class OrderPaidIntegrationEventHandlerForDeliverGoods(IMediator mediator) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    public async Task HandleAsync(OrderPaidIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        var cmd = new DeliverGoodsCommand(eventData.OrderId);
        _ = await mediator.Send(cmd, cancellationToken);
    }
}