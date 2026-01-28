using Microsoft.EntityFrameworkCore;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;

namespace ABC.Template.Web.Application.Queries;

/// <summary>
/// 部门查询DTO
/// </summary>
public record DeptQueryDto(DeptId Id, string Name, string Remark, DeptId ParentId, int Status, DateTimeOffset CreatedAt, DeletedTime? DeletedAt);

/// <summary>
/// 部门查询输入参数
/// </summary>
public class DeptQueryInput
{
    public string? Name { get; set; }
    public string? Remark { get; set; }
    public int? Status { get; set; }
    public DeptId? ParentId { get; set; }
}

/// <summary>
/// 部门树形DTO - 应用层数据传输对象
/// </summary>
public record DeptTreeDto(
    DeptId Id,
    string Name,
    string Remark,
    DeptId ParentId,
    int Status,
    DateTimeOffset CreatedAt,
    IEnumerable<DeptTreeDto> Children);

/// <summary>
/// 部门查询服务
/// </summary>
public class DeptQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<Dept> DeptSet { get; } = applicationDbContext.Depts;

    /// <summary>
    /// 检查部门名称是否存在
    /// </summary>
    public async Task<bool> DoesDeptExist(string name, CancellationToken cancellationToken)
    {
        return await DeptSet.AsNoTracking()
            .AnyAsync(d => d.Name == name, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 检查部门ID是否存在
    /// </summary>
    public async Task<bool> DoesDeptExist(DeptId id, CancellationToken cancellationToken)
    {
        return await DeptSet.AsNoTracking()
            .AnyAsync(d => d.Id == id, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 根据ID获取部门
    /// </summary>
    public async Task<DeptQueryDto?> GetDeptByIdAsync(DeptId id, CancellationToken cancellationToken = default)
    {
        return await DeptSet.AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DeptQueryDto(d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.CreatedAt, d.DeletedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有部门
    /// </summary>
    public async Task<IEnumerable<DeptQueryDto>> GetAllDeptsAsync(DeptQueryInput query, CancellationToken cancellationToken)
    {
        return await DeptSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), d => d.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Remark), d => d.Remark.Contains(query.Remark!))
            .WhereIf(query.Status.HasValue, d => d.Status == query.Status)
            .WhereIf(query.ParentId != null, d => d.ParentId == query.ParentId)
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DeptQueryDto(d.Id, d.Name, d.Remark, d.ParentId, d.Status, d.CreatedAt, d.DeletedAt))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取部门树
    /// 优化：使用投影只选择需要的字段，减少内存占用
    /// </summary>
    public async Task<IEnumerable<DeptTreeDto>> GetDeptTreeAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        // 使用投影只选择构建树所需的字段，减少内存占用
        var allDepts = await DeptSet.AsNoTracking()
            .WhereIf(!includeInactive, d => d.Status != 0)
            .Select(d => new DeptTreeNode
            {
                Id = d.Id,
                Name = d.Name,
                Remark = d.Remark,
                ParentId = d.ParentId,
                Status = d.Status,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // 构建树形结构
        var treeStructure = BuildTreeStructureFromProjection(allDepts);

        // 转换为应用层DTO
        return treeStructure.Select(d => ConvertToTreeDtoFromProjection(d));
    }

    /// <summary>
    /// 构建部门树形结构（基于投影数据）
    /// </summary>
    private static List<DeptTreeNode> BuildTreeStructureFromProjection(
        List<DeptTreeNode> allDepts)
    {
        var deptDict = allDepts.ToDictionary(d => d.Id);
        var result = new List<DeptTreeNode>();

        foreach (var dept in allDepts)
        {
            // 只处理根节点（ParentId为0）
            if (dept.ParentId == new DeptId(0))
            {
                result.Add(BuildTreeDtoFromProjection(dept, deptDict));
            }
        }

        return result.OrderBy(d => d.CreatedAt).ToList();
    }

    /// <summary>
    /// 构建单个部门的树形结构（基于投影数据）
    /// </summary>
    private static DeptTreeNode BuildTreeDtoFromProjection(
        DeptTreeNode dept,
        Dictionary<DeptId, DeptTreeNode> allDepts)
    {
        var children = new List<DeptTreeNode>();

        // 查找所有以当前部门为父级的子部门
        var childDepts = allDepts.Values
            .Where(d => d.ParentId == dept.Id)
            .OrderBy(d => d.CreatedAt);

        foreach (var child in childDepts)
        {
            children.Add(BuildTreeDtoFromProjection(child, allDepts));
        }

        return new DeptTreeNode
        {
            Id = dept.Id,
            Name = dept.Name,
            Remark = dept.Remark,
            ParentId = dept.ParentId,
            Status = dept.Status,
            CreatedAt = dept.CreatedAt,
            Children = children
        };
    }

    /// <summary>
    /// 将投影节点转换为树形DTO
    /// </summary>
    private static DeptTreeDto ConvertToTreeDtoFromProjection(DeptTreeNode node)
    {
        var children = node.Children
            .OrderBy(d => d.CreatedAt)
            .Select(d => ConvertToTreeDtoFromProjection(d))
            .ToList();

        return new DeptTreeDto(
            node.Id,
            node.Name,
            node.Remark,
            node.ParentId,
            node.Status,
            node.CreatedAt,
            children
        );
    }

    /// <summary>
    /// 部门树节点（用于内存中的树构建）
    /// </summary>
    private sealed class DeptTreeNode
    {
        public DeptId Id { get; set; }=default!;
        public string Name { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public DeptId ParentId { get; set; }=default!;
        public int Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<DeptTreeNode> Children { get; set; } = new();
    }
}
