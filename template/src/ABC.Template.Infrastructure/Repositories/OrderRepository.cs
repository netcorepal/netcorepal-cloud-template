using NetCorePal.Extensions.Repository.EntityframeworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Repository;

namespace ABC.Template.Infrastructure.Repositories
{

    public interface IOrderRepository : IRepository<Order, long>
    {
    }


    public class OrderRepository : RepositoryBase<Order, long, ApplicationDbContext>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
