using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;

namespace ABC.Template.Web.Application.Queries;

public record RoleQueryDto(RoleId RoleId, string Name, string Description, bool IsActive, DateTimeOffset CreatedAt, IEnumerable<string> PermissionCodes);

public class RoleQueryInput : PageRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}

public record AssignAdminUserRoleQueryDto(RoleId RoleId, string RoleName, IEnumerable<string> PermissionCodes);

public class RoleQuery(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache) : IQuery
{
    private DbSet<Role> RoleSet { get; } = applicationDbContext.Roles;
    private const string RoleCacheKeyPrefix = "role:";
    private static readonly TimeSpan RoleCacheExpiry = TimeSpan.FromMinutes(10);

    public async Task<bool> DoesRoleExist(string name, CancellationToken cancellationToken)
    {
        return await RoleSet.AsNoTracking()
            .AnyAsync(r => r.Name == name, cancellationToken: cancellationToken);
    }

//#if (UseMongoDB)
    /// <summary>
    /// 获取用于分配的管理员角色信息
    /// MongoDB 不支持在 Select 中投影跨集合导航，拆为多次查询
    /// </summary>
    public async Task<List<AssignAdminUserRoleQueryDto>> GetAdminRolesForAssignmentAsync(IEnumerable<RoleId> ids, CancellationToken cancellationToken)
    {
        var roleIdList = ids.ToList();
        if (!roleIdList.Any())
        {
            return [];
        }

        var roles = await RoleSet
            .IgnoreAutoIncludes()
            .AsNoTracking()
            .Where(r => roleIdList.Contains(r.Id))
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(cancellationToken);

        if (!roles.Any())
        {
            return [];
        }

        var allPermissions = await applicationDbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIdList.Contains(rp.RoleId))
            .ToListAsync(cancellationToken);

        var permissionsByRoleId = allPermissions
            .GroupBy(rp => rp.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(rp => rp.PermissionCode).ToList());

        return roles.Select(r => new AssignAdminUserRoleQueryDto(
            r.Id,
            r.Name,
            permissionsByRoleId.GetValueOrDefault(r.Id, []))).ToList();
    }
//#else
    public async Task<List<AssignAdminUserRoleQueryDto>> GetAdminRolesForAssignmentAsync(IEnumerable<RoleId> ids, CancellationToken cancellationToken)
    {
        return await RoleSet.AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Select(r => new AssignAdminUserRoleQueryDto(
                r.Id,
                r.Name,
                r.Permissions.Select(rp => rp.PermissionCode)))
            .ToListAsync(cancellationToken);
    }
//#endif

//#if (UseMongoDB)
    /// <summary>
    /// 获取指定角色的所有权限代码
    /// MongoDB 不支持 SelectMany 跨集合导航，改为直接查询 RolePermissions 集合
    /// </summary>
    public async Task<IEnumerable<string>> GetAssignedPermissionCodesAsync(IEnumerable<RoleId> ids, CancellationToken cancellationToken)
    {
        if (!ids.Any())
        {
            return Enumerable.Empty<string>();
        }

        var roleIds = ids.ToList();
        return await applicationDbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
//#else
    public async Task<IEnumerable<string>> GetAssignedPermissionCodesAsync(IEnumerable<RoleId> ids, CancellationToken cancellationToken)
    {
        if (!ids.Any())
        {
            return Enumerable.Empty<string>();
        }

        var roleIds = ids.ToList();
        return await RoleSet.AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .SelectMany(r => r.Permissions)
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
//#endif

    public async Task<RoleQueryDto?> GetRoleByIdAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{RoleCacheKeyPrefix}{id}";
        
        return await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = RoleCacheExpiry;
            
//#if (UseMongoDB)
            var role = await RoleSet
                .IgnoreAutoIncludes()
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new { r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt })
                .FirstOrDefaultAsync(cancellationToken);

            if (role == null)
                return null;

            var permissions = await applicationDbContext.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.PermissionCode)
                .ToListAsync(cancellationToken);

            return new RoleQueryDto(role.Id, role.Name, role.Description, role.IsActive, role.CreatedAt, permissions);
//#else
            return await RoleSet.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RoleQueryDto(r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.Permissions.Select(rp => rp.PermissionCode)))
                .FirstOrDefaultAsync(cancellationToken);
//#endif
        });
    }

//#if (UseMongoDB)
    /// <summary>
    /// 获取所有角色（分页）
    /// MongoDB 不支持在 Select 中投影跨集合导航，拆为多次查询
    /// </summary>
    public async Task<PagedData<RoleQueryDto>> GetAllRolesAsync(RoleQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = RoleSet
            .IgnoreAutoIncludes()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            queryable = queryable.Where(r => r.Name.Contains(query.Name!));
        }

        if (!string.IsNullOrWhiteSpace(query.Description))
        {
            queryable = queryable.Where(r => r.Description.Contains(query.Description!));
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(r => r.IsActive == query.IsActive);
        }

        if (query.StartTime.HasValue)
        {
            queryable = queryable.Where(r => r.CreatedAt >= query.StartTime!.Value);
        }

        if (query.EndTime.HasValue)
        {
            queryable = queryable.Where(r => r.CreatedAt <= query.EndTime!.Value);
        }

        var pagedRoles = await queryable
            .OrderBy(r => r.Id)
            .Select(r => new { r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt })
            .ToPagedDataAsync(query, cancellationToken);

        if (pagedRoles.Items == null || !pagedRoles.Items.Any())
        {
            return new PagedData<RoleQueryDto>(
                Enumerable.Empty<RoleQueryDto>(),
                pagedRoles.Total,
                pagedRoles.PageIndex,
                pagedRoles.PageSize);
        }

        var roleIds = pagedRoles.Items.Select(r => r.Id).ToList();

        var allPermissions = await applicationDbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .ToListAsync(cancellationToken);

        var permissionsByRoleId = allPermissions
            .GroupBy(rp => rp.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(rp => rp.PermissionCode).ToList());

        var roleDtos = pagedRoles.Items.Select(r => new RoleQueryDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsActive,
            r.CreatedAt,
            permissionsByRoleId.GetValueOrDefault(r.Id, []))).ToList();

        return new PagedData<RoleQueryDto>(
            roleDtos,
            pagedRoles.Total,
            pagedRoles.PageIndex,
            pagedRoles.PageSize);
    }
//#else
    public async Task<PagedData<RoleQueryDto>> GetAllRolesAsync(RoleQueryInput query, CancellationToken cancellationToken)
    {
        return await RoleSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Name), r => r.Name.Contains(query.Name!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.Description), r => r.Description.Contains(query.Description!))
            .WhereIf(query.IsActive.HasValue, r => r.IsActive == query.IsActive)
            .WhereIf(query.StartTime.HasValue, r => r.CreatedAt >= query.StartTime!.Value)
            .WhereIf(query.EndTime.HasValue, r => r.CreatedAt <= query.EndTime!.Value)
            .OrderBy(r => r.Id)
            .Select(r => new RoleQueryDto(r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.Permissions.Select(rp => rp.PermissionCode)))
            .ToPagedDataAsync(query, cancellationToken);
    }
//#endif
}

