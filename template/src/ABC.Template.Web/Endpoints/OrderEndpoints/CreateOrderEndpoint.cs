using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record CreateOrderRequest(string Name, int Price, int Count);

[Tags("Orders")]
[HttpPost("/api/order")]
[AllowAnonymous]
public class CreateOrderEndpoint(IMediator mediator) : Endpoint<CreateOrderRequest, ResponseData<OrderId>>
{
    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var cmd = new CreateOrderCommand(req.Name, req.Price, req.Count);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(id.AsResponseData(), cancellation: ct);
    }
}