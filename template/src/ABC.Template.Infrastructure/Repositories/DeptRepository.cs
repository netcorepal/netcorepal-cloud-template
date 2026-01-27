using ABC.Template.Domain.AggregatesModel.DeptAggregate;
//#if (UseMongoDB)
using Microsoft.EntityFrameworkCore;
//#endif

namespace ABC.Template.Infrastructure.Repositories;

/// <summary>
/// 部门仓储接口
/// </summary>
public interface IDeptRepository : IRepository<Dept, DeptId> { }

/// <summary>
/// 部门仓储实现
/// </summary>
//#if (UseMongoDB)
/// MongoDB EF Core 不支持 EF.Property，重写 GetAsync 使用直接属性访问
public class DeptRepository(ApplicationDbContext context) : RepositoryBase<Dept, DeptId, ApplicationDbContext>(context), IDeptRepository
{
    /// <summary>
    /// 根据ID获取部门实体
    /// MongoDB EF Core 不支持 EF.Property，使用直接属性访问 d.Id == id
    /// </summary>
    public override async Task<Dept?> GetAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Depts
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
//#else
public class DeptRepository(ApplicationDbContext context) : RepositoryBase<Dept, DeptId, ApplicationDbContext>(context), IDeptRepository { }
//#endif