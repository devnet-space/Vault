using Devnet.Vault.Domain.Entities.Vault;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Vault;

internal sealed class VaultFilesConfiguration : IEntityTypeConfiguration<VaultFiles>
{
    public void Configure(EntityTypeBuilder<VaultFiles> builder)
    {
        builder.ToTable("VaultFiles");

        builder.HasKey(x => x.VaultFileId);

        builder.Property(x => x.VaultFileId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FileKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Extension)
            .HasMaxLength(50);

        builder.Property(x => x.MetadataJson)
            .HasColumnType("longtext");

        builder.Property(x => x.IsFavourite)
            .HasDefaultValue(false);

        // Group Relationship
        builder.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Owner Relationship
        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OwnerId);

        builder.HasIndex(x => x.GroupId);

        builder.HasIndex(x => x.FileKey)
            .IsUnique();
    }
}
