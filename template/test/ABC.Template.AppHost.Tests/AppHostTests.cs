using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace ABC.Template.AppHost.Tests;

public class AppHostTests
{
    // Increased timeout to 240 seconds to accommodate MongoDB replica set initialization
    // which can take 60-120 seconds to become ready
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(240);

    [Fact]
    public async Task AppHost_Should_Start_Successfully()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_AppHost>(cancellationToken);
        // Act & Assert
        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        // Verify the app started successfully
        Assert.NotNull(app);

        await app.StopAsync(cancellationToken);
    }
}
