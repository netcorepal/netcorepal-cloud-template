using ABC.Template.Web.Application.IntegrationEventHandlers;
using DotNetCore.CAP;
using FastEndpoints;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

[Tags("Orders")]
[HttpGet("/sendEvent")]
public class SendEventEndpoint(ICapPublisher capPublisher) : Endpoint<SendEventRequest>
{
    public override async Task HandleAsync(SendEventRequest req, CancellationToken ct)
    {
        await capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(req.Id));
        await Send.OkAsync(cancellation: ct);
    }
}