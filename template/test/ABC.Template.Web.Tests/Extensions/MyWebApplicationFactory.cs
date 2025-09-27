using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace ABC.Template.Web.Tests.Extensions;

public class MyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestContainerFixture Containers = new TestContainerFixture();

    static MyWebApplicationFactory()
    {
        NewtonsoftJsonDefaults.DefaultOptions.Converters.Add(new NewtonsoftEntityIdJsonConverter());
    }

    public MyWebApplicationFactory()
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Redis",
            Containers.RedisContainer.GetConnectionString() + ",defaultDatabase=0");
//#if (UseMySql)
        builder.UseSetting("ConnectionStrings:MySql",
            Containers.DatabaseContainer.GetConnectionString().Replace("mysql", "mysql"));
//#elif (UseSqlServer)
        builder.UseSetting("ConnectionStrings:SqlServer",
            Containers.DatabaseContainer.GetConnectionString());
//#elif (UsePostgreSQL)
        builder.UseSetting("ConnectionStrings:PostgreSQL",
            Containers.DatabaseContainer.GetConnectionString());
//#endif
//#if (UseRabbitMQ)
        builder.UseSetting("RabbitMQ:Port", Containers.RabbitMqContainer.GetMappedPublicPort(5672).ToString());
        builder.UseSetting("RabbitMQ:UserName", "guest");
        builder.UseSetting("RabbitMQ:Password", "guest");
        builder.UseSetting("RabbitMQ:VirtualHost", "/");
        builder.UseSetting("RabbitMQ:HostName", Containers.RabbitMqContainer.Hostname);
//#elif (UseKafka)
        builder.UseSetting("Kafka:BootstrapServers", Containers.KafkaContainer.GetBootstrapAddress());
//#endif
        builder.UseEnvironment("Development");
        base.ConfigureWebHost(builder);
    }

    public async Task InitializeAsync()
    {
//#if (UseRabbitMQ)
        await Containers.CreateVisualHostAsync("/");
//#endif
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}