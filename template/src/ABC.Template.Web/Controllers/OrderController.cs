using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Web.Application.Commands;
using ABC.Template.Web.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Domain;

namespace ABC.Template.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        readonly IMediator _mediator;
        readonly OrderQuery _orderQuery;
        public OrderController(IMediator mediator, OrderQuery orderQuery)
        {
            _mediator = mediator;
            _orderQuery = orderQuery;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }



        [HttpPost]

        public async Task<IActionResult> Post([FromBody] CreateOrderCommand command)
        {
            await _mediator.Send(command);
            return Ok("Hello World");
        }


        [HttpGet]
        [Route("/get/{id}")]
        public async Task<Entity?> GetById([FromQuery]long id)
        {
            return await _orderQuery.QueryOrder(id, HttpContext.RequestAborted);
        }


    }
}
