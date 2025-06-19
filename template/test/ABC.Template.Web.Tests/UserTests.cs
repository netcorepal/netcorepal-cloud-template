using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ABC.Template.Infrastructure;
using ABC.Template.Web.Endpoints.UserEndpoints;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

public class UserTests : IClassFixture<MyWebApplicationFactory>
{
    private readonly MyWebApplicationFactory _factory;

    private readonly HttpClient _client;

    public UserTests(MyWebApplicationFactory factory)
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }

        _factory = factory;
        _client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(p => { }); }).CreateClient();
    }


    [Fact]
    public async Task Login_And_Auth_Test()
    {
        string userName = "testname";
        string password = "testpassword";
        var loginRequest = new LoginRequest(userName, password);
        var response = await _client.PostAsJsonAsync($"/api/user/login", loginRequest);
        Assert.True(response.IsSuccessStatusCode);
        var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<string>>();
        Assert.NotNull(responseData);
        Assert.NotNull(responseData.Data);


        var jwtResponse1 = await _client.GetAsync("/api/user/auth");
        Assert.False(jwtResponse1.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, jwtResponse1.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", responseData.Data);
        var jwtResponse2 = await _client.GetAsync("/api/user/auth");
        Assert.True(jwtResponse2.IsSuccessStatusCode);
    }
}