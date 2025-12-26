<!--#if (UseAspire)-->
var builder = DistributedApplication.CreateBuilder(args);

// Add Redis infrastructure
var redis = builder.AddRedis("Redis");

//#if (!UseSqlite)
var databasePassword = builder.AddParameter("database-password", secret: true);
//#endif
//#if (UseMySql)
// Add MySQL database infrastructure
var mysql = builder.AddMySql("Database", password: databasePassword)
    .WithDataVolume(isReadOnly: false);

var mysqlDb = mysql.AddDatabase("MySql", "test");
//#elif (UseSqlServer)
// Add SQL Server database infrastructure
var sqlserver = builder.AddSqlServer("Database", password: databasePassword)
    .WithDataVolume(isReadOnly: false);

var sqlserverDb = sqlserver.AddDatabase("SqlServer", "test");
//#elif (UsePostgreSQL)
// Add PostgreSQL database infrastructure
var postgres = builder.AddPostgres("Database", password: databasePassword)
    .WithDataVolume(isReadOnly: false);

var postgresDb = postgres.AddDatabase("PostgreSQL", "test");
//#elif (UseGaussDB)
// Add GaussDB database infrastructure using OpenGauss container (GaussDB compatible)
var gaussdb = builder.AddOpenGauss("Database", password: databasePassword)
    .WithDataVolume(isReadOnly: false);

var gaussdbDb = gaussdb.AddDatabase("GaussDB", "test");
//#elif (UseDMDB)
// Add DMDB database infrastructure using DMDB container
var dmdb = builder.AddDmdb("Database")
    .WithDataVolume(isReadOnly: false);

var dmdbDb = dmdb.AddDatabase("DMDB");
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
//#endif
    ;

await builder.Build().RunAsync();
<!--#endif-->
