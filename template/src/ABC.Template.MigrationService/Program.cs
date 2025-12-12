using Microsoft.EntityFrameworkCore;
using ABC.Template.Infrastructure;
using ABC.Template.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ApiDbInitializer>();

builder.AddServiceDefaults();

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(c =>
    c.RegisterServicesFromAssemblies(assembly));

builder.Services.AddDbContextPool<ApplicationDbContext>(options => 
//#if (UseMySql)
    options.UseMySql(builder.Configuration.GetConnectionString("MySql"),
        new MySqlServerVersion(new Version(8, 0, 34)), sqlOptions =>
            sqlOptions.MigrationsAssembly(assembly.FullName))
//#elif (UseSqlServer)
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
        sqlOptions.MigrationsAssembly(assembly.FullName))
//#elif (UsePostgreSQL)
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), sqlOptions =>
        sqlOptions.MigrationsAssembly(assembly.FullName))
//#elif (UseSqlite)
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"), sqlOptions =>
        sqlOptions.MigrationsAssembly(assembly.FullName))
//#elif (UseGaussDB)
    options.UseGaussDB(builder.Configuration.GetConnectionString("GaussDB"), sqlOptions =>
        sqlOptions.MigrationsAssembly(assembly.FullName))
//#elif (UseKingbaseES)
    options.UseKdbndp(builder.Configuration.GetConnectionString("KingbaseES"), sqlOptions =>
        sqlOptions.MigrationsAssembly(assembly.FullName))
//#endif
);

//#if (UseMySql)
builder.EnrichMySqlDbContext<ApplicationDbContext>(
    configureSettings: settings =>
    {
        settings.DisableRetry = false;
    });
//#elif (UseSqlServer)
builder.EnrichSqlServerDbContext<ApplicationDbContext>(
    configureSettings: settings =>
    {
        settings.DisableRetry = false;
    });
//#elif (UsePostgreSQL)
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(
    configureSettings: settings =>
    {
        settings.DisableRetry = false;
    });
//#elif (UseGaussDB)
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(
    configureSettings: settings =>
    {
        settings.DisableRetry = false;
    });
//#elif (UseKingbaseES)
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(
    configureSettings: settings =>
    {
        settings.DisableRetry = false;
    });
//#endif

var host = builder.Build();
await host.RunAsync();