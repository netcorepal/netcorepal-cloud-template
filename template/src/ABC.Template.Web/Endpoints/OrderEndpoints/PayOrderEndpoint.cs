using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record PayOrderRequest(OrderId Id);

[Tags("Orders")]
[HttpPost("/payOrder")]
[AllowAnonymous]
public class PayOrderEndpoint(IMediator mediator) : Endpoint<PayOrderRequest>
{
    public override async Task HandleAsync(PayOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new PayOrderCommand(req.Id), ct);
        await Send.OkAsync(cancellation: ct);
    }
}