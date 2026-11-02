using AuthCore.Domain.Common;

namespace AuthCore.Domain.Entities;

public class UserRole
{
    private UserRole() {}

    private UserRole(
            Guid userId, 
            Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public static UserRole Create(
            Guid userId, 
            Guid roleId)
    {
        if (userId == Guid.Empty) { throw new DomainException("UserId must be set"); }
        if (roleId == Guid.Empty) { throw new DomainException("RoleId must be set"); }
  
        return new UserRole(userId, roleId);
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
}
