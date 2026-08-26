using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Azure.Configuration;
using Procyon.Email.Azure.Models;

namespace Procyon.Email.Azure.Mapping;

internal sealed class AzureRequestMapper(IOptionsMonitor<AzureEmailOptions> options)
{
    public AzureEmailRequest Map(EmailMessage message)
    {
        return new AzureEmailRequest(
            SenderAddress: message.From?.Address ?? options.CurrentValue.SenderEmail!,
            To: message.To.Select(MapAddress).ToArray(),
            Cc: message.Cc.Select(MapAddress).ToArray(),
            Bcc: message.Bcc.Select(MapAddress).ToArray(),
            ReplyTo: message.ReplyTo.Select(MapAddress).ToArray(),
            Subject: message.Subject!,
            HtmlBody: message.HtmlBody,
            TextBody: message.TextBody,
            Attachments: message.Attachments.Select(MapAttachment).ToArray(),
            Headers: MergeHeaders(message.Headers, message.Tags),
            Tags: message.Tags,
            IdempotencyKey: message.IdempotencyKey);
    }

    private static AzureEmailRecipient MapAddress(EmailAddress address)
    {
        return new AzureEmailRecipient(address.Address, address.Name);
    }

    private static AzureEmailAttachment MapAttachment(EmailAttachment attachment)
    {
        return new AzureEmailAttachment(
            attachment.FileName,
            attachment.ContentType,
            attachment.Content,
            attachment.ContentId,
            attachment.IsInline);
    }

    private static IReadOnlyDictionary<string, string> MergeHeaders(
        IReadOnlyDictionary<string, string> headers,
        IReadOnlyDictionary<string, string> tags)
    {
        if (tags.Count == 0)
        {
            return headers;
        }

        var merged = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
        foreach (var tag in tags)
        {
            merged[$"X-Procyon-Tag-{tag.Key}"] = tag.Value;
        }

        return merged;
    }
}
