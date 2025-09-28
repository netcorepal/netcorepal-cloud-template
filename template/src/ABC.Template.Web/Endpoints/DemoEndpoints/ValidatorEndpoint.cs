using FastEndpoints;
using FluentValidation;
using MediatR;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

public record ValidatorRequest(string Name, int Price);

public record ValidatorCommand(string Name, int Price) : ICommand;

public class ValidatorCommandHandler : ICommandHandler<ValidatorCommand>
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
public class ValidatorEndpoint(IMediator mediator) : Endpoint<ValidatorRequest, ResponseData>
{
    public override async Task HandleAsync(ValidatorRequest req, CancellationToken ct)
    {
        var cmd = new ValidatorCommand(req.Name, req.Price);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(ResponseData.Success(), cancellation: ct);
    }
}