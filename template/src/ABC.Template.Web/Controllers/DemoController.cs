using ABC.Template.Web.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ABC.Template.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        readonly IMediator _mediator;
        public DemoController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody]DemoCreateCommand command)
        {
            await _mediator.Send(command);
            return Ok("Hello World");
        }


    }
}
