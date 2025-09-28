using ABC.Template.Domain.AggregatesModel.OrderAggregate;

namespace ABC.Template.Web.Endpoints.OrderEndpoints;

public record CreateOrderRequest(string Name, int Price, int Count);

public record GetOrderByIdRequest(OrderId Id);

public record SendEventRequest(OrderId Id);