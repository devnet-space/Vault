using Devnet.Vault.Application.Features.VaultItems.Interfaces;
using Devnet.Vault.Domain.Entities.Vault;
using Devnet.Vault.Domain.Enums;
using Devnet.Vault.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;

namespace Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.VaultItems;

internal sealed class VaultItemsRepository(AppDbContext _dbContext) : IVaultItemsRepository
{
    public async Task<VaultEntries?> AddNewVaultItem(VaultEntries vaultEntry, CancellationToken cancellationToken)
    {

        await _dbContext.VaultEntries.AddAsync(vaultEntry, cancellationToken);
        var result = await _dbContext.SaveChangesAsync(cancellationToken);
        if (result > 0)
            return vaultEntry;
        return null;
    }

    public async Task<bool> DoesEntryExists(VaultEntries vaultEntry, CancellationToken cancellationToken)
    {
        return await _dbContext.VaultEntries.AsNoTracking()
            .AnyAsync(x => x.OwnerId == vaultEntry.OwnerId && x.Title == vaultEntry.Title
            && x.GroupId == vaultEntry.GroupId && x.IsDeleted == false, cancellationToken);
    }

    public Task<bool> IsGroupValidForItems(long groupId, long ownerId, CancellationToken cancellationToken)
    {
        return _dbContext.GroupDetails.AsNoTracking()
            .AnyAsync(x =>
                x.GroupId == groupId &&
                x.OwnerId == ownerId &&
                !x.IsDeleted &&
                x.GroupType == GroupType.Password,
                cancellationToken);
    }

    public async Task<VaultEntries?> ReuseDeletedEntry(VaultEntries vaultEntry, CancellationToken cancellationToken)
    {
        var existingEntry = await _dbContext.VaultEntries
            .Where(g => g.IsDeleted)
            .OrderByDescending(g =>
                g.GroupId == vaultEntry.GroupId &&
                g.OwnerId == vaultEntry.OwnerId)
            .ThenByDescending(g =>
                g.OwnerId == vaultEntry.OwnerId)
            .ThenBy(g => g.GroupId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (existingEntry != null)
        {
            existingEntry.Title = vaultEntry.Title;
            existingEntry.GroupId = vaultEntry.GroupId;
            existingEntry.OwnerId = vaultEntry.OwnerId;
            existingEntry.EntryType = vaultEntry.EntryType;
            existingEntry.EncryptedData = vaultEntry.EncryptedData;
            existingEntry.IsDeleted = false;
            existingEntry.DeletedBy = null;
            existingEntry.DeletedDate = null;
            existingEntry.UpdatedDate = DateTime.UtcNow;
            existingEntry.UpdatedBy = vaultEntry.CreatedBy;
            var result = await _dbContext.SaveChangesAsync(cancellationToken);
            if (result > 0)
                return existingEntry;
        }
        return null;
    }

    public async Task<bool> UpdateEntryTitle(string title, long entryId, long ownerId, long updatedBy, CancellationToken cancellationToken)
    {
        var isExists = await _dbContext.VaultEntries
        .AnyAsync(e =>
            e.OwnerId == ownerId &&
            e.VaultEntryId != entryId &&
            !e.IsDeleted &&
            e.Title == title &&
            e.GroupId ==
                _dbContext.VaultEntries
                    .Where(x => x.VaultEntryId == entryId
                             && x.OwnerId == ownerId
                             && !x.IsDeleted)
                    .Select(x => x.GroupId)
                    .FirstOrDefault(),
            cancellationToken);

        if (isExists)
            throw new InvalidOperationException(VaultEntryValidationMessages.ENTRY_ALREADY_EXISTS);

        var changes = await _dbContext.VaultEntries.Where(e => e.VaultEntryId == entryId && e.OwnerId == ownerId && !e.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(e => e.Title, title)
               .SetProperty(e => e.UpdatedDate, DateTime.UtcNow)
               .SetProperty(e => e.UpdatedBy, updatedBy)
               , cancellationToken);
        return changes > 0;
    }

    public async Task<bool> UpdateEncryptedData(string encryptedData, long entryId, long ownerId, long updatedBy, CancellationToken cancellationToken)
    {
        var changes = await _dbContext.VaultEntries.Where(e => e.VaultEntryId == entryId && e.OwnerId == ownerId && !e.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(e => e.EncryptedData, encryptedData)
               .SetProperty(e => e.UpdatedDate, DateTime.UtcNow)
               .SetProperty(e => e.UpdatedBy, updatedBy)
               , cancellationToken);
        return changes > 0;
    }

    public async Task<bool> DeleteEntry(long entryId, long ownerId, long deletedBy, CancellationToken cancellationToken)
    {
        var changes = await _dbContext.VaultEntries.Where(e => e.VaultEntryId == entryId && e.OwnerId == ownerId && !e.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(e => e.IsDeleted, true)
               .SetProperty(e => e.DeletedBy, deletedBy)
               .SetProperty(e => e.DeletedDate, DateTime.UtcNow)
               .SetProperty(e => e.UpdatedDate, DateTime.UtcNow)
               .SetProperty(e => e.UpdatedBy, deletedBy)
               , cancellationToken);
        return changes > 0;
    }

    public async Task<bool> MoveEntryToNewGroup(long entryId, long? newGroupId, long ownerId, long updatedBy, CancellationToken cancellationToken)
    {
        var doesTitleExists = await _dbContext.VaultEntries
        .AnyAsync(e =>
            e.OwnerId == ownerId &&
            e.VaultEntryId != entryId &&
            !e.IsDeleted &&
            e.Title ==
                _dbContext.VaultEntries
                    .Where(x => x.VaultEntryId == entryId
                             && x.OwnerId == ownerId
                             && !x.IsDeleted)
                    .Select(x => x.Title)
                    .FirstOrDefault() &&
            e.GroupId == newGroupId,
            cancellationToken);

        if (doesTitleExists)
            throw new InvalidOperationException(VaultEntryValidationMessages.ENTRY_ALREADY_EXISTS);
        var changes = await _dbContext.VaultEntries.Where(e => e.VaultEntryId == entryId && e.OwnerId == ownerId && !e.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(e => e.GroupId, newGroupId)
               .SetProperty(e => e.UpdatedDate, DateTime.UtcNow)
               .SetProperty(e => e.UpdatedBy, updatedBy)
               , cancellationToken);
        return changes > 0;
    }
}
