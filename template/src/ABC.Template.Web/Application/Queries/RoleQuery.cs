//#if (UseAdmin)
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

    public async Task<IEnumerable<string>> GetAssignedPermissionCodesAsync(IEnumerable<RoleId> ids, CancellationToken cancellationToken)
    {
        if (!ids.Any())
        {
            return Enumerable.Empty<string>();
        }

        // 将 ids 转换为列表，避免多次枚举
        var roleIds = ids.ToList();
        
        // 直接在数据库查询中使用 Distinct，让数据库处理去重，提高性能
        return await RoleSet.AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .SelectMany(r => r.Permissions)
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<RoleQueryDto?> GetRoleByIdAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{RoleCacheKeyPrefix}{id}";
        
        return await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = RoleCacheExpiry;
            
            return await RoleSet.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RoleQueryDto(r.Id, r.Name, r.Description, r.IsActive, r.CreatedAt, r.Permissions.Select(rp => rp.PermissionCode)))
                .FirstOrDefaultAsync(cancellationToken);
        });
    }

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
}
//#endif
