using Microsoft.AspNetCore.Http;

namespace Devnet.Vault.Application.Features.Shared.FileUpload.Models;

public sealed class FileUploadTask
{
    public Guid UploadId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}