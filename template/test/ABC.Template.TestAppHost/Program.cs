namespace ABC.Template.TestAppHost;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);
        await builder.Build().RunAsync();
    }
}

