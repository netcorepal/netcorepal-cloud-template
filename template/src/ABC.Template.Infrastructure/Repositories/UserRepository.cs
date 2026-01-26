using ABC.Template.Domain.AggregatesModel.UserAggregate;
//#if (UseMongoDB)
using Microsoft.EntityFrameworkCore;
//#endif

namespace ABC.Template.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User, UserId> { }

//#if (UseMongoDB)
/// <summary>
/// 用户仓储实现
/// MongoDB EF Core 不支持 EF.Property，重写 GetAsync 使用直接属性访问
/// </summary>
public class UserRepository(ApplicationDbContext context) : RepositoryBase<User, UserId, ApplicationDbContext>(context), IUserRepository
{
    /// <summary>
    /// 根据ID获取用户实体
    /// MongoDB EF Core 不支持 EF.Property，使用直接属性访问 u.Id == id
    /// </summary>
    public override async Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Users
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
//#else
public class UserRepository(ApplicationDbContext context) : RepositoryBase<User, UserId, ApplicationDbContext>(context), IUserRepository { }
//#endif
