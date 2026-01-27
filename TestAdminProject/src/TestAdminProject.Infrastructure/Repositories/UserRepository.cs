using TestAdminProject.Domain.AggregatesModel.UserAggregate;

namespace TestAdminProject.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User, UserId> { }

public class UserRepository(ApplicationDbContext context) : RepositoryBase<User, UserId, ApplicationDbContext>(context), IUserRepository { }
