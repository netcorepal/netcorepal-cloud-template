using FluentValidation;
using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

public partial record My2Id : IInt64StronglyTypedId;

public partial record MyId : IInt64StronglyTypedId;

public record JsonRequest(MyId Id, string Name, DateTime Time);

public record JsonResponse(MyId Id, string Name, DateTime Time);

public record ValidatorCommand(string Name, int Price) : ICommand;

public record ValidatorRequest(string Name, int Price);

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