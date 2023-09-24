using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.Application.Sagas;
using DotNetCore.CAP;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.DistributedTransactions.Sagas;

namespace ABC.Template.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        readonly IMediator _mediator;
        readonly OrderQuery _orderQuery;
        readonly ICapPublisher _capPublisher;
        public OrderController(IMediator mediator, OrderQuery orderQuery, ICapPublisher capPublisher)
        {
            _mediator = mediator;
            _orderQuery = orderQuery;
            _capPublisher = capPublisher;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }



        [HttpPost]

        public async Task<OrderId> Post([FromBody] CreateOrderCommand command)
        {
            var id = await _mediator.Send(command);
            return id;
        }


        [HttpGet]
        [Route("/get/{id}")]
        public async Task<ResponseData<Order?>> GetById([FromRoute] OrderId id)
        {
            var order = await _orderQuery.QueryOrder(id, HttpContext.RequestAborted).AsResponseData();
            return order;
        }





        [HttpGet]
        [Route("/sendEvent")]
        public async Task SendEvent(OrderId id)
        {
            await _capPublisher.PublishAsync("OrderPaidIntegrationEvent", new OrderPaidIntegrationEvent(id));
        }


        [HttpGet]
        [Route("/saga")]
        public async Task<ResponseData> Saga([FromServices] ISagaManager sagaManager)
        {
            return await sagaManager.SendAsync<DemoSaga, DemoSagaData>(new DemoSagaData { SagaId = Guid.NewGuid() }, HttpContext.RequestAborted).AsResponseData();
        }



    }
}
