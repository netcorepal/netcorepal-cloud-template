using NetCorePal.Extensions.Repository.EntityframeworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCorePal.Extensions.Repository;
using ABC.Template.Domain;
using ABC.Template.Domain.AggregatesModel.DeliverAggregate;

namespace ABC.Template.Infrastructure.Repositories
{
    public interface IDeliverRecordRepository : IRepository<DeliverRecord, DeliverRecordId>
    {

    }

    public class DeliverRecordRepository : RepositoryBase<DeliverRecord, DeliverRecordId, ApplicationDbContext>, IDeliverRecordRepository
    {
        public DeliverRecordRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
