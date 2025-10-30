using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace ABC.Template.Web.Tests.Extensions;

public class MyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestContainerFixture Containers = new TestContainerFixture();

    static MyWebApplicationFactory()
    {
        NewtonsoftJsonDefaults.DefaultOptions.AddNetCorePalJsonConverters();
    }

    public MyWebApplicationFactory()
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
<!--#if (UseAspire)-->
        // When using Aspire, connection strings use resource names
        builder.UseSetting("ConnectionStrings:redis",
            Containers.RedisContainer.GetConnectionString() + ",defaultDatabase=0");
//#if (UseMySql)
        builder.UseSetting("ConnectionStrings:demo",
            Containers.DatabaseContainer.GetConnectionString().Replace("mysql", "mysql"));
//#elif (UseSqlServer)
        builder.UseSetting("ConnectionStrings:demo",
            Containers.DatabaseContainer.GetConnectionString());
//#elif (UsePostgreSQL)
        builder.UseSetting("ConnectionStrings:demo",
            Containers.DatabaseContainer.GetConnectionString());
//#endif
//#if (UseRabbitMQ)
        builder.UseSetting("ConnectionStrings:rabbitmq",
            $"amqp://guest:guest@{Containers.RabbitMqContainer.Hostname}:{Containers.RabbitMqContainer.GetMappedPublicPort(5672)}/");
//#elif (UseKafka)
        builder.UseSetting("ConnectionStrings:kafka", Containers.KafkaContainer.GetBootstrapAddress());
//#elif (UseNATS)
        builder.UseSetting("ConnectionStrings:nats", Containers.NatsContainer.GetConnectionString());
//#elif (UseRedisStreams)
        // RedisStreams uses the same redis connection string
//#endif
<!--#else-->
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
//#elif (UseNATS)
        builder.UseSetting("NATS:Servers", Containers.NatsContainer.GetConnectionString());
//#endif
<!--#endif-->
        builder.UseEnvironment("Development");
        base.ConfigureWebHost(builder);
    }

    public async Task InitializeAsync()
    {
        await Containers.InitializeAsync();
//#if (UseRabbitMQ)
        await Containers.CreateVisualHostAsync("/");
//#endif
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Containers.DisposeAsync();
    }
}
