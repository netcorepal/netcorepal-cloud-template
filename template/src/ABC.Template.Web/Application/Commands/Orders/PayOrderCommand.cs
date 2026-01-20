//#if (UseDemoCode)
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure.Repositories;

namespace ABC.Template.Web.Application.Commands.Orders;

public record PayOrderCommand(OrderId OrderId) : ICommand;

public class PayOrderCommandLock : ICommandLock<PayOrderCommand>
{
    public Task<CommandLockSettings> GetLockKeysAsync(PayOrderCommand command,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(command.OrderId.ToCommandLockSettings());
    }
}

public class PayOrderCommandHandler(IOrderRepository orderRepository) : ICommandHandler<PayOrderCommand>
{
    public async Task Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAsync(request.OrderId, cancellationToken) ??
                    throw new KnownException($"未找到订单，OrderId = {request.OrderId}");
        order.OrderPaid();
    }
}
//#endif