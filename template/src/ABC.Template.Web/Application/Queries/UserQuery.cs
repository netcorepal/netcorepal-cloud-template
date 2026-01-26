using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Queries;

/// <summary>
/// 用户信息查询DTO
/// </summary>
public record UserInfoQueryDto(UserId UserId, string Name, string Phone, IEnumerable<string> Roles, string RealName, int Status, string Email, DateTimeOffset CreatedAt, string Gender, int Age, DateTimeOffset BirthDate, DeptId? DeptId, string DeptName);

public record UserLoginInfoQueryDto(UserId UserId, string Name, string Email, string PasswordHash, IEnumerable<UserRole> UserRoles);

public class UserQueryInput : PageRequest
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}

public class UserQuery(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache) : IQuery
{
    private DbSet<User> UserSet { get; } = applicationDbContext.Users;
    private const string UserCacheKeyPrefix = "user:";
    private static readonly TimeSpan UserCacheExpiry = TimeSpan.FromMinutes(10);

//#if (UseMongoDB)
    /// <summary>
    /// 根据刷新令牌获取用户ID
    /// MongoDB 不支持 SelectMany 跨集合导航，改为直接查询 RefreshTokens 集合
    /// </summary>
    public async Task<UserId> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var userId = await applicationDbContext.Set<UserRefreshToken>()
            .AsNoTracking()
            .Where(t => t.Token == refreshToken)
            .Select(t => t.UserId)
            .SingleOrDefaultAsync(cancellationToken);

        return userId ?? throw new KnownException("无效的令牌", ErrorCodes.InvalidToken);
    }
//#else
    public async Task<UserId> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
                   .SelectMany(u => u.RefreshTokens)
                   .Where(t => t.Token == refreshToken)
                   .Select(t => t.UserId)
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new KnownException("无效的令牌", ErrorCodes.InvalidToken);
    }
//#endif

    public async Task<bool> DoesUserExist(string username, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Name == username, cancellationToken: cancellationToken);
    }

    public async Task<bool> DoesUserExist(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken: cancellationToken);
    }

    public async Task<bool> DoesEmailExist(string email, CancellationToken cancellationToken)
    {
        return await UserSet.AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 根据ID获取用户信息（带缓存）
    /// </summary>
    public async Task<UserInfoQueryDto?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"{UserCacheKeyPrefix}{userId}";
        
        return await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = UserCacheExpiry;
            
//#if (UseMongoDB)
            // MongoDB 不支持在 Select 中投影跨集合导航，拆为多次查询
            var user = await UserSet
                .IgnoreAutoIncludes()
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Phone,
                    u.RealName,
                    u.Status,
                    u.Email,
                    u.CreatedAt,
                    u.Gender,
                    u.Age,
                    u.BirthDate
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
                return null;

            var roles = await applicationDbContext.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleName)
                .ToListAsync(cancellationToken);

            var dept = await applicationDbContext.UserDepts
                .AsNoTracking()
                .Where(ud => ud.UserId == userId)
                .Select(ud => new { ud.DeptId, ud.DeptName })
                .FirstOrDefaultAsync(cancellationToken);

            return new UserInfoQueryDto(
                user.Id,
                user.Name,
                user.Phone,
                roles,
                user.RealName,
                user.Status,
                user.Email,
                user.CreatedAt,
                user.Gender,
                user.Age,
                user.BirthDate,
                dept?.DeptId,
                dept?.DeptName ?? string.Empty);
//#else
            return await UserSet.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(au => new UserInfoQueryDto(
                    au.Id,
                    au.Name,
                    au.Phone,
                    au.Roles.Select(r => r.RoleName),
                    au.RealName,
                    au.Status,
                    au.Email,
                    au.CreatedAt,
                    au.Gender,
                    au.Age,
                    au.BirthDate,
                    au.Dept != null ? au.Dept.DeptId : null,
                    au.Dept != null ? au.Dept.DeptName : string.Empty))
                .FirstOrDefaultAsync(cancellationToken);
//#endif
        });
    }

//#if (UseMongoDB)
    /// <summary>
    /// 根据角色ID获取所有用户ID列表
    /// MongoDB 不支持在 Where 中使用跨集合导航，改为先查 UserRoles，再获取 UserIds
    /// </summary>
    public async Task<List<UserId>> GetUserIdsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await applicationDbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
//#else
    public async Task<List<UserId>> GetUserIdsByRoleIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Roles.Any(r => r.RoleId == roleId))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
//#endif

    /// <summary>
    /// 根据部门ID获取所有用户ID列表
    /// </summary>
    /// <param name="deptId">部门ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>属于指定部门的所有用户ID列表</returns>
//#if (UseMongoDB)
    /// MongoDB 不支持在 Where 中使用跨集合导航，改为直接查 UserDepts
    public async Task<List<UserId>> GetUserIdsByDeptIdAsync(DeptId deptId, CancellationToken cancellationToken = default)
    {
        return await applicationDbContext.UserDepts
            .AsNoTracking()
            .Where(ud => ud.DeptId == deptId)
            .Select(ud => ud.UserId)
            .ToListAsync(cancellationToken);
    }
