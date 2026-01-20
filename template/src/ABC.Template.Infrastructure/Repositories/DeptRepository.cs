//#if (UseAdmin)
using ABC.Template.Domain.AggregatesModel.DeptAggregate;

namespace ABC.Template.Infrastructure.Repositories;

/// <summary>
/// 部门仓储接口
/// </summary>
public interface IDeptRepository : IRepository<Dept, DeptId> { }

/// <summary>
/// 部门仓储实现
/// </summary>
public class DeptRepository(ApplicationDbContext context) : RepositoryBase<Dept, DeptId, ApplicationDbContext>(context), IDeptRepository { }
//#endif
