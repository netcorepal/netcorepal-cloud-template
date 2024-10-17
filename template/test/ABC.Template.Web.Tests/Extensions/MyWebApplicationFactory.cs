using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using NetCorePal.Extensions.AspNetCore.Json;

namespace ABC.Template.Web.Tests.Extensions;

public class MyWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly TestContainerFixture Containers = new TestContainerFixture();
    private static int _index = 0;
    private static int _portIndex = 8080;
    private readonly int InstanceIndex;

    static MyWebApplicationFactory()
    {
        NewtonsoftJsonDefaults.DefaultOptions.Converters.Add(new NewtonsoftEntityIdJsonConverter());
    }

    public MyWebApplicationFactory()
    {
        InstanceIndex = _index++;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Redis",
            Containers.RedisContainer.GetConnectionString() + ",defaultDatabase=" + InstanceIndex);
        builder.UseSetting("ConnectionStrings:MySql",
            Containers.MySqlContainer.GetConnectionString().Replace("mysql", "mysql" + InstanceIndex));
        builder.UseSetting("RabbitMQ:Port", Containers.RabbitMqContainer.GetMappedPublicPort(5672).ToString());
        builder.UseSetting("RabbitMQ:UserName", "guest");
        builder.UseSetting("RabbitMQ:Password", "guest");
        builder.UseSetting("RabbitMQ:VirtualHost", "v" + InstanceIndex);
        builder.UseSetting("RabbitMQ:HostName", Containers.RabbitMqContainer.Hostname);
        var port = _portIndex++;
        var url = $"http://*:{port}";
        builder.UseSetting("Urls", url);
        this.ClientOptions.BaseAddress = new Uri($"http://localhost:{port}");
        builder.UseEnvironment("Development");
        base.ConfigureWebHost(builder);
    }

    public async Task InitializeAsync()
    {
        await Containers.CreateVisualHostAsync("v" + InstanceIndex);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}