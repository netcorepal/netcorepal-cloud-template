//#if (UseAspire)
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.Hosting;

namespace ABC.Template.Web.Tests.Fixtures;

public class WebAppFixture : AppFixture<Program>
{
    private DistributedApplication? _app;

    protected override async ValueTask PreSetupAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        
        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    protected override void ConfigureApp(IWebHostBuilder a)
    {
        if (_app == null)
        {
            throw new InvalidOperationException("Distributed application not initialized");
        }

        // Get connection strings from Aspire resources
        var redisResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("Redis", StringComparison.OrdinalIgnoreCase));
        if (redisResource != null)
        {
            var redisConnectionString = redisResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                a.UseSetting("ConnectionStrings:Redis", redisConnectionString);
            }
        }

//#if (UseMySql)
        var dbResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("MySql", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("Database", StringComparison.OrdinalIgnoreCase));
        if (dbResource != null)
        {
            var dbConnectionString = dbResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                a.UseSetting("ConnectionStrings:MySql", dbConnectionString);
            }
        }
//#elif (UseSqlServer)
        var dbResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("Database", StringComparison.OrdinalIgnoreCase));
        if (dbResource != null)
        {
            var dbConnectionString = dbResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                a.UseSetting("ConnectionStrings:SqlServer", dbConnectionString);
            }
        }
//#elif (UsePostgreSQL)
        var dbResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("Database", StringComparison.OrdinalIgnoreCase));
        if (dbResource != null)
        {
            var dbConnectionString = dbResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                a.UseSetting("ConnectionStrings:PostgreSQL", dbConnectionString);
            }
        }
//#elif (UseGaussDB)
        var dbResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("GaussDB", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("Database", StringComparison.OrdinalIgnoreCase));
        if (dbResource != null)
        {
            var dbConnectionString = dbResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                a.UseSetting("ConnectionStrings:GaussDB", dbConnectionString);
            }
        }
//#elif (UseDMDB)
        var dbResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("DMDB", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("Database", StringComparison.OrdinalIgnoreCase));
        if (dbResource != null)
        {
            var dbConnectionString = dbResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                a.UseSetting("ConnectionStrings:DMDB", dbConnectionString);
            }
        }
//#elif (UseSqlite)
        // SQLite uses in-memory database for testing
        a.UseSetting("ConnectionStrings:Sqlite", "Data Source=:memory:?cache=shared");
//#endif

//#if (UseRabbitMQ)
        var mqResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("rabbitmq", StringComparison.OrdinalIgnoreCase));
        if (mqResource != null)
        {
            var mqConnectionString = mqResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(mqConnectionString))
            {
                a.UseSetting("ConnectionStrings:rabbitmq", mqConnectionString);
            }
        }
//#elif (UseKafka)
        var mqResource = _app.Resources.OfType<IResourceWithConnectionString>()
            .FirstOrDefault(r => r.Name.Equals("kafka", StringComparison.OrdinalIgnoreCase));
        if (mqResource != null)
        {
            var mqConnectionString = mqResource.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(mqConnectionString))
            {
                a.UseSetting("ConnectionStrings:kafka", mqConnectionString);
            }
        }
//#endif

        a.UseEnvironment("Development");
    }

    public override async ValueTask DisposeAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
//#else
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
