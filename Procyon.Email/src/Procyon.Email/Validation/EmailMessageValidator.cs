using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;

namespace Procyon.Email.Validation;

internal sealed class EmailMessageValidator(IOptionsMonitor<EmailOptions> options)
{
    public void Validate(EmailMessage message)
    {
        if (message.From is null)
        {
            throw new EmailValidationException("Email sender is required.");
        }

        if (message.To.Count == 0)
        {
            throw new EmailValidationException("At least one primary email recipient is required.");
        }

        if (string.IsNullOrWhiteSpace(message.Subject))
        {
            throw new EmailValidationException("Email subject is required.");
        }

        if (string.IsNullOrWhiteSpace(message.HtmlBody) && string.IsNullOrWhiteSpace(message.TextBody))
        {
            throw new EmailValidationException("An HTML body or plain-text body is required.");
        }

        var delivery = options.CurrentValue.Delivery;
        var recipientCount = message.To.Count + message.Cc.Count + message.Bcc.Count;
        if (recipientCount > delivery.MaximumRecipientsPerMessage)
        {
            throw new EmailValidationException($"Email recipient count exceeds the configured limit of {delivery.MaximumRecipientsPerMessage}.");
        }

        var maximumAttachmentBytes = delivery.MaximumAttachmentSizeMb * 1024L * 1024L;
        foreach (var attachment in message.Attachments)
        {
            if (attachment.Content.Length > maximumAttachmentBytes)
            {
                throw new EmailValidationException($"Attachment '{attachment.FileName}' exceeds the configured size limit.");
            }
        }
    }
}
