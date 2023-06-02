using FluentValidation;

namespace ABC.Template.Web.Application.Commands
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(10);
            RuleFor(x => x.Age).InclusiveBetween(18, 60);
        }
    }
}
