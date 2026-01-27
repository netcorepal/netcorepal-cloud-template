using System.Collections.Immutable;

namespace TestAdminProject.Web.AppPermissions;

/// <summary>
/// 管理权限定义的上下文类，负责初始化和提供权限组及其权限项。
/// </summary>
public static class PermissionDefinitionContext
{
    // 存储权限组的字典，键为权限组名称，值为权限组对象
    private static Dictionary<string, AppPermissionGroup> Groups { get; } = new();

    // 静态构造函数，在类初始化时创建默认的权限组和权限项
    static PermissionDefinitionContext()
    {
        var systemAccess = AddGroup("SystemAccess");
        
        // 用户管理权限
        var adminUserManagement = systemAccess.AddPermission(PermissionCodes.UserManagement, "用户管理");
        adminUserManagement.AddChild(PermissionCodes.UserCreate, "创建用户");
        adminUserManagement.AddChild(PermissionCodes.UserEdit, "编辑用户");
        adminUserManagement.AddChild(PermissionCodes.UserDelete, "删除用户");
        adminUserManagement.AddChild(PermissionCodes.UserView, "查看用户");
        adminUserManagement.AddChild(PermissionCodes.UserRoleAssign, "分配用户角色");
        adminUserManagement.AddChild(PermissionCodes.UserResetPassword, "重置用户密码");
        
        // 角色管理权限
        var roleManagement = systemAccess.AddPermission(PermissionCodes.RoleManagement, "角色管理");
        roleManagement.AddChild(PermissionCodes.RoleCreate, "创建角色");
        roleManagement.AddChild(PermissionCodes.RoleEdit, "编辑角色");
        roleManagement.AddChild(PermissionCodes.RoleDelete, "删除角色");
        roleManagement.AddChild(PermissionCodes.RoleView, "查看角色");
        roleManagement.AddChild(PermissionCodes.RoleUpdatePermissions, "更新角色权限");

        // 部门管理权限
        var deptManagement = systemAccess.AddPermission(PermissionCodes.DeptManagement, "部门管理");
        deptManagement.AddChild(PermissionCodes.DeptCreate, "创建部门");
        deptManagement.AddChild(PermissionCodes.DeptEdit, "编辑部门");
        deptManagement.AddChild(PermissionCodes.DeptDelete, "删除部门");
        deptManagement.AddChild(PermissionCodes.DeptView, "查看部门");

        // 所有接口访问权限
        var allApiAccess = systemAccess.AddPermission(PermissionCodes.AllApiAccess, "所有接口访问权限");
    }

    /// <summary>
    /// 添加一个新的权限组，如果权限组名称已存在则抛出异常。
    /// </summary>
    /// <param name="name">权限组名称</param>
    /// <returns>返回创建的权限组</returns>
    /// <exception cref="ArgumentException">如果权限组名称已经存在，则抛出异常</exception>
    private static AppPermissionGroup AddGroup(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (Groups.ContainsKey(name))
        {
            throw new ArgumentException($"There is already an existing permission group with name: {name}");
        }

        return Groups[name] = new AppPermissionGroup(name);
    }

    /// <summary>
    /// 获取所有的权限组。
    /// </summary>
    public static IReadOnlyList<AppPermissionGroup> PermissionGroups => Groups.Values.ToImmutableList();
}

