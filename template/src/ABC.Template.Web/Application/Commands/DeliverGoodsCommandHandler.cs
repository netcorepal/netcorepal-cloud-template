using ABC.Template.Domain.AggregatesModel.DeliverAggregate;
using ABC.Template.Infrastructure.Repositories;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public class DeliverGoodsCommandHandler : ICommandHandler<DeliverGoodsCommand, DeliverRecordId>
    {
        IDeliverRecordRepository _deliverRecordRepository;
        public DeliverGoodsCommandHandler(IDeliverRecordRepository deliverRecordRepository)
        {
            _deliverRecordRepository = deliverRecordRepository;
        }

        public Task<DeliverRecordId> Handle(DeliverGoodsCommand request, CancellationToken cancellationToken)
        {
            var record = new DeliverRecord(request.OrderId);
            _deliverRecordRepository.Add(record);
            return Task.FromResult(record.Id);
        }
    }
}
