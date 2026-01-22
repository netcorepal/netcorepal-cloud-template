using MediatR;
using Microsoft.EntityFrameworkCore;
//#if (UseDMDB)
using Microsoft.EntityFrameworkCore.Storage;
//#endif
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
//#if (UseAdmin)
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
//#endif

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
    //#elif (UseMongoDB)
    , IMongoDBCapDataStorage
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


//#if (UseDMDB)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IRelationalTypeMappingSource, MyDmTypeMappingSource>();
        base.OnConfiguring(optionsBuilder);
    }
//#endif

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ConfigureStronglyTypedIdValueConverter(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

//#if (UseAdmin)
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Dept> Depts => Set<Dept>();
    public DbSet<UserDept> UserDepts => Set<UserDept>();
//#endif
}
