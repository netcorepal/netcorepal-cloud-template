using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Application.Commands
{
    public record class OrderPaidCommand(long OrderId) : ICommand
    {
    }

    
}
