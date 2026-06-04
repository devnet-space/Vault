using Devnet.Vault.Application.Features.Groups.Interfaces.Repositories;
using Devnet.Vault.Domain.Entities.Groups;
using Devnet.Vault.Domain.Enums;
using Devnet.Vault.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;

namespace Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.Groups;

internal sealed class GroupRepository(AppDbContext _dbContext) : IGroupRepository
{
    public async Task<bool> CreateNewGroup(GroupDetails group, CancellationToken ctx)
    {
        try
        {
            await _dbContext.GroupDetails.AddAsync(group, ctx);
            var changes = await _dbContext.SaveChangesAsync(ctx);
            return changes > 0;
        }
        catch (Exception)
        {
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<bool> DoesGroupExist(string groupName, long ownerId, long? parentGroupId, CancellationToken ctx)
    {
        return await _dbContext.GroupDetails
            .Where(g => g.Name == groupName && g.OwnerId == ownerId && g.ParentGroupId == parentGroupId && !g.IsDeleted)
            .AnyAsync(ctx);
    }

    public async Task<GroupDetails?> ReuseDeletedGroupName(GroupDetails group, CancellationToken ctx)
    {
        var existingGroup = await _dbContext.GroupDetails
            .Where(g => g.IsDeleted)
            .OrderByDescending(g =>
                g.ParentGroupId == group.ParentGroupId &&
                g.OwnerId == group.OwnerId)
            .ThenByDescending(g =>
                g.OwnerId == group.OwnerId)
            .ThenBy(g => g.GroupId)
            .FirstOrDefaultAsync(ctx);

        if (existingGroup != null)
        {
            existingGroup.Name = group.Name;
            existingGroup.ParentGroupId = group.ParentGroupId;
            existingGroup.OwnerId = group.OwnerId;
            existingGroup.IsFavourite = group.IsFavourite;
            existingGroup.MetadataJson = group.MetadataJson;
            existingGroup.GroupType = group.GroupType;

            existingGroup.IsDeleted = false;
            existingGroup.DeletedBy = null;
            existingGroup.DeletedDate = null;
            existingGroup.UpdatedDate = DateTime.UtcNow;
            existingGroup.UpdatedBy = group.CreatedBy;

            var changes = await _dbContext.SaveChangesAsync(ctx);

            if (changes > 0)
                return existingGroup;
        }

        return null;
    }

    public async Task<bool> UpdateGroupName(string groupName, long groupId, long ownerId, long updatedBy, CancellationToken ctx)
    {
        var duplicateExists = await _dbContext.GroupDetails
        .AnyAsync(g =>
            g.OwnerId == ownerId &&
            g.GroupId != groupId &&
            !g.IsDeleted &&
            g.Name == groupName &&
            g.ParentGroupId ==
                _dbContext.GroupDetails
                    .Where(x => x.GroupId == groupId
                             && x.OwnerId == ownerId
                             && !x.IsDeleted)
                    .Select(x => x.ParentGroupId)
                    .FirstOrDefault(),
            ctx);

        if (duplicateExists)
            throw new InvalidOperationException(GroupValidationMessages.GROUP_ALREADY_EXISTS);

        var changes = await _dbContext.GroupDetails.Where(g => g.GroupId == groupId && g.OwnerId == ownerId && !g.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(g => g.Name, groupName)
               .SetProperty(g => g.UpdatedDate, DateTime.UtcNow)
               .SetProperty(g => g.UpdatedBy, updatedBy)
               , ctx);
        return changes > 0;
    }

    public async Task<bool> UpdateGroupFavouriteStatus(bool isFavourite, long groupId, long ownerId, long updatedBy, CancellationToken ctx)
    {
        var changes = await _dbContext.GroupDetails.Where(g => g.GroupId == groupId && g.OwnerId == ownerId && !g.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(g => g.IsFavourite, isFavourite)
               .SetProperty(g => g.UpdatedDate, DateTime.UtcNow)
               .SetProperty(g => g.UpdatedBy, updatedBy)
               , ctx);
        return changes > 0;
    }


    public async Task<bool> UpdateGroupParent(long? parentGroupId, GroupType groupType, long groupId, long ownerId, long updatedBy, CancellationToken ctx)
    {
        // Cannot move inside itself
        if (parentGroupId == groupId)
            throw new InvalidOperationException(GroupValidationMessages.DRAG_NOT_ALLOWED);

        // Current group
        var currentGroup = await _dbContext.GroupDetails
            .AsNoTracking()
            .Where(g => g.GroupId == groupId
                     && g.OwnerId == ownerId
                     && g.GroupType == groupType
                     && !g.IsDeleted)
            .Select(g => new
            {
                g.GroupId,
                g.Name
            })
            .FirstOrDefaultAsync(ctx) ?? throw new InvalidOperationException(GroupValidationMessages.GROUP_NOT_FOUND);

        // Validate parent exists
        if (parentGroupId != null)
        {
            var parentExists = await _dbContext.GroupDetails
                .AnyAsync(g =>
                    g.GroupId == parentGroupId &&
                    g.OwnerId == ownerId &&
                    g.GroupType == groupType &&
                    !g.IsDeleted,
                    ctx);

            if (!parentExists)
                throw new InvalidOperationException(GroupValidationMessages.GROUP_NOT_FOUND);
        }

        // Prevent circular hierarchy
        var currentParentId = parentGroupId;

        while (currentParentId != null)
        {
            if (currentParentId == groupId)
                throw new InvalidOperationException(GroupValidationMessages.DRAG_NOT_ALLOWED);

            currentParentId = await _dbContext.GroupDetails
                .Where(g => g.GroupId == currentParentId && !g.IsDeleted)
                .Select(g => g.ParentGroupId)
                .FirstOrDefaultAsync(ctx);
        }

        // Prevent duplicate name in target level
        var duplicateExists = await _dbContext.GroupDetails
            .AnyAsync(g =>
                g.OwnerId == ownerId &&
                g.GroupId != groupId &&
                !g.IsDeleted &&
                g.ParentGroupId == parentGroupId &&
                g.Name == currentGroup.Name,
                ctx);

        if (duplicateExists)
            throw new InvalidOperationException(GroupValidationMessages.GROUP_ALREADY_EXISTS);

        var changes = await _dbContext.GroupDetails
            .Where(g => g.GroupId == groupId
                     && g.OwnerId == ownerId
                     && g.GroupType == groupType
                     && !g.IsDeleted)
            .ExecuteUpdateAsync(x => x
                .SetProperty(g => g.ParentGroupId, parentGroupId)
                .SetProperty(g => g.UpdatedDate, DateTime.UtcNow)
                .SetProperty(g => g.UpdatedBy, updatedBy),
                ctx);

        return changes > 0;
    }

    public async Task<bool> UpdateGroupMetadata(long groupId, string metadataJson, long ownerId, long updatedBy, CancellationToken ctx)
    {
        var changes = await _dbContext.GroupDetails.Where(g => g.GroupId == groupId && g.OwnerId == ownerId && !g.IsDeleted)
               .ExecuteUpdateAsync(X => X.SetProperty(g => g.MetadataJson, metadataJson)
               .SetProperty(g => g.UpdatedDate, DateTime.UtcNow)
               .SetProperty(g => g.UpdatedBy, updatedBy),
               ctx);
        return changes > 0;
    }

    public async Task<bool> DeleteGroup(long groupId, long ownerId, long updatedBy, CancellationToken ctx)
    {
        // Only allow deletion if there are no vault items or child groups inside this group
        var canDelete = await _dbContext.GroupDetails
            .Where(g => g.GroupId == groupId
                        && g.OwnerId == ownerId
                        && !g.IsDeleted)
            .Select(g => new
            {
                HasChildGroups = _dbContext.GroupDetails
                    .Any(c => c.ParentGroupId == groupId && !c.IsDeleted),

                HasVaultEntries = _dbContext.VaultEntries
                    .Any(v => v.GroupId == groupId && !v.IsDeleted),

                HasVaultFiles = _dbContext.VaultFiles
                    .Any(v => v.GroupId == groupId && !v.IsDeleted)
            })
            .FirstOrDefaultAsync(ctx) ?? throw new InvalidOperationException(GroupValidationMessages.GROUP_NOT_FOUND);

        if (canDelete.HasChildGroups ||
            canDelete.HasVaultEntries ||
            canDelete.HasVaultFiles)
            throw new InvalidOperationException(GroupValidationMessages.DELETE_NOT_ALLOWED);

        var changes = await _dbContext.GroupDetails
            .Where(g => g.GroupId == groupId
                        && g.OwnerId == ownerId
                        && !g.IsDeleted)
            .ExecuteUpdateAsync(x => x
                .SetProperty(g => g.IsDeleted, true)
                .SetProperty(g => g.UpdatedDate, DateTime.UtcNow)
                .SetProperty(g => g.UpdatedBy, updatedBy)
                .SetProperty(g => g.DeletedDate, DateTime.UtcNow)
                .SetProperty(g => g.DeletedBy, updatedBy), ctx);

        return changes > 0;
    }

    public async Task<List<GroupDetails>> GetChildGroupDetails(long ownerId, long? parentGroupId, GroupType groupType, CancellationToken ctx)
    {
        var query = _dbContext.GroupDetails.AsNoTracking().
            Where(g => g.OwnerId == ownerId && !g.IsDeleted
            && g.ParentGroupId == parentGroupId && g.GroupType == groupType);

        return await query.ToListAsync(ctx);
    }

    public async Task<GroupDetails?> GetParentGroupDetails(long ownerId, long childGroupId, CancellationToken ctx)
    {
        var parentId = await _dbContext.GroupDetails.AsNoTracking().
            Where(g => g.OwnerId == ownerId && !g.IsDeleted
            && g.GroupId == childGroupId).Select(g => g.ParentGroupId).FirstOrDefaultAsync(ctx);

        if (parentId == null) return null;

        return await GetGroupDetailsById(parentId ?? 0, ctx);
    }

    public async Task<GroupDetails?> GetGroupDetailsById(long groupId, CancellationToken ctx)
    {
        return await _dbContext.GroupDetails.AsNoTracking()
            .Where(g => g.GroupId == groupId && !g.IsDeleted)
            .FirstOrDefaultAsync(ctx);
    }
}
