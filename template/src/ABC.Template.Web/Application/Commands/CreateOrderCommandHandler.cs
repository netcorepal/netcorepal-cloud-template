using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using NetCorePal.Extensions.Mappers;
using NetCorePal.Extensions.Primitives;
using NetCorePal.Extensions.Repository;

namespace ABC.Template.Web.Application.Commands
{
    public class CreateOrderCommandHandler(IOrderRepository orderRepository, IMapperProvider mapperProvider, ILogger<OrderPaidIntegrationEventHandler> logger) : ICommandHandler<CreateOrderCommand, OrderId>
    {

        public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.MapTo<Order>(mapperProvider);
            order = await orderRepository.AddAsync(order, cancellationToken);
            logger.LogInformation("order created, id:{orderId}", order.Id);
            return order.Id;
        }
    }
}
