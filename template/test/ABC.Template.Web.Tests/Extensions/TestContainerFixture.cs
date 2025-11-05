//#if (UseMySql)
using Testcontainers.MySql;
//#elif (UseSqlServer)
using Testcontainers.MsSql;
//#elif (UsePostgreSQL)
using Testcontainers.PostgreSql;
//#endif
//#if (UseRabbitMQ)
using Testcontainers.RabbitMq;
//#elif (UseKafka)
using Testcontainers.Kafka;
//#elif (UseNATS)
using Testcontainers.Nats;
//#elif (UsePulsar)
using Testcontainers.Pulsar;
//#endif
using Testcontainers.Redis;

namespace ABC.Template.Web.Tests.Extensions;

public class TestContainerFixture : IAsyncLifetime
{
    public RedisContainer RedisContainer { get; } = new RedisBuilder()
        .WithCommand("--databases", "1024").Build();

//#if (UseRabbitMQ)
    public RabbitMqContainer RabbitMqContainer { get; } = new RabbitMqBuilder()
        .WithUsername("guest").WithPassword("guest").Build();
//#elif (UseKafka)
    public KafkaContainer KafkaContainer { get; } = new KafkaBuilder().Build();
//#elif (UseNATS)
    public NatsContainer NatsContainer { get; } = new NatsBuilder().Build();
//#elif (UsePulsar)
    public PulsarContainer PulsarContainer { get; } = new PulsarBuilder().Build();
//#endif

//#if (UseMySql)
    public MySqlContainer DatabaseContainer { get; } = new MySqlBuilder()
        .WithUsername("root").WithPassword("123456")
        .WithEnvironment("TZ", "Asia/Shanghai").Build();
//#elif (UseSqlServer)
    public MsSqlContainer DatabaseContainer { get; } = new MsSqlBuilder()
        .WithPassword("Test123456!")
        .WithEnvironment("TZ", "Asia/Shanghai").Build();
//#elif (UsePostgreSQL)
    public PostgreSqlContainer DatabaseContainer { get; } = new PostgreSqlBuilder()
        .WithUsername("postgres").WithPassword("123456")
        .WithEnvironment("TZ", "Asia/Shanghai")
        .WithDatabase("postgres").Build();
//#endif
//#if (UseSqlite)
    // SQLite is file-based and doesn't require a test container
//#endif

    public async Task InitializeAsync()
    {
        var tasks = new List<Task> { RedisContainer.StartAsync() };
//#if (UseRabbitMQ)
        tasks.Add(RabbitMqContainer.StartAsync());
//#elif (UseKafka)
        tasks.Add(KafkaContainer.StartAsync());
//#elif (UseNATS)
        tasks.Add(NatsContainer.StartAsync());
//#elif (UsePulsar)
        tasks.Add(PulsarContainer.StartAsync());
//#endif
//#if (!UseSqlite)
        tasks.Add(DatabaseContainer.StartAsync());
//#endif
        await Task.WhenAll(tasks);
    }

    public async Task DisposeAsync()
    {
        var tasks = new List<Task> { RedisContainer.StopAsync() };
//#if (UseRabbitMQ)
        tasks.Add(RabbitMqContainer.StopAsync());
//#elif (UseKafka)
        tasks.Add(KafkaContainer.StopAsync());
//#elif (UseNATS)
        tasks.Add(NatsContainer.StopAsync());
//#elif (UsePulsar)
        tasks.Add(PulsarContainer.StopAsync());
//#endif
//#if (!UseSqlite)
        tasks.Add(DatabaseContainer.StopAsync());
//#endif
        await Task.WhenAll(tasks);
    }

//#if (UseRabbitMQ)
    public async Task CreateVisualHostAsync(string visualHost)
    {
        await RabbitMqContainer.ExecAsync(new string[] { "rabbitmqctl", "add_vhost", visualHost });
        await RabbitMqContainer.ExecAsync(new string[]
            { "rabbitmqctl", "set_permissions", "-p", visualHost, "guest", ".*", ".*", ".*" });
    }
//#endif
}