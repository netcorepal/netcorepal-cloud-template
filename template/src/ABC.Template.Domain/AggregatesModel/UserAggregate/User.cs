using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;

namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

public partial record UserId : IInt64StronglyTypedId;

public class User : Entity<UserId>, IAggregateRoot
{
    protected User()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string RealName { get; private set; } = string.Empty;
    public int Status { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginTime { get; private set; }
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
    public Deleted IsDeleted { get; private set; } = new Deleted(false);
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);
    public string Gender { get; private set; } = string.Empty;
    public int Age { get; private set; }
    public DateTimeOffset BirthDate { get; private set; } = default!;

    public virtual ICollection<UserRole> Roles { get; } = [];

    public virtual UserDept Dept { get; private set; } = default!;

    public ICollection<UserRefreshToken> RefreshTokens { get; } = [];

    public User(string name, string phone, string password, IEnumerable<UserRole> roles, string realName, int status, string email, string gender, DateTimeOffset birthDate)
    {
        CreatedAt = DateTimeOffset.Now;
        Name = name;
        Phone = phone;
        PasswordHash = password;
        RealName = realName;
        Status = status;
        Email = email;
        Gender = gender;
        Age = CalculateAge(birthDate);
        BirthDate = birthDate;
        foreach (var userRole in roles)
        {
            Roles.Add(userRole);
        }
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public void PasswordReset(string password)
    {
        PasswordHash = password;
    }

    public void SetUserRefreshToken(string refreshToken)
    {
        var refreshTokenInfo = new UserRefreshToken(refreshToken);
        RefreshTokens.Add(refreshTokenInfo);
    }

    public void UpdateLastLoginTime(DateTimeOffset loginTime)
    {
        LastLoginTime = loginTime;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    public static int CalculateAge(DateTimeOffset birthDate)
    {
        var today = DateTimeOffset.Now.Date;
        int age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    public void UpdateUserInfo(string name, string phone, string realName, int status, string email, string gender, DateTimeOffset birthDate)
    {
        Name = name;
        Phone = phone;
        RealName = realName;
        Status = status;
        Email = email;
        Gender = gender;
        Age = CalculateAge(birthDate);
        BirthDate = birthDate;
    }

    public void UpdateRoleInfo(RoleId roleId, string roleName)
    {
        var savedRole = Roles.FirstOrDefault(r => r.RoleId == roleId);
        savedRole?.UpdateRoleInfo(roleName);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (!string.IsNullOrEmpty(newPasswordHash))
        {
            PasswordHash = newPasswordHash;
            UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        }
    }

    public void UpdateRoles(IEnumerable<UserRole> rolesToBeAssigned)
    {
        var currentRoleMap = Roles.ToDictionary(r => r.RoleId);
        var targetRoleMap = rolesToBeAssigned.ToDictionary(r => r.RoleId);

        var roleIdsToRemove = currentRoleMap.Keys.Except(targetRoleMap.Keys);
        foreach (var roleId in roleIdsToRemove)
        {
            Roles.Remove(currentRoleMap[roleId]);
        }

        var roleIdsToAdd = targetRoleMap.Keys.Except(currentRoleMap.Keys);
        foreach (var roleId in roleIdsToAdd)
        {
            var targetRole = targetRoleMap[roleId];
            Roles.Add(targetRole);
        }
    }

    /// <summary>
    /// 分配部门
    /// </summary>
    /// <param name="dept">部门</param>
    public void AssignDept(UserDept dept)
    {
        ArgumentNullException.ThrowIfNull(dept);

        Dept = dept;
    }

    /// <summary>
    /// 更新部门名称
    /// </summary>
    /// <param name="deptName">新的部门名称</param>
    public void UpdateDeptName(string deptName)
    {
        if (Dept == null)
        {
            return;
        }

        Dept.UpdateDeptName(deptName);
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 撤销所有未过期的刷新令牌（用于退出登录）
    /// </summary>
    public void RevokeAllRefreshTokens()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var token in RefreshTokens)
        {
            // 只撤销未过期且未使用的令牌
            if (!token.IsRevoked && !token.IsUsed && token.ExpiresTime > now)
            {
                token.Revoke();
            }
        }
        UpdateTime = new UpdateTime(now);
    }
}
