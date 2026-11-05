using AuthCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthCore.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Id)
            .HasColumnName("id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(permission => permission.Name)
            .HasColumnName("name")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(permission => permission.Name)
            .IsUnique();

        builder.Property(permission => permission.Description)
            .HasColumnName("description")
            .HasMaxLength(512)
            .IsRequired();
    }
}

