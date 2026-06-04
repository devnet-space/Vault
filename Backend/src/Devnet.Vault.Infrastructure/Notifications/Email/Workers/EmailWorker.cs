using Devnet.Vault.Application.Notifications.Email.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Devnet.Vault.Infrastructure.Notifications.Email.Workers;

internal sealed class EmailWorker(IEmailQueue _queue, IServiceScopeFactory _scopeFactory,
    ILogger<EmailWorker> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var email = await _queue.DequeueAsync(stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await sender.SendAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
