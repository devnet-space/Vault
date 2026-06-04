using Devnet.Vault.Domain.Entities.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Groups;

internal sealed class GroupDetailsConfiguration : IEntityTypeConfiguration<GroupDetails>
{
    public void Configure(EntityTypeBuilder<GroupDetails> builder)
    {
        builder.ToTable("GroupDetails");

        builder.HasKey(x => x.GroupId);

        builder.Property(x => x.GroupId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.MetadataJson)
            .IsRequired()
            .HasColumnType("longtext");

        builder.Property(x => x.IsFavourite)
            .HasDefaultValue(false);

        builder.Property(x => x.GroupType)
            .IsRequired();

        // Self Referencing Relationship
        builder.HasOne(x => x.ParentGroup)
            .WithMany(x => x.ChildGroups)
            .HasForeignKey(x => x.ParentGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OwnerId);

        builder.HasIndex(x => x.ParentGroupId);

        // Prevent duplicate folder names inside same parent
        builder.HasIndex(x => new
        {
            x.OwnerId,
            x.ParentGroupId,
            x.Name
        })
        .IsUnique();
    }
}