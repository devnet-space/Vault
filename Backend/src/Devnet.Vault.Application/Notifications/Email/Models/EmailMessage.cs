namespace Devnet.Vault.Application.Notifications.Email.Models;

public sealed class EmailMessage
{
    public List<string> To { get; set; } = [];
    public List<string> Cc { get; set; } = [];
    public List<string> Bcc { get; set; } = [];

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;

    public List<EmailAttachment> Attachments { get; set; } = [];
}