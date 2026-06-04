using Devnet.Vault.Domain.Entities.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devnet.Vault.Infrastructure.Persistence.Configurations.Masters;

internal sealed class CountriesConfiguration : IEntityTypeConfiguration<Countries>
{
    public void Configure(EntityTypeBuilder<Countries> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.CountryId);
        builder.Property(x => x.CountryId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CountryName)
                .IsRequired()
                .HasMaxLength(200);

        builder.Property(x => x.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.CountryCallingCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => x.CountryCode)
            .IsUnique();

        builder.HasIndex(x => x.CountryCallingCode)
            .IsUnique();

        builder.Property(x => x.IsActive)
           .IsRequired()
           .HasDefaultValue(true);
    }
}
