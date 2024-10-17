using Testcontainers.MySql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace ABC.Template.Web.Tests.Extensions;

public class TestContainerFixture : IDisposable
{
    public RedisContainer RedisContainer { get; } = new RedisBuilder()
        .WithCommand("--databases", "1024").Build();

    public RabbitMqContainer RabbitMqContainer { get; } = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();

    public MySqlContainer MySqlContainer { get; } = new MySqlBuilder()
        .WithUsername("root").WithPassword("123456")
        .WithEnvironment("TZ", "Asia/Shanghai")
        .WithDatabase("mysql").Build();

    public TestContainerFixture()
    {
        Task.WhenAll(RedisContainer.StartAsync(),
            RabbitMqContainer.StartAsync(),
            MySqlContainer.StartAsync()).Wait();
    }

    public void Dispose()
    {
        Task.WhenAll(RedisContainer.StopAsync(),
            RabbitMqContainer.StopAsync(),
            MySqlContainer.StopAsync()).Wait();
    }

    public async Task CreateVisualHostAsync(string visualHost)
    {
        await RabbitMqContainer.ExecAsync(new string[] { "rabbitmqctl", "add_vhost", visualHost });
        await RabbitMqContainer.ExecAsync(new string[]
            { "rabbitmqctl", "set_permissions", "-p", visualHost, "guest", ".*", ".*", ".*" });
    }
}