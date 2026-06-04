namespace Devnet.Vault.Application.Notifications.Email.Models;

public sealed class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = string.Empty; // e.g. "application/pdf"
}
