using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace ABC.Template.Web.Application.Queries.Orders;


public record QueryOrder(OrderId Id) : IQuery<QueryOrderResult>;

public record QueryOrderResult(OrderId Id, string Name, int Count);

public class OrderQueryHandler(ApplicationDbContext applicationDbContext)
    : IQueryHandler<QueryOrder, QueryOrderResult>
{
    public async Task<QueryOrderResult> Handle(QueryOrder request, CancellationToken cancellationToken)
    {
        var result = await applicationDbContext.Orders.Where(p => p.Id == request.Id)
                .Select(p => new QueryOrderResult(p.Id, p.Name, p.Count))
                .FirstOrDefaultAsync(cancellationToken);
        return result ?? throw new KnownException("Order not found");
    }
}