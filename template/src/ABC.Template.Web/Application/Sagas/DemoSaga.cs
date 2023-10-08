using NetCorePal.Extensions.DistributedTransactions.Sagas;

namespace ABC.Template.Web.Application.Sagas
{

    public class DemoSagaData : SagaData
    {
    }


    public record DemoEvent(Guid SagaId) : ISagaEvent;


    public class DemoSaga(ISagaContext<DemoSagaData> context) : Saga<DemoSagaData>(context), ISagaEventHandler<DemoEvent>
    {
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
