//#if (UseAdmin)
using ABC.Template.Domain.DomainEvents.RoleEvents;
using ABC.Template.Domain;

namespace ABC.Template.Domain.AggregatesModel.RoleAggregate;

public partial record RoleId : IGuidStronglyTypedId;

public class Role : Entity<RoleId>, IAggregateRoot
{
    protected Role()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsActive { get; private set; } = true;
    public Deleted IsDeleted { get; private set; } = new Deleted(false);
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    public virtual ICollection<RolePermission> Permissions { get; init; } = [];

    public Role(string name, string description, IEnumerable<RolePermission> permissions)
    {
        CreatedAt = DateTimeOffset.Now;
        Name = name;
        Description = description;
        Permissions = new List<RolePermission>(permissions);
        IsActive = true;
    }

    public void UpdateRoleInfo(string name, string description)
    {
        Name = name;
        Description = description;
        AddDomainEvent(new RoleInfoChangedDomainEvent(this));
    }

    public void UpdateRolePermissions(IEnumerable<RolePermission> newPermissions)
    {
        var currentPermissionMap = Permissions.ToDictionary(p => p.PermissionCode);
        var newPermissionMap = newPermissions.ToDictionary(p => p.PermissionCode);

        var permissionsToRemove = currentPermissionMap.Keys.Except(newPermissionMap.Keys).ToList();
        foreach (var permissionCode in permissionsToRemove)
        {
            Permissions.Remove(currentPermissionMap[permissionCode]);
        }

        var permissionsToAdd = newPermissionMap.Keys.Except(currentPermissionMap.Keys).ToList();
        foreach (var permissionCode in permissionsToAdd)
        {
            Permissions.Add(newPermissionMap[permissionCode]);
        }

        AddDomainEvent(new RolePermissionChangedDomainEvent(this));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new KnownException("角色已经被停用", ErrorCodes.RoleAlreadyDeactivated);
        }

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
        {
            throw new KnownException("角色已经是激活状态", ErrorCodes.RoleAlreadyActivated);
        }

        IsActive = true;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }

    public bool HasPermission(string permissionCode)
    {
        return Permissions.Any(p => p.PermissionCode == permissionCode);
    }
}
//#endif
