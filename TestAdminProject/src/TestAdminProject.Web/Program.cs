using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using FluentValidation.AspNetCore;
using TestAdminProject.Web.Clients;
using TestAdminProject.Web.Extensions;
using TestAdminProject.Web.Utils;
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

Log.Logger = new LoggerConfiguration()
    .Enrich.WithClientIp()
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

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

    var redis = await ConnectionMultiplexer.ConnectAsync(builder.Configuration.GetConnectionString("Redis")!);
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => redis);
    
    // DataProtection - use custom extension that resolves IConnectionMultiplexer from DI
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis("DataProtection-Keys");

      // 配置JWT认证
   builder.Services.Configure<AppConfiguration>(builder.Configuration.GetSection("AppConfiguration"));
   var appConfig = builder.Configuration.GetSection("AppConfiguration").Get<AppConfiguration>() ?? new AppConfiguration { JwtIssuer = "netcorepal", JwtAudience = "netcorepal" };
   
   builder.Services.AddAuthentication().AddJwtBearer(options =>
   {
       options.RequireHttpsMetadata = false;
       options.TokenValidationParameters.ValidAudience = appConfig.JwtAudience;
       options.TokenValidationParameters.ValidateAudience = true;
       options.TokenValidationParameters.ValidIssuer = appConfig.JwtIssuer;
       options.TokenValidationParameters.ValidateIssuer = true;
   });
    builder.Services.AddNetCorePalJwt().AddRedisStore();

    #endregion

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

    #region Query
    // 自动注册所有实现 IQuery 接口的查询类
    builder.Services.AddQueries(Assembly.GetExecutingAssembly());
    #endregion

    #region 基础设施

    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseMySql(builder.Configuration.GetConnectionString("MySql"),
            new MySqlServerVersion(new Version(8, 0, 34)));
        // 仅在开发环境启用敏感数据日志，防止生产环境泄露敏感信息
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
        options.EnableDetailedErrors();
    });
    builder.Services.AddUnitOfWork<ApplicationDbContext>();
    builder.Services.AddRedisLocks();
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
        x.UseNetCorePalStorage<ApplicationDbContext>();
        x.JsonSerializerOptions.AddNetCorePalJsonConverters();
        x.ConsumerThreadCount = Environment.ProcessorCount;
        x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
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

    builder.Services.AddHangfire(x => { x.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")); });
    builder.Services.AddHangfireServer(); //hangfire dashboard  path：  /hangfire

    #endregion


    var app = builder.Build();

    // 在非生产环境中执行数据库迁移（包括开发、测试、Staging等环境）
    if (!app.Environment.IsProduction())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // SeedDatabase 必须在数据库迁移之后执行，确保表已创建
    // 在非生产环境中，如果 SeedDatabase 失败，应该抛出异常以便发现问题
    if (!app.Environment.IsProduction())
    {
        try
        {
            app.SeedDatabase();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to seed database in {Environment} environment", app.Environment.EnvironmentName);
            throw; // 重新抛出异常，确保测试能够发现数据库种子数据问题
        }
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
    app.UseCors(); // CORS 必须在 UseRouting 之前
    app.UseRouting();
    app.UseAuthentication(); // Authentication 必须在 Authorization 之前
    app.UseAuthorization();

    app.MapControllers();
    app.UseFastEndpoints();

    #region SignalR

    app.MapHub<TestAdminProject.Web.Application.Hubs.ChatHub>("/chat");

    #endregion

    app.UseHttpMetrics();
    app.MapHealthChecks("/health");
    app.MapMetrics(); // 通过   /metrics  访问指标
    
    // Code analysis endpoint
    app.MapGet("/code-analysis", () =>
    {
        var assemblies = new List<Assembly> { typeof(Program).Assembly, typeof(ApplicationDbContext).Assembly };
        assemblies.Add(typeof(TestAdminProject.Domain.AggregatesModel.UserAggregate.User).Assembly);
        var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(
            CodeFlowAnalysisHelper.GetResultFromAssemblies(assemblies.ToArray())
        );
        return Results.Content(html, "text/html; charset=utf-8");
    });
    
    app.UseHangfireDashboard();
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
