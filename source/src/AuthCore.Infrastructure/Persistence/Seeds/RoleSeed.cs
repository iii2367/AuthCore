using AuthCore.Domain.Entities;

namespace AuthCore.Infrastructure.Persistence.Seeds;

public static class RoleSeed
{ 
    public static readonly IReadOnlyCollection<Role> Data =
    [
        Role.Create("admin", "System administrator"),
        Role.Create("user",  "Regular user")
    ];
}
