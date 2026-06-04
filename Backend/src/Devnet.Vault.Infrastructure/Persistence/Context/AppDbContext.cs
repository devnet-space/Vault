using Devnet.Vault.Domain.Common;
using Devnet.Vault.Domain.Entities.Groups;
using Devnet.Vault.Domain.Entities.Identity;
using Devnet.Vault.Domain.Entities.Masters;
using Devnet.Vault.Domain.Entities.Vault;
using Microsoft.EntityFrameworkCore;

namespace Devnet.Vault.Infrastructure.Persistence.Context;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    // =========================
    // DbSets (Identity)
    // =========================
    public DbSet<UserDetails> Users => Set<UserDetails>();
    public DbSet<UserLogins> UserLogins => Set<UserLogins>();
    public DbSet<Roles> Roles => Set<Roles>();
    public DbSet<RoleFeatures> RoleFeatures => Set<RoleFeatures>();


    // =========================
    // DbSets (Masters)
    // =========================
    public DbSet<Countries> Countries => Set<Countries>();
    public DbSet<Features> Features => Set<Features>();

    /// <summary>
    /// DbSets (Vault)
    /// </summary>
    public DbSet<GroupDetails> GroupDetails => Set<GroupDetails>();
    public DbSet<VaultFiles> VaultFiles => Set<VaultFiles>();
    public DbSet<VaultEntries> VaultEntries => Set<VaultEntries>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureAuditProperties(modelBuilder);
    }

    private static void ConfigureAuditProperties(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditProperty).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.CreatedBy));

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.CreatedDate))
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.IsDeleted))
                    .IsRequired()
                    .HasDefaultValue(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.UpdatedBy));

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.UpdatedDate));

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.DeletedBy));

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditProperty.DeletedDate));
            }
        }
    }
}