//#if (UseDemoCode)
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands.Orders;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record PayOrderRequest(OrderId Id);

[Tags("Orders")]
[HttpPost("/api/order/pay")]
[AllowAnonymous]
public class PayOrderEndpoint(IMediator mediator) : Endpoint<PayOrderRequest, ResponseData<bool>>
{
    public override async Task HandleAsync(PayOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new PayOrderCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
//#endif