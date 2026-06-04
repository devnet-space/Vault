using Devnet.Vault.Application.Features.Auth.Interfaces.Repositories;
using Devnet.Vault.Domain.Entities.Identity;
using Devnet.Vault.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using static Devnet.Vault.Domain.Constants.Messages.ValidationMessages;

namespace Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.Authentication;

internal sealed class AuthRepository(AppDbContext _dbContext) : IAuthRepository
{
    /// <summary>
    /// Register a new user in the system
    /// </summary>
    public async Task<UserDetails?> RegisterNewUserAsync(UserDetails userDetails, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Users.Add(userDetails);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return userDetails;
        }
        catch (Exception ex)
        {
            _dbContext.ChangeTracker.Clear(); // Clear the change tracker to prevent inconsistent state
            throw new InvalidOperationException(AuthValidationMessages.REGISTRATION_FAILED, ex);
        }
    }

    /// <summary>
    /// Save user login details for session tracking
    /// </summary>
    public async Task<bool> SaveUserLoginDetailsAsync(UserLogins userLogins, CancellationToken cancellationToken)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            _dbContext.UserLogins.Add(userLogins);
            _dbContext.Users.Where(u => u.UserId == userLogins.UserId)
                .ExecuteUpdate(u => u.SetProperty(p => p.LastLoginDate, DateTime.UtcNow)
                .SetProperty(U => U.UpdatedDate, DateTime.UtcNow)
                .SetProperty(U => U.UpdatedBy, userLogins.UserId));
            var rowsAffected = await _dbContext.SaveChangesAsync(cancellationToken);
            if (rowsAffected > 0)
                await transaction.CommitAsync(cancellationToken);
            else
                await transaction.RollbackAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear(); // Clear the change tracker to prevent inconsistent state
            throw new InvalidOperationException(AuthValidationMessages.LOGIN_DETAILS_SAVE_FAILED, ex);
        }
    }

    /// <summary>
    /// Get user login by refresh token hash
    /// </summary>
    public async Task<UserLogins?> GetUserLoginByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.UserLogins
                .Where(x => x.RefreshTokenHash == refreshTokenHash && !x.IsRevoked)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(AuthValidationMessages.LOGIN_DETAILS_NOT_FOUND, ex);
        }
    }

    /// <summary>
    /// Revoke a user login session
    /// </summary>
    public async Task<bool> RevokeUserLoginAsync(long userLoginId, CancellationToken cancellationToken)
    {
        try
        {
            var userLogin = await _dbContext.UserLogins.FindAsync([userLoginId], cancellationToken: cancellationToken);
            if (userLogin == null)
                return false;

            userLogin.IsRevoked = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _dbContext.ChangeTracker.Clear();
            throw new InvalidOperationException(AuthValidationMessages.LOGIN_REVOKE_FAILED, ex);
        }
    }
}
