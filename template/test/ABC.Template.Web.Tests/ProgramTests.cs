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
            // Arrange
            // var client = factory.CreateClient();
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