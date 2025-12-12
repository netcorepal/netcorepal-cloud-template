var builder = DistributedApplication.CreateBuilder(args);

//Enable Docker publisher
builder.AddDockerComposeEnvironment("docker-env")
    .WithDashboard(dashboard =>
    {
        dashboard.WithHostPort(8080)
            .WithForwardedHeaders(enabled: true);
    });

// Add Redis infrastructure
var redis = builder.AddRedis("Redis");

//#if (!UseSqlite)
var databasePassword = builder.AddParameter("database-password", secret: true);
//#endif
//#if (UseMySql)
// Add MySQL database infrastructure
var mysql = builder.AddMySql("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    .WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPhpMyAdmin();

var mysqlDb = mysql.AddDatabase("MySql", "dev");
//#elif (UseSqlServer)
// Add SQL Server database infrastructure
var sqlserver = builder.AddSqlServer("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    .WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    .WithLifetime(ContainerLifetime.Persistent);

var sqlserverDb = sqlserver.AddDatabase("SqlServer", "dev");
//#elif (UsePostgreSQL)
// Add PostgreSQL database infrastructure
var postgres = builder.AddPostgres("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    .WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var postgresDb = postgres.AddDatabase("PostgreSQL", "dev");
//#elif (UseGaussDB)
// Add GaussDB database infrastructure using OpenGauss container (GaussDB compatible)
var gaussdb = builder.AddContainer("Database", "opengauss/opengauss", "latest")
    .WithEnvironment("GS_PASSWORD", databasePassword)
    .WithEndpoint(5432, 5432, "tcp", "gaussdb")
    .WithBindMount("gaussdb-data", "/var/lib/opengauss")
    .WithLifetime(ContainerLifetime.Persistent);

var gaussdbDb = builder.AddConnectionString("GaussDB", 
    $"Host={{Database.bindings.gaussdb.host}};Port={{Database.bindings.gaussdb.port}};Database=dev;Username=gaussdb;Password={{Database.env.GS_PASSWORD}}");
//#elif (UseKingbaseES)
// Add KingbaseES database infrastructure using KingbaseES container
var kingbasees = builder.AddContainer("Database", "apecloud/kingbase", "v008r006c009b0014-unit")
    .WithEnvironment("ENABLE_CI", "yes")
    .WithEnvironment("DB_USER", "system")
    .WithEnvironment("DB_PASSWORD", databasePassword)
    .WithEnvironment("DB_MODE", "oracle")
    .WithEndpoint(54321, 54321, "tcp", "kingbasees")
    .WithBindMount("kingbasees-data", "/home/kingbase/userdata")
    .WithLifetime(ContainerLifetime.Persistent);

var kingbaseesDb = builder.AddConnectionString("KingbaseES",
    $"Host={{Database.bindings.kingbasees.host}};Port={{Database.bindings.kingbasees.port}};Database=TEST;Username={{Database.env.DB_USER}};Password={{Database.env.DB_PASSWORD}}");
//#endif
//#if (UseSqlite)
// SQLite is a file-based database and doesn't require container infrastructure
//#endif

//#if (UseRabbitMQ)
// Add RabbitMQ message queue infrastructure
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();
//#elif (UseKafka)
// Add Kafka message queue infrastructure
var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();
//#endif

//#if (!UseSqlite)
var migrationService = builder.AddProject<Projects.ABC_Template_MigrationService>("migration")
//#if (UseMySql)
    .WithReference(mysqlDb)
    .WaitFor(mysqlDb);
//#elif (UseSqlServer)
    .WithReference(sqlserverDb)
    .WaitFor(sqlserverDb);
//#elif (UsePostgreSQL)
    .WithReference(postgresDb)
    .WaitFor(postgresDb);
//#elif (UseGaussDB)
    .WithReference(gaussdbDb)
    .WaitFor(gaussdbDb);
//#elif (UseKingbaseES)
    .WithReference(kingbaseesDb)
    .WaitFor(kingbaseesDb);
//#endif
//#endif

// Add web project with infrastructure dependencies
builder.AddProject<Projects.ABC_Template_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis)
//#if (UseMySql)
    .WithReference(mysqlDb)
    .WaitFor(mysqlDb)
//#elif (UseSqlServer)
    .WithReference(sqlserverDb)
    .WaitFor(sqlserverDb)
//#elif (UsePostgreSQL)
    .WithReference(postgresDb)
    .WaitFor(postgresDb)
//#elif (UseGaussDB)
    .WithReference(gaussdbDb)
    .WaitFor(gaussdbDb)
//#elif (UseKingbaseES)
    .WithReference(kingbaseesDb)
    .WaitFor(kingbaseesDb)
//#endif
//#if (UseSqlite)
    // SQLite doesn't need infrastructure reference
//#endif
//#if (UseRabbitMQ)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
//#elif (UseKafka)
    .WithReference(kafka)
    .WaitFor(kafka)
//#endif
//#if (!UseSqlite)
    .WaitForCompletion(migrationService)
//#endif
    ;

await builder.Build().RunAsync();