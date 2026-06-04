using Devnet.Vault.Application.Features.Account.Interfaces.Repositories;
using Devnet.Vault.Domain.Entities.Identity;
using Devnet.Vault.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.Account;


internal sealed class UserRepository(AppDbContext _dbContext) : IUserRepository
{
    public async Task<UserDetails?> GetUserDetailsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<string?> GetUserSpecificEncryptionKeyAsync(long userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AsNoTracking()
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .Select(u => u.UserSecretKey)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserDetails?> GetUserDetailsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted, cancellationToken);
    }

    public async Task<UserDetails?> GetUserDetailsByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> UpdateUserDetailsAsync(long userId, long updatedBy, string? profileUrl, string? name, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Users.Where(u => u.UserId == userId && !u.IsDeleted).ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.Name, u => name ?? u.Name)
                    .SetProperty(u => u.ProfileUrl, u => profileUrl ?? u.ProfileUrl)
                    .SetProperty(u => u.UpdatedBy, u => updatedBy)
                    .SetProperty(u => u.UpdatedDate, u => DateTime.UtcNow),
            cancellationToken);

        return result > 0;
    }

    public async Task<bool> UpdateUserContactDetailsAsync(long userId, long updatedBy, string? email, string? phoneNumber, int? countryId, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Users.Where(u => u.UserId == userId && !u.IsDeleted).ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.Email, u => email ?? u.Email)
                    .SetProperty(u => u.PhoneNumber, u => phoneNumber ?? u.PhoneNumber)
                    .SetProperty(u => u.CountryId, u => countryId ?? u.CountryId)
                    .SetProperty(u => u.UpdatedBy, u => updatedBy)
                    .SetProperty(u => u.UpdatedDate, u => DateTime.UtcNow),
            cancellationToken);

        return result > 0;
    }

    public async Task<bool> DeactivateUserAsync(long userId, long deactivatedBy, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var affectedRows = await _dbContext.Users
                .Where(u => u.UserId == userId && !u.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeactivated, true)
                    .SetProperty(u => u.DeactivatedAt, utcNow)
                    .SetProperty(u => u.DeactivatedBy, deactivatedBy),
                    cancellationToken);

            if (affectedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            // deactivate related sessions/logins
            await _dbContext.UserLogins
                .Where(u => u.UserId == userId && u.ExpiryDate > utcNow)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.ExpiryDate, utcNow),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(long userId, long deletedBy, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Soft delete user
            var affectedRows = await _dbContext.Users
                .Where(u => u.UserId == userId && !u.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.IsDeleted, true)
                    .SetProperty(u => u.DeletedDate, utcNow)
                    .SetProperty(u => u.DeletedBy, deletedBy),
                    cancellationToken);

            // 2. Revoke active logins/sessions
            var loginAffectedRows = await _dbContext.UserLogins
                .Where(u => u.UserId == userId && u.ExpiryDate > utcNow)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.ExpiryDate, utcNow),
                    cancellationToken);

            if (affectedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
