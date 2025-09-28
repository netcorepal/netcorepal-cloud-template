using ABC.Template.Web.Application.IntegrationEventHandlers;
using DotNetCore.CAP;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

[Tags("Orders")]
[HttpGet("/sendEvent")]
[AllowAnonymous]
public class SendEventEndpoint(ICapPublisher capPublisher) : Endpoint<SendEventRequest>
{
    public override async Task HandleAsync(SendEventRequest req, CancellationToken ct)
    {
        await capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(req.Id));
        await Send.OkAsync(cancellation: ct);
    }
}