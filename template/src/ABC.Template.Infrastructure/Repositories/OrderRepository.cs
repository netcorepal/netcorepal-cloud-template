using NetCorePal.Extensions.Repository.EntityframeworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Template.Infrastructure.Repositories
{
    internal class OrderRepository : RepositoryBase<Order, ApplicationDbContext>
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
