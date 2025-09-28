using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using ABC.Template.Web.Application.Queries;
using DotNetCore.CAP;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IMediator mediator, OrderQuery orderQuery, ICapPublisher capPublisher) : ControllerBase
{
    [HttpPost]
    public async Task<ResponseData<OrderId>> Post([FromBody] CreateOrderRequest request)
    {
        var cmd = new CreateOrderCommand(request.Name, request.Price, request.Count);
        var id = await mediator.Send(cmd);
        return id.AsResponseData();
    }


    [HttpGet]
    [Route("/get/{id}")]
    public async Task<ResponseData<Order?>> GetById([FromRoute] OrderId id)
    {
        var order = await orderQuery.QueryOrder(id, HttpContext.RequestAborted).AsResponseData();
        return order;
    }


    [HttpGet]
    [Route("/sendEvent")]
    public async Task SendEvent(OrderId id)
    {
        await capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(id));
    }
}

public record CreateOrderRequest(required string Name, required int Price, required int Count);