//#else
    public async Task<List<UserId>> GetUserIdsByDeptIdAsync(DeptId deptId, CancellationToken cancellationToken = default)
    {
        return await UserSet.AsNoTracking()
            .Where(u => u.Dept != null && u.Dept.DeptId == deptId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
//#endif

//#if (UseMongoDB)
    /// <summary>
    /// 获取登录用用户信息。MongoDB 不支持在 Select 中投影跨集合导航 u.Roles，
    /// 故拆为两次查询：先查 User 基础字段，再按 UserId 查 UserRoles。
    /// </summary>
    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginAsync(string name, CancellationToken cancellationToken)
    {
        var user = await UserSet
            .IgnoreAutoIncludes()
            .AsNoTracking()
            .Where(u => u.Name == name)
            .Select(u => new { u.Id, u.Name, u.Email, u.PasswordHash })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return null;

        var roles = await applicationDbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);

        return new UserLoginInfoQueryDto(user.Id, user.Name, user.Email, user.PasswordHash, roles);
    }

    /// <summary>
    /// 按 UserId 获取登录用用户信息。同上，避免投影 u.Roles，拆为两次查询。
    /// </summary>
    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        var user = await UserSet
            .IgnoreAutoIncludes()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Name, u.Email, u.PasswordHash })
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return null;

        var roles = await applicationDbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);

        return new UserLoginInfoQueryDto(user.Id, user.Name, user.Email, user.PasswordHash, roles);
    }
//#else
    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginAsync(string name, CancellationToken cancellationToken)
    {
        return await UserSet
            .Where(u => u.Name == name)
            .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserLoginInfoQueryDto?> GetUserInfoForLoginByIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await UserSet
            .Where(u => u.Id == userId)
            .Select(u => new UserLoginInfoQueryDto(u.Id, u.Name, u.Email, u.PasswordHash, u.Roles))
            .FirstOrDefaultAsync(cancellationToken);
    }
//#endif

//#if (UseMongoDB)
    /// <summary>
    /// 获取所有用户（分页）
    /// MongoDB 不支持在 Select 中投影跨集合导航，拆为多次查询
    /// </summary>
    public async Task<PagedData<UserInfoQueryDto>> GetAllUsersAsync(UserQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = UserSet
            .IgnoreAutoIncludes()
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => u.Name.Contains(query.Keyword!) || u.Email.Contains(query.Keyword!));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status);
        }

        var pagedUsers = await queryable
            .OrderByDescending(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Phone,
                u.RealName,
                u.Status,
                u.Email,
                u.CreatedAt,
                u.Gender,
                u.Age,
                u.BirthDate
            })
            .ToPagedDataAsync(query, cancellationToken);

        if (pagedUsers.Items == null || !pagedUsers.Items.Any())
        {
            return new PagedData<UserInfoQueryDto>(
                Enumerable.Empty<UserInfoQueryDto>(),
                pagedUsers.Total,
                pagedUsers.PageIndex,
                pagedUsers.PageSize);
        }

        var userIds = pagedUsers.Items.Select(u => u.Id).ToList();

        var allRoles = await applicationDbContext.UserRoles
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId))
            .ToListAsync(cancellationToken);
        var rolesByUserId = allRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => ur.RoleName).ToList());

        var allDepts = await applicationDbContext.UserDepts
            .AsNoTracking()
            .Where(ud => userIds.Contains(ud.UserId))
            .ToListAsync(cancellationToken);
        var deptByUserId = allDepts
            .ToDictionary(ud => ud.UserId, ud => new { ud.DeptId, ud.DeptName });

        var userInfoDtos = pagedUsers.Items.Select(u =>
        {
            var dept = deptByUserId.GetValueOrDefault(u.Id);
            return new UserInfoQueryDto(
                u.Id,
                u.Name,
                u.Phone,
                rolesByUserId.GetValueOrDefault(u.Id, []),
                u.RealName,
                u.Status,
                u.Email,
                u.CreatedAt,
                u.Gender,
                u.Age,
                u.BirthDate,
                dept?.DeptId,
                dept?.DeptName ?? string.Empty);
        }).ToList();

        return new PagedData<UserInfoQueryDto>(
            userInfoDtos,
            pagedUsers.Total,
            pagedUsers.PageIndex,
            pagedUsers.PageSize);
    }
//#else
    public async Task<PagedData<UserInfoQueryDto>> GetAllUsersAsync(UserQueryInput query, CancellationToken cancellationToken)
    {
        var queryable = UserSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            queryable = queryable.Where(u => u.Name.Contains(query.Keyword!) || u.Email.Contains(query.Keyword!));
        }

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(u => u.Status == query.Status);
        }

        return await queryable
            .OrderByDescending(u => u.Id)
            .Select(u => new UserInfoQueryDto(
                u.Id,
                u.Name,
                u.Phone,
                u.Roles.Select(r => r.RoleName),
                u.RealName,
                u.Status,
                u.Email,
                u.CreatedAt,
                u.Gender,
                u.Age,
                u.BirthDate,
                u.Dept != null ? u.Dept.DeptId : null,
                u.Dept != null ? u.Dept.DeptName : string.Empty))
            .ToPagedDataAsync(query, cancellationToken);
    }
//#endif
}
