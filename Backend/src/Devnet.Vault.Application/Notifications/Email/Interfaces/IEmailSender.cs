using Devnet.Vault.Application.Notifications.Email.Models;

namespace Devnet.Vault.Application.Notifications.Email.Interfaces;

public interface IEmailSender
{
    /// <summary>
    /// Send email as per given request
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendAsync(EmailMessage message);
}
