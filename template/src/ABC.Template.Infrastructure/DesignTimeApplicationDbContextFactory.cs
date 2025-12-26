using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace ABC.Template.Infrastructure;

public class DesignTimeApplicationDbContextFactory: IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMediatR(c =>
            c.RegisterServicesFromAssemblies(typeof(DesignTimeApplicationDbContextFactory).Assembly));
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // change connectionstring if you want to run command “dotnet ef database update”
            <!--#if (UseMySql)-->
            options.UseMySql("Server=any;User ID=any;Password=any;Database=any",
                new MySqlServerVersion(new Version(8, 0, 34)),
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                    b.UseMicrosoftJson();
                });
            <!--#elif (UseSqlServer)-->
            options.UseSqlServer("Server=any;Database=any;Trusted_Connection=true;",
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                });
            <!--#elif (UsePostgreSQL)-->
            options.UseNpgsql("Host=any;Database=any;Username=any;Password=any",
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                });
            <!--#elif (UseSqlite)-->
            options.UseSqlite("Data Source=any.db",
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                });
            <!--#elif (UseGaussDB)-->
            options.UseGaussDB("Host=any;Database=any;Username=any;Password=any",
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                });
            <!--#elif (UseDMDB)-->
            options.UseDm("Host=any;Database=any;Username=any;Password=any",
                b =>
                {
                    b.MigrationsAssembly(typeof(DesignTimeApplicationDbContextFactory).Assembly.FullName);
                });
            <!--#endif-->
        });
        var provider = services.BuildServiceProvider();
        var dbContext = provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return dbContext;
    }
}