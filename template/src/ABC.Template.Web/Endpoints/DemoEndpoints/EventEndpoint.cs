using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.DistributedTransactions;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

[Tags("Demo")]
[HttpPost("/demo/event")]
[AllowAnonymous]
public class EventEndpoint(IIntegrationEventPublisher publisher) : EndpointWithoutRequest<ResponseData<long>>
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        await publisher.PublishAsync(new OrderPaidIntegrationEvent(new OrderId(55)));
        await Send.OkAsync(55L.AsResponseData(), cancellation: ct);
    }
}