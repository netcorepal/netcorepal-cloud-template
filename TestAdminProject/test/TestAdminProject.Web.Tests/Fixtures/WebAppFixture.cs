using Testcontainers.MySql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Microsoft.AspNetCore.Hosting;
using TestAdminProject.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace TestAdminProject.Web.Tests.Fixtures;

public class WebAppFixture : AppFixture<Program>
{
    private RedisContainer _redisContainer = null!;
    private RabbitMqContainer _rabbitMqContainer = null!;
    private MySqlContainer _databaseContainer = null!;

    protected override async ValueTask PreSetupAsync()
    {
        _redisContainer = new RedisBuilder()
            .WithCommand("--databases", "1024").Build();
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithUsername("guest").WithPassword("guest").Build();
        _databaseContainer = new MySqlBuilder()
            .WithUsername("root").WithPassword("123456")
            .WithEnvironment("TZ", "Asia/Shanghai").Build();

        var tasks = new List<Task> { _redisContainer.StartAsync() };
        tasks.Add(_rabbitMqContainer.StartAsync());
        tasks.Add(_databaseContainer.StartAsync());
        await Task.WhenAll(tasks);
        await CreateVisualHostAsync("/");
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        a.UseSetting("ConnectionStrings:Redis",
            _redisContainer.GetConnectionString());
        a.UseSetting("ConnectionStrings:MySql",
            _databaseContainer.GetConnectionString());
        a.UseSetting("RabbitMQ:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString());
        a.UseSetting("RabbitMQ:UserName", "guest");
        a.UseSetting("RabbitMQ:Password", "guest");
        a.UseSetting("RabbitMQ:VirtualHost", "/");
        a.UseSetting("RabbitMQ:HostName", _rabbitMqContainer.Hostname);
        a.UseEnvironment("Development");
    }

    private async Task CreateVisualHostAsync(string visualHost)
    {
        await _rabbitMqContainer.ExecAsync(["rabbitmqctl", "add_vhost", visualHost]);
        await _rabbitMqContainer.ExecAsync(["rabbitmqctl", "set_permissions", "-p", visualHost, "guest", ".*", ".*", ".*"
        ]);
    }
}
