using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

public record ValidatorCommand(string Name, int Price) : NetCorePal.Extensions.Domain.ICommand;

public record ValidatorRequest(string Name, int Price);

public class ValidatorCommandHandler : NetCorePal.Extensions.Domain.ICommandHandler<ValidatorCommand>
{
    public Task Handle(ValidatorCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class ValidatorCommandValidator : AbstractValidator<ValidatorCommand>
{
    public ValidatorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("不能为空").WithErrorCode("code1");
        RuleFor(x => x.Price).InclusiveBetween(18, 60).WithMessage("价格必须在18-60之间").WithErrorCode("code2");
    }
}

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