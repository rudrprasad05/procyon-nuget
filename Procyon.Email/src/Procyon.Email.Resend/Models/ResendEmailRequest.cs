namespace Procyon.Email.Resend.Models;

internal sealed record ResendEmailRequest(
    ResendEmailRecipient From,
    IReadOnlyCollection<ResendEmailRecipient> To,
    IReadOnlyCollection<ResendEmailRecipient> Cc,
    IReadOnlyCollection<ResendEmailRecipient> Bcc,
    IReadOnlyCollection<ResendEmailRecipient> ReplyTo,
    string Subject,
    string? HtmlBody,
    string? TextBody,
    IReadOnlyCollection<ResendEmailAttachment> Attachments,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Tags,
    DateTimeOffset? SendAt,
    string? IdempotencyKey);
