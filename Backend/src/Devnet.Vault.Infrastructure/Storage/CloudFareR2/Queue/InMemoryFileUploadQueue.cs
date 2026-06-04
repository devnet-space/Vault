using Devnet.Vault.Application.Features.Shared.FileUpload.Interfaces;
using Devnet.Vault.Application.Features.Shared.FileUpload.Models;
using System.Threading.Channels;

namespace Devnet.Vault.Infrastructure.Storage.CloudFareR2.Queue;

internal sealed class InMemoryFileUploadQueue : IFileUploadQueue
{
    private readonly Channel<FileUploadTask> _queue = Channel.CreateUnbounded<FileUploadTask>();

    public void Enqueue(FileUploadTask uploadTask)
    {
        _queue.Writer.TryWrite(uploadTask);
    }

    public async Task<FileUploadTask> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
