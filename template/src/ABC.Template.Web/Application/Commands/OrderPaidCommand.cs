using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public record class OrderPaidCommand(OrderId OrderId) : ICommand;
}
