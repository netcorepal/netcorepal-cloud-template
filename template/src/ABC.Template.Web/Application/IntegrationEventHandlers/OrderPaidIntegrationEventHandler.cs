using ABC.Template.Web.Application.Commands;
using DotNetCore.CAP;
using MediatR;

namespace ABC.Template.Web.Application.IntegrationEventHandlers
{
    public class OrderPaidIntegrationEventHandler : ICapSubscribe
    {
        readonly ILogger _logger;
        readonly IMediator _mediator;
        public OrderPaidIntegrationEventHandler(IMediator mediator, ILogger<OrderPaidIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        [CapSubscribe("OrderPaidIntegrationEvent")]
        public async Task HandlerAsync(OrderPaidIntegrationEvent integrationEvent)
        {
            var cmd = new OrderPaidCommand(integrationEvent.OrderId);
            await _mediator.Send(cmd);
        }


    }
}
