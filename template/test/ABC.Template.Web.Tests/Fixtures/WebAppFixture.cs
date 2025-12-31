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
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);
    private IDistributedApplicationTestingBuilder? _appHost;

    private DistributedApplication? _app;

    protected override async ValueTask PreSetupAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_TestAppHost>();
        // Add Redis infrastructure
        var redis = builder.AddRedis("Redis");

        //#if (!UseSqlite)
        var databasePassword = builder.AddParameter("database-password", value: "123456@Abc", secret: true);
        //#endif
        //#if (UseMySql)
        // Add MySQL database infrastructure
        var mysql = builder.AddMySql("Database", password: databasePassword);

        var database =mysql.AddDatabase("MySql", "test");
        //#elif (UseSqlServer)
        // Add SQL Server database infrastructure
        var sqlserver = builder.AddSqlServer("Database", password: databasePassword);

        var database = sqlserver.AddDatabase("SqlServer", "test");
        //#elif (UsePostgreSQL)
        // Add PostgreSQL database infrastructure
        var postgres = builder.AddPostgres("Database", password: databasePassword);

        var database = postgres.AddDatabase("PostgreSQL", "test");
        //#elif (UseGaussDB)
        // Add GaussDB database infrastructure using OpenGauss container (GaussDB compatible)
        var gaussdb = builder.AddOpenGauss("Database", password: databasePassword);

        var database = gaussdb.AddDatabase("GaussDB", "test");
        //#elif (UseDMDB)
        // Add DMDB database infrastructure using DMDB container
        var dmdb = builder.AddDmdb("Database", userName: null, databasePassword, databasePassword);

        var database = dmdb.AddDatabase("DMDB");
        //#elif (UseMongoDB)
        // Add MongoDB database infrastructure
        var mongo = builder.AddMongoDB("Database")
                   .WithMongoExpress();

        var mongodb = mongo.AddDatabase("MongoDB");
        //#endif
        //#if (UseSqlite)
        // SQLite is a file-based database and doesn't require container infrastructure
        //#endif

        //#if (UseRabbitMQ)
        // Add RabbitMQ message queue infrastructure
        var rabbitmq = builder.AddRabbitMQ("rabbitmq");
        //#elif (UseKafka)
        // Add Kafka message queue infrastructure
        var kafka = builder.AddKafka("kafka");
        //#elif (UseNATS)
        // Add NATS message queue infrastructure
        var nats = builder.AddNats("nats");
        //#endif
        
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        _appHost = builder;
        _app = await builder.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#if (UseMySql)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(database.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseSqlServer)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(database.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UsePostgreSQL)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(database.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseGaussDB)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(database.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseDMDB)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(database.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseMongoDB)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(mongodb.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#endif
//#if (UseRabbitMQ)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(rabbitmq.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseKafka)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(kafka.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseNATS)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(nats.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseAzureServiceBus || UseAmazonSQS || UsePulsar)
        // Azure Service Bus, Amazon SQS, and Pulsar are not available in local testing environment
        // Use Redis as fallback for testing
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(redis.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#elif (UseRedisStreams)
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(redis.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
//#endif
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(redis.Resource.Name, cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
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
//#elif (UseMongoDB)
        SetConnectionString(a, "MongoDB", "ConnectionStrings:MongoDB");
//#elif (UseSqlite)
        // SQLite uses in-memory database for testing
        var fileName = $"testdb{Guid.NewGuid():N}";
        a.UseSetting("ConnectionStrings:Sqlite", $"Data Source=file:{fileName}?mode=memory&cache=shared");
//#endif

//#if (UseRabbitMQ)
        SetConnectionString(a, "rabbitmq", "ConnectionStrings:rabbitmq");
//#elif (UseKafka)
        SetConnectionString(a, "kafka", "ConnectionStrings:kafka");
//#elif (UseNATS)
        SetConnectionString(a, "Nats", "ConnectionStrings:Nats");
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
//#elif (UseMongoDB)
using Testcontainers.MongoDb;
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
//#elif (UseMongoDB)
    private MongoDbContainer _databaseContainer = null!;
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
//#elif (UseMongoDB)
        _databaseContainer = new MongoDbBuilder()
        .WithImage("mongo:8.0")
        .WithReplicaSet("rs0")
        .WithUsername("admin")
        .WithPassword("guest")
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
            _databaseContainer.GetConnectionString());
//#elif (UseMongoDB)
        a.UseSetting("ConnectionStrings:MongoDB",
            _databaseContainer.GetConnectionString());
//#elif (UseSqlite)
        // SQLite uses in-memory database for testing with cache=shared to persist data between connections
        var fileName = $"testdb{Guid.NewGuid():N}";
        a.UseSetting("ConnectionStrings:Sqlite", $"Data Source=file:{fileName}?mode=memory&cache=shared");
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
        a.UseSetting("ConnectionStrings:Nats", _natsContainer.GetConnectionString());
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