using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class RolePermission
{
    private RolePermission() {}

    private RolePermission(
            Guid roleId, 
            Guid permissionId)
    {
        RoleId          = roleId;
        PermissionId    = permissionId;
    }

    public static RolePermission Create(
            Guid roleId, 
            Guid permissionId)
    {
        if (roleId == Guid.Empty)       { throw new DomainException("RoleId must be set."); }
        if (permissionId == Guid.Empty) { throw new DomainException("PermissionId must be set."); }
  
        return new RolePermission(roleId, permissionId);
    }

    public Guid RoleId          { get; private set; }
    public Guid PermissionId    { get; private set; }
}
