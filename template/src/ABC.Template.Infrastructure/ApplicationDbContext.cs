using MediatR;
using Microsoft.EntityFrameworkCore;
//#if (UseDMDB)
using Microsoft.EntityFrameworkCore.Storage;
//#endif
//#if (UseMongoDB)
using Microsoft.EntityFrameworkCore.Infrastructure;
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
//#elif (UseMongoDB)
    /// <summary>
    /// 单机 MongoDB 不支持事务，禁用自动事务以避免 SaveChanges 报错。
    /// 生产环境若使用副本集，可移除此配置以启用事务保证一致性。
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
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
