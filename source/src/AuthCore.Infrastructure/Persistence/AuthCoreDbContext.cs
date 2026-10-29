using Microsoft.EntityFrameworkCore;

namespace AuthCore.Infrastructure.Persistence;

public sealed class AuthCoreDbContext(DbContextOptions<AuthCoreDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Підхоплює всі IEntityTypeConfiguration<T> з папки Persistence/Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthCoreDbContext).Assembly);
    }
}
