using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using FluentValidation.AspNetCore;
//#if (UseDemoCode)
using ABC.Template.Web.Application.IntegrationEventHandlers;
//#endif
using ABC.Template.Web.Clients;
using ABC.Template.Web.Extensions;
//#if (UseAdmin)
using ABC.Template.Web.Utils;
//#endif
using FastEndpoints;
using Serilog;
using Serilog.Formatting.Json;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using NetCorePal.Extensions.CodeAnalysis;
//#if (UseMongoDB)
using NetCorePal.Extensions.DistributedTransactions.CAP.MongoDB;
//#endif

<!--#if (UseAspire)-->
// Create a minimal logger for startup
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
<!--#else-->
Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
<!--#endif-->
try
{
    var builder = WebApplication.CreateBuilder(args);
<!--#if (UseAspire)-->
    
    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();
<!--#else-->
    builder.Host.UseSerilog();
<!--#endif-->

    #region SignalR

    builder.Services.AddHealthChecks();
    builder.Services.AddMvc()
        .AddNewtonsoftJson(options => { options.SerializerSettings.AddNetCorePalJsonConverters(); });
    builder.Services.AddSignalR();

    #endregion

    #region Prometheus监控

    builder.Services.AddHealthChecks().ForwardToPrometheus();
    builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();

    #endregion

    // Add services to the container.

    #region 身份认证

<!--#if (UseAspire)-->
    // When using Aspire, Redis connection is managed by Aspire and injected automatically
    builder.AddRedisClient("Redis");
<!--#else-->
    var redis = await ConnectionMultiplexer.ConnectAsync(builder.Configuration.GetConnectionString("Redis")!);
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => redis);
<!--#endif-->
    
    // DataProtection - use custom extension that resolves IConnectionMultiplexer from DI
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis("DataProtection-Keys");

    builder.Services.AddAuthentication().AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidAudience = "netcorepal";
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidIssuer = "netcorepal";
        options.TokenValidationParameters.ValidateIssuer = true;
    });
    builder.Services.AddNetCorePalJwt().AddRedisStore();

    #endregion

//#if (UseAdmin)
    #region CORS

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:5666", "http://localhost:5173", "http://localhost:3000" };
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    #endregion
//#endif

    #region Controller

    builder.Services.AddControllers().AddNetCorePalSystemTextJson();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.AddEntityIdSchemaMap()); //强类型id swagger schema 映射

    #endregion

    #region FastEndpoints

    builder.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true);
    builder.Services.Configure<JsonOptions>(o =>
        o.SerializerOptions.AddNetCorePalJsonConverters());

    #endregion

    #region 模型验证器

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    builder.Services.AddKnownExceptionErrorModelInterceptor();

    #endregion

//#if (UseAdmin)
    #region Query
    // 自动注册所有实现 IQuery 接口的查询类
    builder.Services.AddQueries(Assembly.GetExecutingAssembly());
    #endregion
//#endif

    #region 基础设施

    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);

<!--#if (UseAspire)-->
    // When using Aspire, database connection is managed by Aspire
    // Use AddDbContext instead of AddMySqlDbContext/AddSqlServerDbContext/AddNpgsqlDbContext
    // to avoid ExecutionStrategy issues with user-initiated transactions
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
//#if (UseMySql)
        options.UseMySql(builder.Configuration.GetConnectionString("MySql"),
            new MySqlServerVersion(new Version(8, 0, 34)));
//#elif (UseSqlServer)
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
//#elif (UsePostgreSQL)
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
//#elif (UseSqlite)
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
//#elif (UseGaussDB)
        options.UseGaussDB(builder.Configuration.GetConnectionString("GaussDB"));
//#elif (UseDMDB)
        options.UseDm(builder.Configuration.GetConnectionString("DMDB")!);
//#elif (UseMongoDB)
        options.UseMongoDB(builder.Configuration.GetConnectionString("MongoDB")!, "ABC_Template");
//#endif
        // 仅在开发环境启用敏感数据日志，防止生产环境泄露敏感信息
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
        options.EnableDetailedErrors();
    });
<!--#else-->
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
//#if (UseMySql)
        options.UseMySql(builder.Configuration.GetConnectionString("MySql"),
            new MySqlServerVersion(new Version(8, 0, 34)));
