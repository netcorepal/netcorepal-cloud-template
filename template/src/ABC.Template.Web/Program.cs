using NetCorePal.Extensions.Primitives;
using ABC.Template.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using System;
using FluentValidation.AspNetCore;
using FluentValidation;
using NetCorePal.Extensions.Domain.Json;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.Application.IntegrationEventHandlers;
using NetCorePal.Extensions.Repository.EntityframeworkCore.Extensions;
using NetCorePal.Extensions.Repository;
using NetCorePal.Extensions.Domain;
using Microsoft.OpenApi.Models;
using ABC.Template.Domain;
using ABC.Template.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
#region SignalR
builder.Services.AddMvc();
builder.Services.AddSignalR();
#endregion

#region Prometheus监控
builder.Services.AddHealthChecks().ForwardToPrometheus();
builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();
#endregion
// Add services to the container.

#region  文件系统
//TODO: 注册文件服务为fileprovider，如阿里云对象存储

#endregion
#region 身份认证
var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(p => redis);
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
//.PersistKeysToFileSystem(new System.IO.DirectoryInfo("d://DataProtection-Keys"));
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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
builder.Services.AddRepositories();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    //options.UseInMemoryDatabase("ApplicationDbContext");
    options.UseMySql(builder.Configuration.GetConnectionString("MySql"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")));

    options.LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
});
builder.Services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<ApplicationDbContext>());
builder.Services.AddCap(x =>
{
    x.UseEntityFramework<ApplicationDbContext>();
    x.UseRabbitMQ(p => builder.Configuration.GetSection("RabbitMQ").Bind(p));
});
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
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();   // 通过   /metrics  访问指标
});

app.Run();
