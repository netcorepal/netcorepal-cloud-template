using TestAdminProject.Domain.AggregatesModel.RoleAggregate;

namespace TestAdminProject.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role, RoleId> { }

public class RoleRepository(ApplicationDbContext context) : RepositoryBase<Role, RoleId, ApplicationDbContext>(context), IRoleRepository { }
