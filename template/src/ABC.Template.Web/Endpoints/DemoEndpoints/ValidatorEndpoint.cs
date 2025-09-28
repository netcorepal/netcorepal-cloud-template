using ABC.Template.Web.Application.Commands;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

public record ValidatorRequest(string Name, int Price);

[Tags("Demo")]
[HttpPost("/demo/validator")]
[AllowAnonymous]
public class ValidatorEndpoint(IMediator mediator) : Endpoint<ValidatorRequest, ResponseData>
{
    public override async Task HandleAsync(ValidatorRequest req, CancellationToken ct)
    {
        var cmd = new ValidatorCommand(req.Name, req.Price);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}