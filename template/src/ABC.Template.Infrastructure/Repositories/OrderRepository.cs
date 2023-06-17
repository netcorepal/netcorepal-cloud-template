using NetCorePal.Extensions.Repository.EntityframeworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Repository;
using ABC.Template.Domain;

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
