var builder = DistributedApplication.CreateBuilder(args);

// Add Redis infrastructure
var redis = builder.AddRedis("redis");

//#if (UseMySql)
// Add MySQL database infrastructure
var mysql = builder.AddMySql("mysql")
    .WithPhpMyAdmin()
    .AddDatabase("demo");
//#elif (UseSqlServer)
// Add SQL Server database infrastructure
var sqlserver = builder.AddSqlServer("sqlserver")
    .AddDatabase("demo");
//#elif (UsePostgreSQL)
// Add PostgreSQL database infrastructure
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("demo");
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
//#if (UseMySql)
    .WithReference(mysql)
//#elif (UseSqlServer)
    .WithReference(sqlserver)
//#elif (UsePostgreSQL)
    .WithReference(postgres)
//#endif
//#if (UseRabbitMQ)
    .WithReference(rabbitmq)
//#elif (UseKafka)
    .WithReference(kafka)
//#endif
    ;

await builder.Build().RunAsync();
