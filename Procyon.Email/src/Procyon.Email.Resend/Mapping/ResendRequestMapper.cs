using Procyon.Email.Abstractions;
using Procyon.Email.Resend.Models;

namespace Procyon.Email.Resend.Mapping;

internal sealed class ResendRequestMapper
{
    public ResendEmailRequest Map(EmailMessage message)
    {
        return new ResendEmailRequest(
            From: MapAddress(message.From!),
            To: message.To.Select(MapAddress).ToArray(),
            Cc: message.Cc.Select(MapAddress).ToArray(),
            Bcc: message.Bcc.Select(MapAddress).ToArray(),
            ReplyTo: message.ReplyTo.Select(MapAddress).ToArray(),
            Subject: message.Subject!,
            HtmlBody: message.HtmlBody,
            TextBody: message.TextBody,
            Attachments: message.Attachments.Select(MapAttachment).ToArray(),
            Headers: message.Headers,
            Tags: message.Tags,
            SendAt: message.SendAt,
            IdempotencyKey: message.IdempotencyKey);
    }

    private static ResendEmailRecipient MapAddress(EmailAddress address)
    {
        return new ResendEmailRecipient(address.Address, address.Name);
    }

    private static ResendEmailAttachment MapAttachment(EmailAttachment attachment)
    {
        return new ResendEmailAttachment(
            attachment.FileName,
            attachment.ContentType,
            attachment.Content,
            attachment.ContentId,
            attachment.IsInline);
    }
}
