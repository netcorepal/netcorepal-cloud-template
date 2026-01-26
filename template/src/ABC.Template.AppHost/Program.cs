var builder = DistributedApplication.CreateBuilder(args);

//Enable Docker publisher
// builder.AddDockerComposeEnvironment("docker-env")
//     .WithDashboard(dashboard =>
//     {
//         dashboard.WithHostPort(8080)
//             .WithForwardedHeaders(enabled: true);
//     });

// Add Redis infrastructure
var redis = builder.AddRedis("Redis").WithRedisInsight();

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
var dmdb = builder.AddDmdb("Database", userName: null, databasePassword, databasePassword);
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent);

var dmdbDb = dmdb.AddDatabase("DMDB");
//#elif (UseMongoDB)
// Add MongoDB database infrastructure
var mongodb = builder.AddMongoDB("Database", password: databasePassword)
    .WithReplicaSet()
    // Configure the container to store data in a volume so that it persists across instances.
    //.WithDataVolume(isReadOnly: false)
    // Keep the container running between app host sessions.
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithDbGate();

var mongoDatabase = mongodb.AddDatabase("s1");
var mongoReplicaSet = builder
            .AddMongoReplicaSet("MongoDB", mongoDatabase.Resource);
//#endif
//#if (UseSqlite)
// SQLite is a file-based database and doesn't require container infrastructure
//#endif

//#if (UseRabbitMQ)
// Add RabbitMQ message queue infrastructure
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", value: "guest", secret: true);
var rabbitmq = builder.AddRabbitMQ("rabbitmq", password: rabbitmqPassword)
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
var web = builder.AddProject<Projects.ABC_Template_Web>("web")
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
//#elif (UseMongoDB)
    .WithReference(mongoDatabase)
    .WithReference(mongoReplicaSet)
    .WaitFor(mongoDatabase)
    .WaitFor(mongoReplicaSet)
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

//#if (UseAdmin)
// Add frontend project
var frontend = builder.AddJavaScriptApp("frontend", "../frontend")
    .WithPnpm()
    .WithDeveloperCertificateTrust(true)
    .WithHttpEndpoint(port: 5666, env: "VITE_PORT", name: "http", isProxied: false)
    .WithExternalHttpEndpoints()
    .WithEnvironment("VITE_API_BASE", web.GetEndpoint("http"))
    .WithReference(web)
    .WaitFor(web);
//#endif

await builder.Build().RunAsync();
