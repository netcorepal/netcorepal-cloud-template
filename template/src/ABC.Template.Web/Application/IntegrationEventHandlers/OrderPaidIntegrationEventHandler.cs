using ABC.Template.Domain.DomainEvents;
using ABC.Template.Infrastructure.Repositories;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Metadata;
using NetCorePal.Extensions.Repository;

namespace ABC.Template.Web.Application.IntegrationEventHandlers
{
    public class OrderPaidIntegrationEventHandler : ICapSubscribe
    {

        readonly IOrderRepository _orderRepository;
        readonly ILogger _logger;
        readonly IUnitOfWork _unitOfWork;
        public OrderPaidIntegrationEventHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<OrderPaidIntegrationEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        [CapSubscribe("OrderPaidIntegrationEvent")]
        public async Task HandlerAsync(OrderPaidIntegrationEvent integrationEvent)
        {
            var order = await _orderRepository.GetAsync(integrationEvent.OrderId);
            if (order == null)
            {
                _logger.LogWarning("收到OrderPaidIntegrationEvent，OrderId = {orderid},但未找到对应订单", integrationEvent.OrderId);
            }
            else
            {
                order.OrderPaid();
            }
            await _unitOfWork.SaveEntitiesAsync();
        }


    }
}
