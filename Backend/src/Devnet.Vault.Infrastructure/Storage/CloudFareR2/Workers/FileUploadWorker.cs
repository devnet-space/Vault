using Devnet.Vault.Application.Features.Shared.Cache.Interfaces.Services;
using Devnet.Vault.Application.Features.Shared.FileUpload.DTOs;
using Devnet.Vault.Application.Features.Shared.FileUpload.Interfaces;
using Devnet.Vault.Application.Features.Shared.FileUpload.Models;
using Devnet.Vault.Application.Features.Shared.Logging.Interfaces;
using Devnet.Vault.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Devnet.Vault.Infrastructure.Storage.CloudFareR2.Workers;

internal sealed class FileUploadWorker(IFileUploadQueue _queue, IServiceScopeFactory _scopeFactory,
    IAppLogger<FileUploadWorker> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("R2 upload worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            FileUploadTask? uploadTask = null;
            try
            {
                uploadTask = await _queue.DequeueAsync(stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var uploadService = scope.ServiceProvider.GetRequiredService<IR2FileUploadService>();
                var cacheKey = GetCacheKey(uploadTask.UploadId);

                await cache.SetAsync(cacheKey, new UploadStatusResponse(uploadTask.UploadId, uploadTask.FileName, FileUploadStatus.Uploading, null, null, DateTime.UtcNow), TimeSpan.FromDays(1));

                var fileKey = await uploadService.UploadAsync(uploadTask, stoppingToken);

                await cache.SetAsync(cacheKey, new UploadStatusResponse(uploadTask.UploadId, uploadTask.FileName, FileUploadStatus.Completed, fileKey, null, DateTime.UtcNow, DateTime.UtcNow), TimeSpan.FromDays(1));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to R2");

                if (uploadTask is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    var failedStatus = new UploadStatusResponse(uploadTask.UploadId, uploadTask.FileName, FileUploadStatus.Failed, null, ex.Message, DateTime.UtcNow, DateTime.UtcNow);
                    await cache.SetAsync(GetCacheKey(uploadTask.UploadId), failedStatus, TimeSpan.FromDays(1));
                }

                await Task.Delay(2000, stoppingToken);
            }
        }
    }

    private static string GetCacheKey(Guid uploadId) => $"upload:{uploadId}";
}
