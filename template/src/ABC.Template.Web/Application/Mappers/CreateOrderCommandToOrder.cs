using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands;
using NetCorePal.Extensions.Mappers;

namespace ABC.Template.Web.Application.Mappers
{
    public class CreateOrderCommandToOrder : IMapper<CreateOrderCommand, Order>
    {
        public Order To(CreateOrderCommand from)
        {
            return new Order(from.Name, from.Count);
        }
    }
}
