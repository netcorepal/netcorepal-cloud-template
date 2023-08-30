using ABC.Template.Domain.AggregatesModel.DeliverAggregate;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public record DeliverGoodsCommand(OrderId OrderId) : ICommand<DeliverRecordId>
    {
    }
}
