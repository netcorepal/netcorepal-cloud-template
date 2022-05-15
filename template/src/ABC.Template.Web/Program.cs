using ABC.Extensions.Primitives;
using ABC.Template.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region Prometheus监控
builder.Services.AddHealthChecks().ForwardToPrometheus();
builder.Services.AddHttpClient(Options.DefaultName)
        .UseHttpClientMetrics();
#endregion
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddMediatR(typeof(Program).Assembly);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{

#if DEBUG
    options.UseInMemoryDatabase("ApplicationDbContext");
    options.LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
#else
    options.UseMySql(builder.Configuration.GetConnectionString("db"), ServerVersion.AutoDetect(builder.Configuration["TenantIndexDb:ConnectionString"]));
#endif

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.UseHttpMetrics();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();   // 通过   /metrics  访问指标
});

app.Run();
