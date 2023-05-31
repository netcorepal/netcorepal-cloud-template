using MediatR;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public record DemoCreateCommand(string Name, int Age) : ICommand
    {
    }
}
