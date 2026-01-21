using ABC.Template.Domain.AggregatesModel.RoleAggregate;

namespace ABC.Template.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role, RoleId> { }

public class RoleRepository(ApplicationDbContext context) : RepositoryBase<Role, RoleId, ApplicationDbContext>(context), IRoleRepository { }