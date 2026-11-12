using AuthCore.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

namespace AuthCore.Infrastructure.Persistence;

public sealed class AuthCoreDatabaseInitializer
{
    private readonly AuthCoreDbContext _context;
    private readonly AuthCoreDbSeeder _seeder;

    public AuthCoreDatabaseInitializer(AuthCoreDbContext context, AuthCoreDbSeeder seeder)
    {
        _context = context;
        _seeder = seeder;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

        if (!canConnect)
        {
            throw new InvalidOperationException("Cannot connect to the database.");
        }

        var migrations = _context.Database.GetMigrations();

        if (!migrations.Any())
        {
            throw new InvalidOperationException("No database migrations were found in the application.");
        }

        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendingMigrations.Any())
        {
            throw new InvalidOperationException("Database has pending migrations.");
        }

        await _seeder.SeedAsync(cancellationToken);
    }
}
