var builder = DistributedApplication.CreateBuilder(args);

//Enable Docker publisher
// builder.AddDockerComposeEnvironment("docker-env")
//     .WithDashboard(dashboard =>
//     {
//         dashboard.WithHostPort(8080)
//             .WithForwardedHeaders(enabled: true);
//     });

// Add Redis infrastructure
var redis = builder.AddRedis("Redis");

//#if (!UseSqlite)
var databasePassword = builder.AddParameter("database-password", value:"1234@Dev", secret: true);
//#endif
//#if (UseMySql)
// Add MySQL database infrastructure
var mysql = builder.AddMySql("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithPhpMyAdmin();

var mysqlDb = mysql.AddDatabase("MySql", "dev");
//#elif (UseSqlServer)
// Add SQL Server database infrastructure
var sqlserver = builder.AddSqlServer("Database", password: databasePassword);
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent);

var sqlserverDb = sqlserver.AddDatabase("SqlServer", "dev");
//#elif (UsePostgreSQL)
// Add PostgreSQL database infrastructure
var postgres = builder.AddPostgres("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var postgresDb = postgres.AddDatabase("PostgreSQL", "dev");
//#elif (UseGaussDB)
// Add GaussDB database infrastructure using OpenGauss container (GaussDB compatible)
var gaussdb = builder.AddOpenGauss("Database", password: databasePassword)
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();
var gaussdbDb = gaussdb.AddDatabase("GaussDB", "dev");
//#elif (UseDMDB)
// Add DMDB database infrastructure using DMDB container
var dmdb = builder.AddDmdb("Database");
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent);

var dmdbDb = dmdb.AddDatabase("DMDB");
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
//#elif (UseNATS)
// Add NATS message queue infrastructure
var nats = builder.AddNats("Nats");
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
//#elif (UseDMDB)
    .WithReference(dmdbDb)
    .WaitFor(dmdbDb)
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
//#elif (UseNATS)
    .WithReference(nats)
    .WaitFor(nats)
//#endif
    ;

await builder.Build().RunAsync();