using NetCorePal.Extensions.DistributedTransactions.Sagas;

namespace ABC.Template.Web.Application.Sagas
{

    public class DemoSagaData : SagaData
    {
    }


    public class DemoEvent : ISagaEvent
    {
        public DemoEvent(Guid sagaId)
        {
            SagaId = sagaId;
        }

        public Guid SagaId { get; set; }
    }


    public class DemoSaga : Saga<DemoSagaData>, ISagaEventHandler<DemoEvent>
    {
        public DemoSaga(ISagaContext<DemoSagaData> context) : base(context)
        {
        }

        public Task HandleAsync(DemoEvent eventData, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override async Task OnStart(DemoSagaData data, CancellationToken cancellationToken = default)
        {
            await Context.EventPublisher.PublishAsync(new DemoEvent(data.SagaId), cancellationToken);
        }
    }
}
