using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure.Repositories;

namespace ABC.Template.Web.Application.Commands.Orders;

public record CreateOrderCommand(string Name, int Price, int Count) : ICommand<OrderId>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(10).WithErrorCode("name error code");
        RuleFor(x => x.Price).InclusiveBetween(18, 60).WithErrorCode("price error code");
    }
}

public class CreateOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, OrderId>
{

    public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(request.Name, request.Count);
        order = await orderRepository.AddAsync(order, cancellationToken);
        logger.LogInformation("order created, id:{OrderId}", order.Id);
        return order.Id;
    }
}