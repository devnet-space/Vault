using Devnet.Vault.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Identity;

internal sealed class UserDetailsConfiguration : IEntityTypeConfiguration<UserDetails>
{
    public void Configure(EntityTypeBuilder<UserDetails> builder)
    {
        builder.ToTable("UserDetails");

        // Primary Key
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .ValueGeneratedOnAdd();
        // Properties
        builder.Property(x => x.Name)
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.ProfileUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsDeactivated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.LastLoginDate);

        builder.Property(x => x.DeactivatedAt);

        builder.Property(x => x.UserSecretKey)
            .IsRequired()
            .HasMaxLength(500);

        // Indexes 
        builder.HasIndex(u => new { u.Email, u.IsDeleted })
        .IsUnique();

        builder.HasIndex(u => new { u.PhoneNumber, u.IsDeleted })
        .IsUnique();

        builder.HasIndex(u => new { u.UserId, u.IsDeleted })
        .IsUnique();

        builder.Property(x => x.RoleId);

        builder.Property(x => x.CountryId);

        // Relationships
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
