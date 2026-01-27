using ABC.Template.Domain.AggregatesModel.RoleAggregate;
//#if (UseMongoDB)
using Microsoft.EntityFrameworkCore;
//#endif

namespace ABC.Template.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<Role, RoleId> { }

//#if (UseMongoDB)
/// <summary>
/// 角色仓储实现
/// MongoDB EF Core 不支持 EF.Property，重写 GetAsync 使用直接属性访问
/// </summary>
public class RoleRepository(ApplicationDbContext context) : RepositoryBase<Role, RoleId, ApplicationDbContext>(context), IRoleRepository
{
    /// <summary>
    /// 根据ID获取角色实体
    /// MongoDB EF Core 不支持 EF.Property，使用直接属性访问 r.Id == id
    /// </summary>
    public override async Task<Role?> GetAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Roles
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
//#else
public class RoleRepository(ApplicationDbContext context) : RepositoryBase<Role, RoleId, ApplicationDbContext>(context), IRoleRepository { }
//#endif