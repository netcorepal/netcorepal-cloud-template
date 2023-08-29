using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ABC.Template.Web.Tests
{
    public class ProgramTests : IClassFixture<MyWebApplicationFactory>
    {

        private readonly MyWebApplicationFactory _factory;

        public ProgramTests(MyWebApplicationFactory factory)
        {
            _factory = factory;
        }


        [Fact]
        public void HealthCheckTest()
        {
            var client = _factory.CreateClient();
            var response = client.GetAsync("/health").Result;
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}