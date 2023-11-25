using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using NetCorePal.Extensions.AspNetCore.Json;

namespace ABC.Template.Web.Tests
{
    public class MyWebApplicationFactory(TestContainerFixture containers) : WebApplicationFactory<Program>
    {
        static MyWebApplicationFactory()
        {
            NewtonsoftJsonDefaults.DefaultOptions.Converters.Add(new NewtonsoftEntityIdJsonConverter());
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //builder.UseSetting("ConnectionStrings:PostgreSQL", postgreSqlContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:Redis", containers.RedisContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:MySql", containers.MySqlContainer.GetConnectionString());
            builder.UseSetting("RabbitMQ:Port", containers.RabbitMqContainer.GetMappedPublicPort(5672).ToString());
            builder.UseSetting("RabbitMQ:UserName", "guest");
            builder.UseSetting("RabbitMQ:Password", "guest");
            builder.UseSetting("RabbitMQ:VirtualHost", "/");
            builder.UseSetting("RabbitMQ:HostName", containers.RabbitMqContainer.Hostname);
            builder.UseEnvironment("Development");
            base.ConfigureWebHost(builder);
        }
    }
}