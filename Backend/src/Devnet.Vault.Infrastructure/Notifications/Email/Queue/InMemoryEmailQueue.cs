using Devnet.Vault.Application.Notifications.Email.Interfaces;
using Devnet.Vault.Application.Notifications.Email.Models;
using System.Threading.Channels;

namespace Devnet.Vault.Infrastructure.Notifications.Email.Queue;

internal sealed class InMemoryEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _queue = Channel.CreateUnbounded<EmailMessage>();

    public void Enqueue(EmailMessage message)
    {
        _queue.Writer.TryWrite(message);
    }

    public async Task<EmailMessage> DequeueAsync(CancellationToken ct)
    {
        return await _queue.Reader.ReadAsync(ct);
    }
}
