using Devnet.Vault.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Identity;

internal sealed class UserLoginsConfiguration : IEntityTypeConfiguration<UserLogins>
{
    public void Configure(EntityTypeBuilder<UserLogins> builder)
    {
        builder.ToTable("UserLogins");

        // Primary Key
        builder.HasKey(x => x.UserLoginId);

        builder.Property(x => x.UserLoginId)
            .ValueGeneratedOnAdd();

        // Required fields
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.ExpiryDate);

        builder.Property(x => x.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);


        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.RefreshTokenHash);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
