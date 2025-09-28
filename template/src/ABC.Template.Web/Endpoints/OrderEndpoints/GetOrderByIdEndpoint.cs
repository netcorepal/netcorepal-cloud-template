using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Queries;
using FastEndpoints;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record GetOrderByIdRequest(OrderId Id);

[Tags("Orders")]
[HttpGet("/get/{Id}")]
public class GetOrderByIdEndpoint(OrderQuery orderQuery) : Endpoint<GetOrderByIdRequest, ResponseData<Order?>>
{
    public override async Task HandleAsync(GetOrderByIdRequest req, CancellationToken ct)
    {
        var order = await orderQuery.QueryOrder(req.Id, ct);
        await Send.OkAsync(order.AsResponseData(), cancellation: ct);
    }
}