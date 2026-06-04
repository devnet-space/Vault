using Devnet.Vault.Domain.Entities.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Masters;

internal sealed class RolesConfiguration : IEntityTypeConfiguration<Roles>
{
    public void Configure(EntityTypeBuilder<Roles> builder)
    {
        builder.ToTable("Roles");

        // Primary Key
        builder.HasKey(x => x.RoleId);

        builder.Property(x => x.RoleId)
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(x => x.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => x.RoleName)
            .IsUnique();
    }
}
