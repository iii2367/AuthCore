using AuthCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthCore.Infrastructure.Persistence.Configurations;

public sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.ToTable("credentials");

        builder.HasKey(credential => credential.UserId);

        builder.Property(credential => credential.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(credential => credential.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Credential>(credential => credential.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
