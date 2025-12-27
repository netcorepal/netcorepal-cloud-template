using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace ABC.Template.AppHost.Tests;

public class AppHostTests
{
    //[Fact]
    public async Task AppHost_Should_Start_Successfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>(TestContext.Current.CancellationToken);
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        // Act & Assert
        await using var app = await appHost.BuildAsync(cts.Token);
        
        await app.StartAsync(cts.Token);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("web", cts.Token);

        // Verify the app started successfully
        Assert.NotNull(app);
        
        await app.StopAsync(TestContext.Current.CancellationToken);
    }
}
