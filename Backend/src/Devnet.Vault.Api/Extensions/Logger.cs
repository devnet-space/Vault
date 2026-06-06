using Serilog;

namespace Devnet.Vault.Api.Extensions;

/// <summary>
/// Extension to register serilog as primary logger with all configuration done through config
/// </summary>
public static class Logger
{
    public static WebApplicationBuilder ConfigureLogger(this WebApplicationBuilder builder)
    {

        builder.Host.UseSerilog((context, services, config) =>
        {
            config.ReadFrom.Configuration(builder.Configuration);
        });

        return builder;
    }
}
