using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Repository;

namespace ABC.Template.Infrastructure.Repositories;

public interface IOrderRepository : IRepository<Order, OrderId>
{
}

public class OrderRepository(ApplicationDbContext context) : RepositoryBase<Order, OrderId, ApplicationDbContext>(context), IOrderRepository
{
}

