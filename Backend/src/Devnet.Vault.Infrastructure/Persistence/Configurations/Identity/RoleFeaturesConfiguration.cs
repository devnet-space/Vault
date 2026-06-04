using Devnet.Vault.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Identity;

internal sealed class RoleFeaturesConfiguration : IEntityTypeConfiguration<RoleFeatures>
{
    public void Configure(EntityTypeBuilder<RoleFeatures> builder)
    {
        builder.ToTable("RoleFeatures");

        // Composite Primary Key
        builder.HasKey(x => new { x.RoleId, x.FeatureId });

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany(x => x.RoleFeatures)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Feature)
            .WithMany()
            .HasForeignKey(x => x.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
