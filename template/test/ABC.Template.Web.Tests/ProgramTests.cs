using System.Net.Http.Json;
using ABC.Template.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABC.Template.Web.Tests
{
    [Collection("web")]
    public class ProgramTests : IClassFixture<MyWebApplicationFactory>
    {
        private readonly MyWebApplicationFactory _factory;

        private readonly HttpClient _client;

        public ProgramTests(MyWebApplicationFactory factory)
        {
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }
            
            _factory = factory;
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task HealthCheckTest()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/health");
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}