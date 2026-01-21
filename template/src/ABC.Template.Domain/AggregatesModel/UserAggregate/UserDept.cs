using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Domain;

namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

/// <summary>
/// 用户部门关系实体
/// 表示用户与部门的一对一关系
/// </summary>
public class UserDept
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;

    /// <summary>
    /// 部门ID
    /// </summary>
    public DeptId DeptId { get; private set; } = default!;

    /// <summary>
    /// 部门名称
    /// </summary>
    public string DeptName { get; private set; } = string.Empty;

    /// <summary>
    /// 分配时间
    /// </summary>
    public DateTimeOffset AssignedAt { get; init; }

    protected UserDept()
    {
    }

    /// <summary>
    /// 创建用户部门关系
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="deptId">部门ID</param>
    /// <param name="deptName">部门名称</param>
    public UserDept(UserId userId, DeptId deptId, string deptName)
    {
        UserId = userId;
        DeptId = deptId;
        AssignedAt = DateTimeOffset.UtcNow;
        DeptName = deptName;
    }

    /// <summary>
    /// 更新部门名称
    /// </summary>
    /// <param name="deptName">新的部门名称</param>
    public void UpdateDeptName(string deptName)
    {
        if (string.IsNullOrWhiteSpace(deptName))
        {
            throw new KnownException("部门名称不能为空", ErrorCodes.DeptNameCannotBeEmpty);
        }

        DeptName = deptName;
    }
}
