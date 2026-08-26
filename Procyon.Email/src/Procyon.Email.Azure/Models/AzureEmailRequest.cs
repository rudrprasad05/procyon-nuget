namespace Procyon.Email.Azure.Models;

internal sealed record AzureEmailRequest(
    string SenderAddress,
    IReadOnlyCollection<AzureEmailRecipient> To,
    IReadOnlyCollection<AzureEmailRecipient> Cc,
    IReadOnlyCollection<AzureEmailRecipient> Bcc,
    IReadOnlyCollection<AzureEmailRecipient> ReplyTo,
    string Subject,
    string? HtmlBody,
    string? TextBody,
    IReadOnlyCollection<AzureEmailAttachment> Attachments,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Tags,
    string? IdempotencyKey);
