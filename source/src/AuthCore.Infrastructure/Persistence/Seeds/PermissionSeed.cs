using AuthCore.Domain.Entities;

namespace AuthCore.Infrastructure.Persistence.Seeds;

public static class PermissionSeed
{ 
    public static readonly IReadOnlyCollection<Permission> Data =
    [
        Permission.Create("users:read", "Read users")
    ];
}
