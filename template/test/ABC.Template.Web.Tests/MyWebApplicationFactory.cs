using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Testcontainers.PostgreSql;

namespace ABC.Template.Web.Tests
{
    public class MyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {

        private readonly RedisContainer redisContainer = new RedisBuilder().WithHostname("redishost").WithExposedPort(6380).Build();
        private readonly RabbitMqContainer rabbitMqContainer = new RabbitMqBuilder().WithHostname("rabbitmqhost").WithExposedPort(5673).WithUsername("guest").WithPassword("guest").Build();
        private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder().WithHostname("postgresqlhost").WithExposedPort(5433).WithUsername("postgres").WithPassword("123456").WithDatabase("demo").Build();


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            base.ConfigureWebHost(builder);
        }

        public Task InitializeAsync()
        {
            return Task.WhenAll(redisContainer.StartAsync(),
                                rabbitMqContainer.StartAsync(),
                                postgreSqlContainer.StartAsync());
        }

        public new Task DisposeAsync()
        {
            return Task.WhenAll(redisContainer.StopAsync(),
                                rabbitMqContainer.StopAsync(),
                                postgreSqlContainer.StopAsync());
        }


    }
}
