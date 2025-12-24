using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using ABC.Template.Domain.AggregatesModel.DeliverAggregate;

namespace ABC.Template.Infrastructure;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
    : AppDbContextBase(options, mediator)
    //#if (UseMySql)
    , IMySqlCapDataStorage
    //#elif (UseSqlServer)
    , ISqlServerCapDataStorage
    //#elif (UsePostgreSQL)
    , IPostgreSqlCapDataStorage
    //#elif (UseSqlite)
    , ISqliteCapDataStorage
    //#elif (UseGaussDB)
    , IGaussDBCapDataStorage
    //#elif (UseDMDB)
    , IDMDBCapDataStorage
    //#endif
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }


    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ConfigureStronglyTypedIdValueConverter(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<DeliverRecord> DeliverRecords => Set<DeliverRecord>();
}
