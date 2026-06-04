using Devnet.Vault.Domain.Entities.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Masters;

internal sealed class FeaturesConfiguration : IEntityTypeConfiguration<Features>
{
    public void Configure(EntityTypeBuilder<Features> builder)
    {
        builder.ToTable("Features");

        // Primary Key
        builder.HasKey(x => x.FeatureId);

        builder.Property(x => x.FeatureId)
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(x => x.FeatureName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Enum → STRING mapping 
        builder.Property(x => x.FeatureGroup)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.ParentFeature)
            .WithMany(x => x.SubFeatures)
            .HasForeignKey(x => x.SubfeatureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}