using FluentValidation;

namespace ABC.Template.Web.Application.Commands
{
    public class DemoCreateCommandValidator : AbstractValidator<DemoCreateCommand>
    {
        public DemoCreateCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Age).InclusiveBetween(18, 60);
        }
    }
}
