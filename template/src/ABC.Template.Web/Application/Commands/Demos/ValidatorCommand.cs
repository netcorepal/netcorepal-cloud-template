using FluentValidation;
using NetCorePal.Extensions.Domain;

namespace ABC.Template.Web.Application.Commands;

public record ValidatorCommand(string Name, int Price) : ICommand;

public class ValidatorCommandValidator : AbstractValidator<ValidatorCommand>
{
    public ValidatorCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("不能为空").WithErrorCode("code1");
        RuleFor(x => x.Price).InclusiveBetween(18, 60).WithMessage("价格必须在18-60之间").WithErrorCode("code2");
    }
}

public class ValidatorCommandHandler : ICommandHandler<ValidatorCommand>
{
    public Task Handle(ValidatorCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}