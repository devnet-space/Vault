using Devnet.Vault.Application.Features.Shared.Cache.Interfaces.Services;
using Devnet.Vault.Application.Features.Shared.Otp.Interfaces.Services;

namespace Devnet.Vault.Infrastructure.Otp.Services;

internal sealed class OtpValidationService(ICacheService _cacheService) : IOtpValidationService
{
    public async Task<bool> ValidateAsync(string key, string otp)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(otp))
        {
            return false;
        }

        var storedOtp = await _cacheService.GetAsync<string>(key);
        if (!string.Equals(storedOtp, otp, StringComparison.Ordinal))
            return false;

        await _cacheService.RemoveAsync(key);
        return true;
    }
}
