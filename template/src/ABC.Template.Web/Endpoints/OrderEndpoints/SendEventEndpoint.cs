using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using DotNetCore.CAP;
using FastEndpoints;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record SendEventRequest(OrderId Id);

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