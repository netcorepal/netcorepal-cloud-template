//#if (UseAspire)
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.Hosting;
using System; // 添加 System 命名空间
using System.Linq; // 添加 System.Linq 命名空间


namespace ABC.Template.Web.Tests.Fixtures;

public class WebAppFixture : AppFixture<Program>
{
    private IDistributedApplicationTestingBuilder? _appHost;

    private DistributedApplication? _app;

    protected override async ValueTask PreSetupAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_TestAppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        _appHost = appHost;
        _app = await appHost.BuildAsync();
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        await _app.StartAsync(cts.Token);
//#if (UseMySql)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("MySql", cts.Token);
//#elif (UseSqlServer)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("SqlServer", cts.Token);
//#elif (UsePostgreSQL)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("PostgreSQL", cts.Token);
//#elif (UseGaussDB)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("GaussDB", cts.Token);
//#elif (UseDMDB)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("DMDB", cts.Token);
//#endif
//#if (UseRabbitMQ)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq", cts.Token);
//#elif (UseKafka)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("kafka", cts.Token);
//#elif (UseNATS)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("nats", cts.Token);
//#elif (UseAzureServiceBus || UseAmazonSQS || UsePulsar)
        // Azure Service Bus, Amazon SQS, and Pulsar are not available in local testing environment
        // Use Redis as fallback for testing
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("Redis", cts.Token);
//#elif (UseRedisStreams)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("Redis", cts.Token);
//#endif
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("Redis", cts.Token);
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        if (_app == null)
        {
            throw new InvalidOperationException("Distributed application not initialized");
        }

        // Get connection strings from Aspire resources
        SetConnectionString(a, "Redis", "ConnectionStrings:Redis");

//#if (UseMySql)
        SetConnectionString(a, "MySql", "ConnectionStrings:MySql");
//#elif (UseSqlServer)
        SetConnectionString(a, "SqlServer", "ConnectionStrings:SqlServer");
//#elif (UsePostgreSQL)
        SetConnectionString(a, "PostgreSQL", "ConnectionStrings:PostgreSQL");
//#elif (UseGaussDB)
        SetConnectionString(a, "GaussDB", "ConnectionStrings:GaussDB");
//#elif (UseDMDB)
        SetConnectionString(a, "DMDB", "ConnectionStrings:DMDB");
//#elif (UseSqlite)
        // SQLite uses in-memory database for testing
        a.UseSetting("ConnectionStrings:Sqlite", "Data Source=:memory:?cache=shared");
//#endif

//#if (UseRabbitMQ)
        SetConnectionString(a, "rabbitmq", "ConnectionStrings:rabbitmq");
//#elif (UseKafka)
        SetConnectionString(a, "kafka", "ConnectionStrings:kafka");
//#elif (UseNATS)
        SetConnectionString(a, "nats", "ConnectionStrings:nats");
//#elif (UseAzureServiceBus || UseAmazonSQS || UsePulsar)
        // Azure Service Bus, Amazon SQS, and Pulsar are not available in local testing environment
        // Use Redis as fallback for testing
        SetConnectionString(a, "Redis", "ConnectionStrings:redis-fallback");
//#elif (UseRedisStreams)
        SetConnectionString(a, "Redis", "ConnectionStrings:redis");
//#endif

        a.UseEnvironment("Development");
    }

    private void SetConnectionString(IWebHostBuilder builder, string resourceName, string configKey, params string[] alternativeNames)
    {
        var resource =  (IResourceWithConnectionString)_appHost!.Resources.First(r => r.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase) ||
                                alternativeNames.Any(n => r.Name.Equals(n, StringComparison.OrdinalIgnoreCase)));
        if (resource != null)
        {
            // Note: Using GetAwaiter().GetResult() is necessary here because ConfigureApp is synchronous
            // This is safe during test fixture initialization
            var connectionString = resource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.UseSetting(configKey, connectionString);
            }
        }
    }

    protected override async ValueTask TearDownAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        await base.TearDownAsync();
    }
}
//#else
//#if (UseMySql)
using Testcontainers.MySql;
//#elif (UseSqlServer)
using Testcontainers.MsSql;
//#elif (UsePostgreSQL)
using Testcontainers.PostgreSql;
//#elif (UseGaussDB)
using Testcontainers.OpenGauss;
//#elif (UseDMDB)
using Testcontainers.DMDB;
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
//#elif (UseGaussDB)
    private OpenGaussContainer _databaseContainer = null!;
//#elif (UseDMDB)
    private DmdbContainer _databaseContainer = null!;
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
//#elif (UseGaussDB)
        _databaseContainer = new OpenGaussBuilder()
            .Build();
//#elif (UseDMDB)
        _databaseContainer = new DmdbBuilder()
            .Build();
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
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
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
//#elif (UseGaussDB)
        a.UseSetting("ConnectionStrings:GaussDB",
            _databaseContainer.GetConnectionString());
//#elif (UseDMDB)
        a.UseSetting("ConnectionStrings:DMDB",
            _databaseContainer.GetConnectionString() + ";schema=testdb;");
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
}
//#endif