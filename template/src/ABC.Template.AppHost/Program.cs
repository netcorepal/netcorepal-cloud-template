var builder = DistributedApplication.CreateBuilder(args);

// Add Redis infrastructure
var redis = builder.AddRedis("Redis");

//#if (UseMySql)
// Add MySQL database infrastructure
var mysql = builder.AddMySql("Database")
    .WithPhpMyAdmin()
    .AddDatabase("MySql", "dev");
//#elif (UseSqlServer)
// Add SQL Server database infrastructure
var sqlserver = builder.AddSqlServer("Database")
    .AddDatabase("SqlServer", "dev");
//#elif (UsePostgreSQL)
// Add PostgreSQL database infrastructure
var postgres = builder.AddPostgres("Database")
    .WithPgAdmin()
    .AddDatabase("PostgreSQL", "dev");
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

// Add web project with infrastructure dependencies
builder.AddProject<Projects.ABC_Template_Web>("web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis)
//#if (UseMySql)
    .WithReference(mysql)
    .WaitFor(mysql)
//#elif (UseSqlServer)
    .WithReference(sqlserver)
    .WaitFor(sqlserver)
//#elif (UsePostgreSQL)
    .WithReference(postgres)
    .WaitFor(postgres)
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
