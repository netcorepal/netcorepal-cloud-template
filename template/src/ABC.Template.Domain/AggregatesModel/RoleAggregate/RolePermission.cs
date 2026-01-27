namespace ABC.Template.Domain.AggregatesModel.RoleAggregate;

public class RolePermission
{
    protected RolePermission()
    {
    }

    public RoleId RoleId { get; internal set; } = default!;
    public string PermissionCode { get; private set; } = string.Empty;
    public string PermissionName { get; private set; } = string.Empty;
    public string PermissionDescription { get; private set; } = string.Empty;

    public RolePermission(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    public RolePermission(string permissionCode, string permissionName, string permissionDescription)
    {
        PermissionCode = permissionCode;
        PermissionName = permissionName;
        PermissionDescription = permissionDescription;
    }

    public void UpdatePermissionInfo(string permissionName, string permissionDescription)
    {
        PermissionName = permissionName;
        PermissionDescription = permissionDescription;
    }
}
