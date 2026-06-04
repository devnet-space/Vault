using Devnet.Vault.Application.Features.Shared.Logging.Interfaces;
using Serilog;

namespace Devnet.Vault.Infrastructure.Logging.Services;

internal sealed class SerilogAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger _logger;

    public SerilogAppLogger()
    {
        _logger = Log.ForContext<T>();
    }

    public void LogInformation(string message, params object[] args)
        => _logger.Information(message, args);

    public void LogWarning(string message, params object[] args)
        => _logger.Warning(message, args);

    public void LogError(Exception ex, string message, params object[] args)
        => _logger.Error(ex, message, args);
}
