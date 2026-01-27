namespace ABC.Template.Web.AppPermissions;

/// <summary>
/// 权限映射辅助类，用于通过权限代码自动获取权限名称和描述
/// </summary>
public static class PermissionMapper
{
    /// <summary>
    /// 权限代码到权限描述的映射字典
    /// 从 SeedDatabaseExtension.cs 中提取
    /// </summary>
    private static readonly Dictionary<string, string> _permissionDescriptionMap = new()
    {
        // 父权限码（用于菜单和路由权限控制）
        { PermissionCodes.UserManagement, "用户管理" },
        { PermissionCodes.RoleManagement, "角色管理" },
        { PermissionCodes.DeptManagement, "部门管理" },

        // 用户管理权限
        { PermissionCodes.UserCreate, "创建新用户" },
        { PermissionCodes.UserView, "查看用户信息" },
        { PermissionCodes.UserEdit, "更新用户信息" },
        { PermissionCodes.UserDelete, "删除用户" },
        { PermissionCodes.UserRoleAssign, "分配用户角色权限" },
        { PermissionCodes.UserResetPassword, "重置用户密码" },

        // 角色管理权限
        { PermissionCodes.RoleCreate, "创建新角色" },
        { PermissionCodes.RoleView, "查看角色信息" },
        { PermissionCodes.RoleEdit, "更新角色信息" },
        { PermissionCodes.RoleDelete, "删除角色" },
        { PermissionCodes.RoleUpdatePermissions, "更新角色的权限" },

        // 部门管理权限
        { PermissionCodes.DeptCreate, "创建部门" },
        { PermissionCodes.DeptView, "查看部门信息" },
        { PermissionCodes.DeptEdit, "更新部门信息" },
        { PermissionCodes.DeptDelete, "删除部门" },

        // 所有接口访问权限
        { PermissionCodes.AllApiAccess, "所有接口访问权限" },
    };

    /// <summary>
    /// 通过权限代码获取权限信息（名称和描述）
    /// </summary>
    /// <param name="permissionCode">权限代码</param>
    /// <returns>返回 (权限名称, 权限描述) 元组</returns>
    public static (string Name, string Description) GetPermissionInfo(string permissionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);

        // 优先从 PermissionDefinitionContext 获取权限名称（DisplayName）
        var permissionName = GetPermissionNameFromContext(permissionCode);
        
        // 如果 PermissionDefinitionContext 中没有找到，使用权限代码作为名称
        if (string.IsNullOrEmpty(permissionName))
        {
            permissionName = permissionCode;
        }

        // 从映射字典获取权限描述
        var permissionDescription = _permissionDescriptionMap.TryGetValue(permissionCode, out var description)
            ? description
            : string.Empty;

        return (permissionName, permissionDescription);
    }

    /// <summary>
    /// 从 PermissionDefinitionContext 中查找权限的 DisplayName
    /// </summary>
    /// <param name="permissionCode">权限代码</param>
    /// <returns>权限的显示名称，如果未找到则返回 null</returns>
    private static string? GetPermissionNameFromContext(string permissionCode)
    {
        foreach (var group in PermissionDefinitionContext.PermissionGroups)
        {
            // 在权限组的所有权限及其子权限中查找
            foreach (var permission in group.Permissions)
            {
                var found = FindPermissionRecursive(permission, permissionCode);
                if (found != null)
                {
                    return found.DisplayName;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 递归查找权限
    /// </summary>
    /// <param name="permission">当前权限</param>
    /// <param name="permissionCode">要查找的权限代码</param>
    /// <returns>找到的权限对象，如果未找到则返回 null</returns>
    private static AppPermission? FindPermissionRecursive(AppPermission permission, string permissionCode)
    {
        // 检查当前权限是否匹配
        if (permission.Code == permissionCode)
        {
            return permission;
        }

        // 递归查找子权限
        foreach (var child in permission.Children)
        {
            var found = FindPermissionRecursive(child, permissionCode);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
