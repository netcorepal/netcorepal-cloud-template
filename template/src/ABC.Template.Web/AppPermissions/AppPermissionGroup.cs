using System.Collections.Immutable;

namespace ABC.Template.Web.AppPermissions;

/// <summary>
/// 表示一个权限组，包含该组的所有权限以及相关操作。
/// </summary>
public sealed class AppPermissionGroup
{
    /// <summary>
    /// 权限组的唯一名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 权限组内的所有权限，权限是只读的。
    /// </summary>
    public IReadOnlyList<AppPermission> Permissions => _permissions.ToImmutableList();

    private readonly List<AppPermission> _permissions = [];

    /// <summary>
    /// 创建一个新的权限组。
    /// </summary>
    /// <param name="name">权限组的名称。</param>
    internal AppPermissionGroup(string name)
    {
        // 初始化权限组的名称和权限集合
        Name = name;
    }

    /// <summary>
    /// 向权限组中添加一个权限。
    /// </summary>
    /// <param name="code">权限的唯一代码。</param>
    /// <param name="name">权限的名称。</param>
    /// <param name="isEnabled">是否启用该权限，默认为 true。</param>
    /// <returns>返回创建的权限对象。</returns>
    public AppPermission AddPermission(string code, string name, bool isEnabled = true)
    {
        var permission = new AppPermission(code, name, isEnabled);
        _permissions.Add(permission); // 将权限添加到权限组中
        return permission;
    }
}

