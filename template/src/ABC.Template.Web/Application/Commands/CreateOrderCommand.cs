using MediatR;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public record CreateOrderCommand(string Name, int Price, int Count) : ICommand<long>
    {
    }
}
