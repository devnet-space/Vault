using Devnet.Vault.Domain.Constants.AppKeys;

namespace Devnet.Vault.Api.Extensions;

/// <summary>
/// Added cqrs support implementation for allowing specific request controlled through config
/// </summary>
public static class Cors
{
    public static IServiceCollection AddCORSPolicy(this IServiceCollection _services, IConfiguration _config)
    {
        var allowedOrigins = _config
            .GetSection(ConfigKeys.CORS_ALLOWED_ORIGINS_KEY)
            .Get<string[]>() ?? [];

        _services.AddCors(options =>
        {
            options.AddPolicy(ConfigKeys.CORS_POLICY_NAME, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return _services;
    }
}
