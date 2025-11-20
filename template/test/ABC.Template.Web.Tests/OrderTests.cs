using System.Net.Http.Json;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.Application.Queries.Orders;
using ABC.Template.Web.Endpoints.OrderEndpoints;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

[Collection("web")]
public class OrderTests : IClassFixture<MyWebApplicationFactory>
{
    private readonly MyWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrderTests(MyWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(p => { }); }).CreateClient();
    }

    [Fact]
    public async Task CreateOrder_And_PayOrder_Test()
    {
        // Arrange - Create an order first (Price must be 18-60 per validator)
        var createRequest = new CreateOrderRequest("TestOrder", 25, 2);
        var createResponse = await _client.PostAsNewtonsoftJsonAsync("/api/order", createRequest);
        Assert.True(createResponse.IsSuccessStatusCode);
        
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<OrderId>>();
        Assert.NotNull(createResponseData);
        Assert.NotNull(createResponseData.Data);
        var orderId = createResponseData.Data;

        // Act - Pay for the order
        var payRequest = new PayOrderRequest(orderId);
        var payResponse = await _client.PostAsNewtonsoftJsonAsync("/api/order/pay", payRequest);
        Assert.True(payResponse.IsSuccessStatusCode);
        
        var payResponseData = await payResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(payResponseData);
        Assert.True(payResponseData.Success);
        Assert.True(payResponseData.Data);

        await Task.Delay(1000); //wait for IntegrationEventHandler
        
        // Verify order is marked as paid in the database
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await dbContext.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.True(order.Paid);

        //verify DeliverGoods
        var goods = await dbContext.DeliverRecords.FirstOrDefaultAsync(o=>o.OrderId==orderId);
        Assert.NotNull(goods);
    }

    [Fact]
    public async Task PayOrder_NonExistentOrder_ShouldFail()
    {
        // Arrange - Use a non-existent order ID
        var nonExistentOrderId = new OrderId(Guid.NewGuid());
        var payRequest = new PayOrderRequest(nonExistentOrderId);
        
        // Act
        var payResponse = await _client.PostAsNewtonsoftJsonAsync("/api/order/pay", payRequest);
        
        // Assert - Should fail
        Assert.True(payResponse.IsSuccessStatusCode);
        var payResponseData = await payResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<bool>>();
        Assert.NotNull(payResponseData);
        Assert.False(payResponseData.Success);
    }

    [Fact]
    public async Task GetOrderById_Test()
    {
        // Arrange - Create an order first (Name max 10 chars, Price 18-60 per validator)
        var createRequest = new CreateOrderRequest("TestGet", 30, 1);
        var createResponse = await _client.PostAsNewtonsoftJsonAsync("/api/order", createRequest);
        Assert.True(createResponse.IsSuccessStatusCode);
        
        var createResponseData = await createResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<OrderId>>();
        Assert.NotNull(createResponseData);
        Assert.NotNull(createResponseData.Data);
        var orderId = createResponseData.Data;

        // Act - Get the order
        var getResponse = await _client.GetAsync($"/api/order/{orderId.Id}");
        
        // Assert
        Assert.True(getResponse.IsSuccessStatusCode);
        var getResponseData = await getResponse.Content.ReadFromNewtonsoftJsonAsync<ResponseData<QueryOrderResult>>();
        Assert.NotNull(getResponseData);
        Assert.True(getResponseData.Success);
        Assert.NotNull(getResponseData.Data);
        Assert.Equal(orderId, getResponseData.Data.Id);
    }
}
