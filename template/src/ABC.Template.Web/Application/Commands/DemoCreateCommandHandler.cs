using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public class DemoCreateCommandHandler : ICommandHandler<DemoCreateCommand>
    {
        public Task Handle(DemoCreateCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