//#elif (UseSqlServer)
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
//#elif (UsePostgreSQL)
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
//#elif (UseSqlite)
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
//#elif (UseGaussDB)
        options.UseGaussDB(builder.Configuration.GetConnectionString("GaussDB"));
//#elif (UseDMDB)
        options.UseDm(builder.Configuration.GetConnectionString("DMDB")!);
//#elif (UseMongoDB)
        options.UseMongoDB(builder.Configuration.GetConnectionString("MongoDB")!, "ABC_Template");
//#endif
        // 仅在开发环境启用敏感数据日志，防止生产环境泄露敏感信息
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
        options.EnableDetailedErrors();
    });
<!--#endif-->
    builder.Services.AddUnitOfWork<ApplicationDbContext>();
<!--#if (UseAspire)-->
    // Redis locks use the Aspire-managed Redis connection
    builder.Services.AddRedisLocks();
<!--#else-->
    builder.Services.AddRedisLocks();
<!--#endif-->
    builder.Services.AddContext().AddEnvContext().AddCapContextProcessor();
    builder.Services.AddNetCorePalServiceDiscoveryClient();
    builder.Services.AddIntegrationEvents(typeof(Program))
        .UseCap<ApplicationDbContext>(b =>
        {
            b.RegisterServicesFromAssemblies(typeof(Program));
            b.AddContextIntegrationFilters();
        });


    builder.Services.AddCap(x =>
    {
//#if (UseMongoDB)
        x.UseMongoDBNetCorePalStorage<ApplicationDbContext>();
//#else
        x.UseNetCorePalStorage<ApplicationDbContext>();
//#endif
        x.JsonSerializerOptions.AddNetCorePalJsonConverters();
        x.ConsumerThreadCount = Environment.ProcessorCount;
<!--#if (UseAspire)-->
//#if (UseRabbitMQ)
        // When using Aspire, RabbitMQ connection is managed by Aspire
        x.UseRabbitMQ(p =>
        {
            var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Parse Aspire-provided connection string
                var uri = new Uri(connectionString);
                p.HostName = uri.Host;
                p.Port = uri.Port;
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var userInfo = uri.UserInfo.Split(':');
                    p.UserName = userInfo[0];
                    if (userInfo.Length > 1)
                    {
                        p.Password = userInfo[1];
                    }
                }
                if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
                {
                    p.VirtualHost = uri.AbsolutePath.TrimStart('/');
                }
            }
            else
            {
                builder.Configuration.GetSection("RabbitMQ").Bind(p);
            }
        });
//#elif (UseKafka)
        // When using Aspire, Kafka connection is managed by Aspire
        x.UseKafka(p =>
        {
            var connectionString = builder.Configuration.GetConnectionString("kafka");
            if (!string.IsNullOrEmpty(connectionString))
            {
                p.Servers = connectionString;
            }
            else
            {
                builder.Configuration.GetSection("Kafka").Bind(p);
            }
        });
//#elif (UseRedisStreams)
        // When using Aspire, Redis connection is managed by Aspire
        x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
//#elif (UseAzureServiceBus)
        // In development, use RedisStreams as fallback for testing
        if (builder.Environment.IsDevelopment())
        {
            x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
        }
        else
        {
            x.UseAzureServiceBus(p => builder.Configuration.GetSection("AzureServiceBus").Bind(p));
        }
//#elif (UseAmazonSQS)
        // In development, use RedisStreams as fallback for testing
        if (builder.Environment.IsDevelopment())
        {
            x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
        }
        else
        {
            x.UseAmazonSQS(p => builder.Configuration.GetSection("AmazonSQS").Bind(p));
        }
//#elif (UseNATS)
        x.UseNATS(builder.Configuration.GetConnectionString("Nats")!);
//#elif (UsePulsar)
        // In development, use RedisStreams as fallback for testing
        if (builder.Environment.IsDevelopment())
        {
            x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
        }
        else
        {
            x.UsePulsar(p => builder.Configuration.GetSection("Pulsar").Bind(p));
        }
//#endif
<!--#else-->
//#if (UseRabbitMQ)
        x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
//#elif (UseKafka)
        x.UseKafka(p => builder.Configuration.GetSection("Kafka").Bind(p));
//#elif (UseAzureServiceBus)
        // In development, use RedisStreams as fallback for testing
        if (builder.Environment.IsDevelopment())
        {
            x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
        }
        else
        {
            x.UseAzureServiceBus(p => builder.Configuration.GetSection("AzureServiceBus").Bind(p));
        }
