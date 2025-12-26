//#if (UseAspire)
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace ABC.Template.Web.Tests.Fixtures;

public class AspireHostAppFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _client;

    public IServiceProvider Services => throw new NotSupportedException("Services property is not available when using AspireHostAppFixture. Use the web app's service provider directly through dependency injection in tests.");
    
    public HttpClient Client => _client ?? throw new InvalidOperationException("Client not initialized. Call InitializeAsync first.");
    
    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ABC_Template_TestAppHost>();
        
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Get the web resource and create HTTP client
        _client = _app.CreateHttpClient("web");
    }

    public HttpClient CreateClient()
    {
        if (_app == null)
        {
            throw new InvalidOperationException("App not initialized. Call InitializeAsync first.");
        }
        
        return _app.CreateHttpClient("web");
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
//#endif
