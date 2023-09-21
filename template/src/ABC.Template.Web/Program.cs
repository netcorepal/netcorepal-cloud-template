using NetCorePal.Extensions.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using FluentValidation.AspNetCore;
using FluentValidation;
using NetCorePal.Extensions.Domain.Json;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using ABC.Template.Web.Extensions;
using Serilog;
using Serilog.Formatting.Json;

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
    builder.Services.AddMvc();
    builder.Services.AddSignalR();

    #endregion

    #region Prometheus监控

    builder.Services.AddHealthChecks().ForwardToPrometheus();
    builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();

    #endregion

    // Add services to the container.

    #region 身份认证

    var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!);
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => redis);
    builder.Services.AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

    #endregion

    #region Controller

    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new EntityIdJsonConverterFactory());
    });
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.AddEntityIdSchemaMap()); //强类型id swagger schema 映射

    #endregion

    #region 公共服务

    builder.Services.AddSingleton<IClock, SystemClock>();

    #endregion


    #region 集成事件

    builder.Services.AddTransient<OrderPaidIntegrationEventHandler>();

    #endregion

    #region 模型验证器

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    #endregion


    #region Mapper Provider

    builder.Services.AddMapperPrivider(Assembly.GetExecutingAssembly());

    #endregion

    #region Query

    builder.Services.AddScoped<OrderQuery>();

    #endregion


    #region 基础设施

    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()).AddUnitOfWorkBehaviors());
    builder.Services.AddRepositories(typeof(ApplicationDbContext).Assembly);

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"),
            b => b.MigrationsAssembly(typeof(Program).Assembly.FullName));
        options.LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
    builder.Services.AddUnitOfWork<ApplicationDbContext>();
    builder.Services.AddPostgreSqlTransactionHandler();
    builder.Services.AddAllCAPEventHanders(typeof(Program));
    builder.Services.AddCap(x =>
    {
        x.UseEntityFramework<ApplicationDbContext>();
        x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
    });
    builder.Services.AddCAPSagaEventPublisher();
    builder.Services.AddSagas<ApplicationDbContext>(typeof(Program));

    #endregion

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllers();

    #region SignalR

    app.MapHub<ABC.Template.Web.Application.Hubs.ChatHub>("/chat");

    #endregion

    app.UseHttpMetrics();
    app.MapHealthChecks("/health");
    app.MapMetrics("/metrics"); // 通过   /metrics  访问指标
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

#pragma warning disable S1118
public partial class Program
#pragma warning restore S1118
{
}