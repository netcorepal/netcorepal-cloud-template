//#if (UseDemoCode)
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.Application.Queries.Orders;
using ABC.Template.Web.Endpoints.OrderEndpoints;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

[Collection(WebAppTestCollection.Name)]
public class OrderTests(WebAppFixture app) : TestBase<WebAppFixture>
{
    [Fact]
    public async Task CreateOrder_And_PayOrder_Test()
    {
        // Arrange - Create an order first (Price must be 18-60 per validator)
        var createRequest = new CreateOrderRequest("TestOrder", 25, 2);
        var (createRsp, createRes) = await app.Client.POSTAsync<CreateOrderEndpoint, CreateOrderRequest, ResponseData<OrderId>>(createRequest);
        
        Assert.True(createRsp.IsSuccessStatusCode);
        Assert.NotNull(createRes);
        Assert.NotNull(createRes.Data);
        var orderId = createRes.Data;

        // Act - Pay for the order
        var payRequest = new PayOrderRequest(orderId);
        var (payRsp, payRes) = await app.Client.POSTAsync<PayOrderEndpoint, PayOrderRequest, ResponseData<bool>>(payRequest);
        
        Assert.True(payRsp.IsSuccessStatusCode);
        Assert.NotNull(payRes);
        Assert.True(payRes.Success);
        Assert.True(payRes.Data);

        await Task.Delay(1000, TestContext.Current.CancellationToken); //wait for IntegrationEventHandler
        
        // Verify order is marked as paid in the database
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await dbContext.Set<Domain.AggregatesModel.OrderAggregate.Order>().FirstOrDefaultAsync(o => o.Id == orderId, TestContext.Current.CancellationToken);
        Assert.NotNull(order);
        Assert.True(order.Paid);

        //verify DeliverGoods
        var goods = await dbContext.DeliverRecords.FirstOrDefaultAsync(o=>o.OrderId==orderId, TestContext.Current.CancellationToken);
        Assert.NotNull(goods);
    }

    [Fact]
    public async Task PayOrder_NonExistentOrder_ShouldFail()
    {
        // Arrange - Use a non-existent order ID
        var nonExistentOrderId = new OrderId(Guid.NewGuid());
        var payRequest = new PayOrderRequest(nonExistentOrderId);
        
        // Act
        var (payRsp, payRes) = await app.Client.POSTAsync<PayOrderEndpoint, PayOrderRequest, ResponseData<bool>>(payRequest);
        
        // Assert - Should fail
        Assert.True(payRsp.IsSuccessStatusCode);
        Assert.NotNull(payRes);
        Assert.False(payRes.Success);
    }

    [Fact]
    public async Task GetOrderById_Test()
    {
        // Arrange - Create an order first (Name max 10 chars, Price 18-60 per validator)
        var createRequest = new CreateOrderRequest("TestGet", 30, 1);
        var (createRsp, createRes) = await app.Client.POSTAsync<CreateOrderEndpoint, CreateOrderRequest, ResponseData<OrderId>>(createRequest);
        
        Assert.True(createRsp.IsSuccessStatusCode);
        Assert.NotNull(createRes);
        Assert.NotNull(createRes.Data);
        var orderId = createRes.Data;

        // Act - Get the order
        var getRequest = new GetOrderByIdRequest(orderId);
        var (getRsp, getRes) = await app.Client.GETAsync<GetOrderByIdEndpoint, GetOrderByIdRequest, ResponseData<QueryOrderResult>>(getRequest);
        
        // Assert
        Assert.True(getRsp.IsSuccessStatusCode);
        Assert.NotNull(getRes);
        Assert.True(getRes.Success);
        Assert.NotNull(getRes.Data);
        Assert.Equal(orderId, getRes.Data.Id);
    }
}
//#endif