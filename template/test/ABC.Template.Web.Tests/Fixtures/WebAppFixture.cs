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
//#endif
using Testcontainers.Redis;
using Microsoft.AspNetCore.Hosting;
using ABC.Template.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ABC.Template.Web.Tests.Fixtures;

public class WebAppFixture : AppFixture<Program>
{
    private RedisContainer _redisContainer = null!;
//#if (UseRabbitMQ)
    private RabbitMqContainer _rabbitMqContainer = null!;
//#elif (UseKafka)
    private KafkaContainer _kafkaContainer = null!;
//#elif (UseNATS)
    private NatsContainer _natsContainer = null!;
//#endif
//#if (UseMySql)
    private MySqlContainer _databaseContainer = null!;
//#elif (UseSqlServer)
    private MsSqlContainer _databaseContainer = null!;
//#elif (UsePostgreSQL)
    private PostgreSqlContainer _databaseContainer = null!;
//#endif

    protected override async ValueTask PreSetupAsync()
    {
        _redisContainer = new RedisBuilder()
            .WithCommand("--databases", "1024").Build();
//#if (UseRabbitMQ)
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithUsername("guest").WithPassword("guest").Build();
//#elif (UseKafka)
        _kafkaContainer = new KafkaBuilder().Build();
//#elif (UseNATS)
        _natsContainer = new NatsBuilder().Build();
//#endif
//#if (UseMySql)
        _databaseContainer = new MySqlBuilder()
            .WithUsername("root").WithPassword("123456")
            .WithEnvironment("TZ", "Asia/Shanghai").Build();
//#elif (UseSqlServer)
        _databaseContainer = new MsSqlBuilder()
            .WithPassword("Test123456!")
            .WithEnvironment("TZ", "Asia/Shanghai").Build();
//#elif (UsePostgreSQL)
        _databaseContainer = new PostgreSqlBuilder()
            .WithUsername("postgres").WithPassword("123456")
            .WithEnvironment("TZ", "Asia/Shanghai")
            .WithDatabase("postgres").Build();
//#endif

        var tasks = new List<Task> { _redisContainer.StartAsync() };
//#if (UseRabbitMQ)
        tasks.Add(_rabbitMqContainer.StartAsync());
//#elif (UseKafka)
        tasks.Add(_kafkaContainer.StartAsync());
//#elif (UseNATS)
        tasks.Add(_natsContainer.StartAsync());
//#endif
//#if (!UseSqlite)
        tasks.Add(_databaseContainer.StartAsync());
//#endif
        await Task.WhenAll(tasks);

//#if (UseRabbitMQ)
        await CreateVisualHostAsync("/");
//#endif
//#if (UseAspire && !UseSqlite)
        await CreateDatabaseAsync(_databaseContainer.GetConnectionString());
//#endif
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
//#if (UseAspire)
        // When using Aspire, connection strings use resource names
        a.UseSetting("ConnectionStrings:redis",
            _redisContainer.GetConnectionString());
//#if (UseMySql)
        a.UseSetting("ConnectionStrings:MySql",
            _databaseContainer.GetConnectionString());
//#elif (UseSqlServer)
        a.UseSetting("ConnectionStrings:SqlServer",
            _databaseContainer.GetConnectionString());
//#elif (UsePostgreSQL)
        a.UseSetting("ConnectionStrings:PostgreSQL",
            _databaseContainer.GetConnectionString());
//#elif (UseSqlite)
        // SQLite uses in-memory database for testing with cache=shared to persist data between connections
        a.UseSetting("ConnectionStrings:Sqlite", "Data Source=:memory:?cache=shared");
//#endif
//#if (UseRabbitMQ)
        a.UseSetting("ConnectionStrings:rabbitmq",
            $"amqp://guest:guest@{_rabbitMqContainer.Hostname}:{_rabbitMqContainer.GetMappedPublicPort(5672)}/");
//#elif (UseKafka)
        a.UseSetting("ConnectionStrings:kafka", _kafkaContainer.GetBootstrapAddress());
//#elif (UseNATS)
        a.UseSetting("NATS:Servers", _natsContainer.GetConnectionString());
//#elif (UseRedisStreams)
        // RedisStreams uses the same redis connection string
//#endif
//#else
        a.UseSetting("ConnectionStrings:Redis",
            _redisContainer.GetConnectionString());
//#if (UseMySql)
        a.UseSetting("ConnectionStrings:MySql",
            _databaseContainer.GetConnectionString());
//#elif (UseSqlServer)
        a.UseSetting("ConnectionStrings:SqlServer",
            _databaseContainer.GetConnectionString());
//#elif (UsePostgreSQL)
        a.UseSetting("ConnectionStrings:PostgreSQL",
            _databaseContainer.GetConnectionString());
//#elif (UseSqlite)
        // SQLite uses in-memory database for testing with cache=shared to persist data between connections
        a.UseSetting("ConnectionStrings:Sqlite", "Data Source=:memory:?cache=shared");
//#endif
//#if (UseRabbitMQ)
        a.UseSetting("RabbitMQ:Port", _rabbitMqContainer.GetMappedPublicPort(5672).ToString());
        a.UseSetting("RabbitMQ:UserName", "guest");
        a.UseSetting("RabbitMQ:Password", "guest");
        a.UseSetting("RabbitMQ:VirtualHost", "/");
        a.UseSetting("RabbitMQ:HostName", _rabbitMqContainer.Hostname);
//#elif (UseKafka)
        a.UseSetting("Kafka:BootstrapServers", _kafkaContainer.GetBootstrapAddress());
//#elif (UseNATS)
        a.UseSetting("NATS:Servers", _natsContainer.GetConnectionString());
//#endif
//#endif
        a.UseEnvironment("Development");
    }

//#if (UseRabbitMQ)
    private async Task CreateVisualHostAsync(string visualHost)
    {
        await _rabbitMqContainer.ExecAsync(["rabbitmqctl", "add_vhost", visualHost]);
        await _rabbitMqContainer.ExecAsync(["rabbitmqctl", "set_permissions", "-p", visualHost, "guest", ".*", ".*", ".*"
        ]);
    }
//#endif

//#if(UseAspire && !UseSqlite)
    private static async Task CreateDatabaseAsync(string connectionString)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<WebAppFixture>());
        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
        {
//#if (UseMySql)
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 34)));
//#elif (UseSqlServer)
            options.UseSqlServer(connectionString);
//#elif (UsePostgreSQL)
            options.UseNpgsql(connectionString);
//#elif (UseSqlite)
            options.UseSqlite(connectionString);
//#endif
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        await using var serviceProvider = serviceCollection.BuildServiceProvider();
        await using var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
//#endif
}
