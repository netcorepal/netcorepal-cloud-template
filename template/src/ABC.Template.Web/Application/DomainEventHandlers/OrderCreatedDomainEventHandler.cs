using ABC.Template.Domain.DomainEvents;
using ABC.Template.Web.Application.Commands;
using MediatR;
using NetCorePal.Extensions.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Template.Web.Application.DomainEventHandlers
{
    internal class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
    {

        IMediator _mediator;

        public OrderCreatedDomainEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }


        public Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            return _mediator.Send(new DeliverGoodsCommand(notification.Order.Id), cancellationToken);
        }
    }
}
