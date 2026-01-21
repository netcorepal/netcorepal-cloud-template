using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Queries.Orders;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record GetOrderByIdRequest(OrderId Id);

[Tags("Orders")]
[HttpGet("/api/order/{Id}")]
[AllowAnonymous]
public class GetOrderByIdEndpoint(IMediator mediator) : Endpoint<GetOrderByIdRequest, ResponseData<QueryOrderResult>>
{
    public override async Task HandleAsync(GetOrderByIdRequest req, CancellationToken ct)
    {
        var order = await mediator.Send(new QueryOrder(req.Id), ct);
        await Send.OkAsync(order.AsResponseData(), cancellation: ct);
    }
}