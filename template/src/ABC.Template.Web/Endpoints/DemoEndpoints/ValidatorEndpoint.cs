using FastEndpoints;
using MediatR;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

[Tags("Demo")]
[HttpPost("/demo/validator")]
public class ValidatorEndpoint(IMediator mediator) : Endpoint<ValidatorRequest, ResponseData>
{
    public override async Task HandleAsync(ValidatorRequest req, CancellationToken ct)
    {
        var cmd = new ValidatorCommand(req.Name, req.Price);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(ResponseData.Success(), cancellation: ct);
    }
}