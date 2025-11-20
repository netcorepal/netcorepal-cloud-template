using ABC.Template.Domain.DomainEvents;
using ABC.Template.Web.Application.Commands.Delivers;

namespace ABC.Template.Web.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandlerForDeliverGoods(IMediator mediator) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
    }
}