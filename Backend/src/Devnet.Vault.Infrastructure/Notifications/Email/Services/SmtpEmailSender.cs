using Devnet.Vault.Application.Configurations;
using Devnet.Vault.Application.Notifications.Email.Interfaces;
using Devnet.Vault.Application.Notifications.Email.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Devnet.Vault.Infrastructure.Notifications.Email.Services;

internal sealed class SmtpEmailSender(IOptions<EmailSettings> _options) : IEmailSender
{
    private readonly EmailSettings emailSettings = _options.Value;
    public async Task SendAsync(EmailMessage message)
    {
        var smtp = new SmtpClient(emailSettings.SmtpServer)
        {
            Port = emailSettings.Port,

            Credentials = new NetworkCredential(
               emailSettings.Username,
               emailSettings.Password),

            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(emailSettings.SenderEmail, emailSettings.SenderName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };

        // Multiple TO
        foreach (var to in message.To)
            mail.To.Add(to);

        // CC
        foreach (var cc in message.Cc)
            mail.CC.Add(cc);

        // BCC
        foreach (var bcc in message.Bcc)
            mail.Bcc.Add(bcc);

        // Attachments
        foreach (var att in message.Attachments)
        {
            var stream = new MemoryStream(att.Content);
            var attachment = new Attachment(stream, att.FileName, att.ContentType);
            mail.Attachments.Add(attachment);
        }

        await smtp.SendMailAsync(mail);
    }
}
