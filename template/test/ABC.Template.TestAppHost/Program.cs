var builder = DistributedApplication.CreateBuilder(args);

// Add Redis infrastructure
var redis = builder.AddRedis("Redis");

//#if (!UseSqlite)
var databasePassword = builder.AddParameter("database-password", value: "123456@Abc", secret: true);
//#endif
//#if (UseMySql)
// Add MySQL database infrastructure
var mysql = builder.AddMySql("Database", password: databasePassword);

var mysqlDb = mysql.AddDatabase("MySql", "test");
//#elif (UseSqlServer)
// Add SQL Server database infrastructure
var sqlserver = builder.AddSqlServer("Database", password: databasePassword);

var sqlserverDb = sqlserver.AddDatabase("SqlServer", "test");
//#elif (UsePostgreSQL)
// Add PostgreSQL database infrastructure
var postgres = builder.AddPostgres("Database", password: databasePassword);

var postgresDb = postgres.AddDatabase("PostgreSQL", "test");
//#elif (UseGaussDB)
// Add GaussDB database infrastructure using OpenGauss container (GaussDB compatible)
var gaussdb = builder.AddOpenGauss("Database", password: databasePassword);

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

await builder.Build().RunAsync();
