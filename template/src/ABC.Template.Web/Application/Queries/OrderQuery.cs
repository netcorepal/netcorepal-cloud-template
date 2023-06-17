using ABC.Template.Domain;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure;
using System.Threading;

namespace ABC.Template.Web.Application.Queries
{
    public class OrderQuery
    {
        readonly ApplicationDbContext _applicationDbContext;
        public OrderQuery(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Order?> QueryOrder(OrderId orderId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.Orders.FindAsync(new object[] { orderId }, cancellationToken);
        }
    }
}
