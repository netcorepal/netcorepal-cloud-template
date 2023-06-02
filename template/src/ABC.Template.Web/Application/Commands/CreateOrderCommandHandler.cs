using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
    {
        public Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
