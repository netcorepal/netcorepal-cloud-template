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

        await app.ResourceNotifications.WaitForResourceHealthyAsync("web");

        // Verify the app started successfully
        Assert.NotNull(app);
        
        await app.StopAsync();
    }
}
