using System.ComponentModel.DataAnnotations.Schema;
using ABC.Template.Domain.DomainEvents.DeptEvents;
using ABC.Template.Domain;

namespace ABC.Template.Domain.AggregatesModel.DeptAggregate;

/// <summary>
/// 部门ID（强类型ID）
/// </summary>
public partial record DeptId : IInt64StronglyTypedId;

/// <summary>
/// 部门聚合根
/// 用于管理企业部门的层级结构
/// </summary>
public class Dept : Entity<DeptId>, IAggregateRoot
{
    /// <summary>
    /// 部门名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>
    /// 上级部门ID
    /// </summary>
    public DeptId ParentId { get; private set; } = default!;

    /// <summary>
    /// 状态（0=禁用，1=启用）
    /// </summary>
    public int Status { get; private set; } = 1;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 删除时间
    /// </summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 子部门（不映射到数据库，用于内存中的树形结构）
    /// </summary>
    [NotMapped]
    public virtual ICollection<Dept> Children { get; } = [];

    protected Dept()
    {
    }

    /// <summary>
    /// 创建部门
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <param name="remark">备注</param>
    /// <param name="parentId">上级部门ID</param>
    /// <param name="status">状态（0=禁用，1=启用）</param>
    public Dept(string name, string remark, DeptId parentId, int status)
    {
        CreatedAt =DateTimeOffset.UtcNow;
        Name = name;
        Remark = remark;
        ParentId = parentId;
        Status = status;
    }

    /// <summary>
    /// 更新部门信息
    /// </summary>
    /// <param name="name">部门名称</param>
    /// <param name="remark">备注</param>
    /// <param name="parentId">上级部门ID</param>
    /// <param name="status">状态（0=禁用，1=启用）</param>
    public void UpdateInfo(string name, string remark, DeptId parentId, int status)
    {
        Name = name;
        Remark = remark;
        ParentId = parentId;
        Status = status;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);

        AddDomainEvent(new DeptInfoChangedDomainEvent(this));
    }

    /// <summary>
    /// 激活部门
    /// </summary>
    public void Activate()
    {
        if (Status == 1)
        {
            throw new KnownException("部门已经是激活状态", ErrorCodes.DeptAlreadyActivated);
        }

        Status = 1;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 停用部门
    /// </summary>
    public void Deactivate()
    {
        if (Status == 0)
        {
            throw new KnownException("部门已经被停用", ErrorCodes.DeptAlreadyDeactivated);
        }

        Status = 0;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 软删除部门
    /// </summary>
    public void SoftDelete()
    {
        if (IsDeleted)
        {
            throw new KnownException("部门已经被删除", ErrorCodes.DeptAlreadyDeleted);
        }

        IsDeleted = true;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加子部门
    /// </summary>
    /// <param name="child">子部门</param>
    public void AddChild(Dept child)
    {
        if (child == null)
        {
            throw new KnownException("子部门不能为空", ErrorCodes.ChildDeptCannotBeEmpty);
        }

        Children.Add(child);
    }

    /// <summary>
    /// 移除子部门
    /// </summary>
    /// <param name="child">子部门</param>
    public void RemoveChild(Dept child)
    {
        if (child == null)
        {
            throw new KnownException("子部门不能为空", ErrorCodes.ChildDeptCannotBeEmpty);
        }

        Children.Remove(child);
    }

    /// <summary>
    /// 获取所有子部门（包括子级的子级）
    /// </summary>
    /// <returns>所有子部门</returns>
    public IEnumerable<Dept> GetAllChildren()
    {
        var result = new List<Dept>();
        foreach (var child in Children)
        {
            result.Add(child);
            result.AddRange(child.GetAllChildren());
        }
        return result;
    }

    /// <summary>
    /// 获取部门层级路径
    /// </summary>
    /// <returns>层级路径</returns>
    public string GetPath()
    {
        return Name;
    }
}
