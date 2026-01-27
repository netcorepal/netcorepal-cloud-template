using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Domain.AggregatesModel.DeptAggregate;

namespace TestAdminProject.Infrastructure;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
    : AppDbContextBase(options, mediator)
    , IMySqlCapDataStorage
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

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Dept> Depts => Set<Dept>();
    public DbSet<UserDept> UserDepts => Set<UserDept>();
}
