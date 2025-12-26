using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace ABC.Template.AppHost.Tests;

public class AppHostTests
{
    [Fact]
    public async Task AppHost_Should_Start_Successfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>();

        // Act & Assert
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Verify the app started successfully
        Assert.NotNull(app);
        
        await app.StopAsync();
    }

    [Fact]
    public async Task AppHost_Should_Have_Web_Resource()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        
        // Assert
        var webResource = app.Resources.FirstOrDefault(r => r.Name == "web");
        Assert.NotNull(webResource);
        
        await app.StartAsync();
        await app.StopAsync();
    }

    [Fact]
    public async Task AppHost_Should_Have_Redis_Resource()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        
        // Assert
        var redisResource = app.Resources.FirstOrDefault(r => r.Name.Equals("Redis", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(redisResource);
        
        await app.StartAsync();
        await app.StopAsync();
    }

//#if (!UseSqlite)
    [Fact]
    public async Task AppHost_Should_Have_Database_Resource()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        
        // Assert
        var dbResource = app.Resources.FirstOrDefault(r => 
//#if (UseMySql)
            r.Name.Equals("MySql", StringComparison.OrdinalIgnoreCase) ||
//#elif (UseSqlServer)
            r.Name.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) ||
//#elif (UsePostgreSQL)
            r.Name.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
//#elif (UseGaussDB)
            r.Name.Equals("GaussDB", StringComparison.OrdinalIgnoreCase) ||
//#elif (UseDMDB)
            r.Name.Equals("DMDB", StringComparison.OrdinalIgnoreCase) ||
//#endif
            r.Name.Equals("Database", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(dbResource);
        
        await app.StartAsync();
        await app.StopAsync();
    }
//#endif

//#if (UseRabbitMQ || UseKafka)
    [Fact]
    public async Task AppHost_Should_Have_MessageQueue_Resource()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        
        // Assert
        var mqResource = app.Resources.FirstOrDefault(r => 
//#if (UseRabbitMQ)
            r.Name.Equals("rabbitmq", StringComparison.OrdinalIgnoreCase)
//#elif (UseKafka)
            r.Name.Equals("kafka", StringComparison.OrdinalIgnoreCase)
//#endif
        );
        Assert.NotNull(mqResource);
        
        await app.StartAsync();
        await app.StopAsync();
    }
//#endif
}
