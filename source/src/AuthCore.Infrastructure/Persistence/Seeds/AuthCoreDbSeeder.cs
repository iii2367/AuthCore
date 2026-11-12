using Microsoft.EntityFrameworkCore;

namespace AuthCore.Infrastructure.Persistence.Seeds;

public sealed class AuthCoreDbSeeder
{
    private readonly AuthCoreDbContext _context;

    public AuthCoreDbSeeder(AuthCoreDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await SeedRolesAsync(cancellationToken);
        await SeedPermissionsAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var existingNames = await _context.Roles
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var newRoles = RoleSeed.Data
            .Where(role => !existingNames.Contains(role.Name))
            .ToList();

        if (newRoles.Count > 0)
        {
            _context.Roles.AddRange(newRoles);
        }
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        var existingNames = await _context.Permissions
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var newPermissions = PermissionSeed.Data
            .Where(permission => !existingNames.Contains(permission.Name))
            .ToList();

        if (newPermissions.Count > 0)
        {
            _context.Permissions.AddRange(newPermissions);
        }
    }
}
