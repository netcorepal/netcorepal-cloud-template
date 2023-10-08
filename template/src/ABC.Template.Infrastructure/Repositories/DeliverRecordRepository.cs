using NetCorePal.Extensions.Repository.EntityFrameworkCore;
using NetCorePal.Extensions.Repository;
using ABC.Template.Domain.AggregatesModel.DeliverAggregate;

namespace ABC.Template.Infrastructure.Repositories
{
    public interface IDeliverRecordRepository : IRepository<DeliverRecord, DeliverRecordId>
    {

    }

    public class DeliverRecordRepository(ApplicationDbContext context) : RepositoryBase<DeliverRecord, DeliverRecordId, ApplicationDbContext>(context), IDeliverRecordRepository
    {
    }
}
