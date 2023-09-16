using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using NetCorePal.Extensions.Repository;

namespace ABC.Template.Infrastructure.Repositories
{

    public interface IOrderRepository : IRepository<Order, OrderId>
    {

    }


    public class OrderRepository : RepositoryBase<Order, OrderId, ApplicationDbContext>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