//#elif (UseAmazonSQS)
        // In development, use RedisStreams as fallback for testing
        if (builder.Environment.IsDevelopment())
        {
            x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
        }
        else
        {
            x.UseAmazonSQS(p => builder.Configuration.GetSection("AmazonSQS").Bind(p));
        }
//#elif (UseNATS)
        x.UseNATS(builder.Configuration.GetConnectionString("Nats")!);
//#elif (UseRedisStreams)
        x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
//#elif (UsePulsar)
        // In development, use RedisStreams as fallback for testing
        if (builder.Environment.IsDevelopment())
        {
            x.UseRedis(builder.Configuration.GetConnectionString("Redis")!);
        }
        else
        {
            x.UsePulsar(p => builder.Configuration.GetSection("Pulsar").Bind(p));
        }
//#endif
<!--#endif-->
        x.UseDashboard(); //CAP Dashboard  path：  /cap
    });

    #endregion

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly())
            .AddCommandLockBehavior()
            .AddKnownExceptionValidationBehavior()
            .AddUnitOfWorkBehaviors());

    #region 多环境支持与服务注册发现

    builder.Services.AddMultiEnv(envOption => envOption.ServiceName = "Abc.Template")
        .UseMicrosoftServiceDiscovery();
    builder.Services.AddConfigurationServiceEndpointProvider();

    #endregion

    #region 远程服务客户端配置

    var jsonSerializerSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
    jsonSerializerSettings.AddNetCorePalJsonConverters();
    var ser = new NewtonsoftJsonContentSerializer(jsonSerializerSettings);
    var settings = new RefitSettings(ser);
    builder.Services.AddRefitClient<IUserServiceClient>(settings)
        .ConfigureHttpClient(client =>
            client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("https+http://user:8080")!))
        .AddMultiEnvMicrosoftServiceDiscovery() //多环境服务发现支持
        .AddStandardResilienceHandler(); //添加标准的重试策略

    #endregion

    #region Jobs

<!--#if (UseAspire)-->
    // When using Aspire, Redis connection is managed by Aspire
    builder.Services.AddHangfire(x => { x.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")); });
<!--#else-->
    builder.Services.AddHangfire(x => { x.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")); });
<!--#endif-->
    builder.Services.AddHangfireServer(); //hangfire dashboard  path：  /hangfire

    #endregion


    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
<!--#if (UseMongoDB)-->
        await dbContext.Database.EnsureCreatedAsync();
<!--#else-->
        await dbContext.Database.MigrateAsync();
<!--#endif-->
    }


    app.UseKnownExceptionHandler();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseStaticFiles();
    //app.UseHttpsRedirection();
//#if (UseAdmin)
    app.UseCors(); // CORS 必须在 UseRouting 之前
//#endif
    app.UseRouting();
    app.UseAuthentication(); // Authentication 必须在 Authorization 之前
    app.UseAuthorization();

    app.MapControllers();
    app.UseFastEndpoints();

    #region SignalR

    app.MapHub<ABC.Template.Web.Application.Hubs.ChatHub>("/chat");

    #endregion

    app.UseHttpMetrics();
<!--#if (!UseAspire)-->
    app.MapHealthChecks("/health");
<!--#endif-->
    app.MapMetrics(); // 通过   /metrics  访问指标
<!--#if (UseAspire)-->
    app.MapDefaultEndpoints();
<!--#endif-->
    
    // Code analysis endpoint
    app.MapGet("/code-analysis", () =>
    {
        var assemblies = new List<Assembly> { typeof(Program).Assembly, typeof(ApplicationDbContext).Assembly };
//#if (UseDemoCode)
        assemblies.Add(typeof(ABC.Template.Domain.AggregatesModel.OrderAggregate.Order).Assembly);
//#endif
//#if (UseAdmin)
        assemblies.Add(typeof(ABC.Template.Domain.AggregatesModel.UserAggregate.User).Assembly);
//#endif
        var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(
            CodeFlowAnalysisHelper.GetResultFromAssemblies(assemblies.ToArray())
        );
        return Results.Content(html, "text/html; charset=utf-8");
    });
    
    app.UseHangfireDashboard();
//#if (UseAdmin)
    app.SeedDatabase();
//#endif
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

#pragma warning disable S1118
public partial class Program
#pragma warning restore S1118
{
}
