using ABC.Template.Web.Application.Commands;
using DotNetCore.CAP;
using MediatR;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.IntegrationEventHandlers
{
    public class OrderPaidIntegrationEventHandler(IMediator mediator) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
    {
        public async Task HandleAsync(OrderPaidIntegrationEvent eventData, CancellationToken cancellationToken = default)
        {
            var cmd = new DeliverGoodsCommand(eventData.OrderId);
            _ = await mediator.Send(cmd, cancellationToken);
        }
    }
}