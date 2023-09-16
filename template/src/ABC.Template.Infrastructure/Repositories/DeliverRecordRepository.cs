using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Extensions.Repository;
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
