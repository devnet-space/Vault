using Devnet.Vault.Application.Notifications.Email.Models;

namespace Devnet.Vault.Application.Notifications.Email.Interfaces;

public interface IEmailQueue
{
    /// <summary>
    /// Add emailmessage to in memory queue
    /// </summary>
    /// <param name="message"></param>
    void Enqueue(EmailMessage message);

    /// <summary>
    /// read the queue and remove it from queue
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<EmailMessage> DequeueAsync(CancellationToken ct);
}
