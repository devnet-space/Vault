using Devnet.Vault.Domain.Constants.AppSettings;
using System.Security.Claims;

namespace Devnet.Vault.Api.Extensions;

/// <summary>
/// Extension to provide methods to help get necessary data related to  vault user request
/// </summary>
public static class VaultUsers
{
    /// <summary>
    /// Extract user id from logged user claim if avaialable else return 0 as default
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static long GetUserId(this HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;

        return 0;
    }

    /// <summary>
    /// Extract request source ip address with respecting reverse proxy if added
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string? GetRequestIpAddress(this HttpContext context)
    {
        return context.Connection?.RemoteIpAddress?.ToString() ?? // gets ip address from request itself
               context.Request.Headers[AppConstants.IP_ADDRESS_HEADER].FirstOrDefault() ?? "unknown"; // gets ip address from header if applicaiton behind reverse proxy
    }
}
