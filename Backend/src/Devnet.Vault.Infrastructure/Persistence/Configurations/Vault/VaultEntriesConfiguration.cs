using Devnet.Vault.Domain.Entities.Vault;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Vault;

internal sealed class VaultEntriesConfiguration : IEntityTypeConfiguration<VaultEntries>
{
    public void Configure(EntityTypeBuilder<VaultEntries> builder)
    {
        builder.ToTable("VaultEntries");

        // Primary Key
        builder.HasKey(x => x.VaultEntryId);

        // Auto Increment
        builder.Property(x => x.VaultEntryId)
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.EncryptedData)
            .IsRequired()
            .HasColumnType("longtext");

        builder.Property(x => x.EntryType)
            .IsRequired();

        builder.Property(x => x.IsFavourite)
            .HasDefaultValue(false);

        // Group Relationship
        builder.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // User Relationship
        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OwnerId);

        builder.HasIndex(x => x.GroupId);

        builder.HasIndex(x => x.EntryType);

        builder.HasIndex(x => new
        {
            x.OwnerId,
            x.GroupId,
            x.Title
        });

        // Prevent duplicate titles in same group for same user
        builder.HasIndex(x => new
        {
            x.OwnerId,
            x.GroupId,
            x.Title
        })
        .IsUnique();
    }
}