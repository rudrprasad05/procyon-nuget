namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents a provider-neutral email message.
/// </summary>
public sealed record EmailMessage
{
    /// <summary>
    /// Gets the sender address.
    /// </summary>
    public EmailAddress? From { get; init; }

    /// <summary>
    /// Gets the primary recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> To { get; init; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets the carbon-copy recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> Cc { get; init; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets the blind-carbon-copy recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> Bcc { get; init; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets the reply-to recipients.
    /// </summary>
    public IReadOnlyCollection<EmailAddress> ReplyTo { get; init; } = Array.Empty<EmailAddress>();

    /// <summary>
    /// Gets the message subject.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Gets the HTML body.
    /// </summary>
    public string? HtmlBody { get; init; }

    /// <summary>
    /// Gets the plain-text body.
    /// </summary>
    public string? TextBody { get; init; }

    /// <summary>
    /// Gets the attachments.
    /// </summary>
    public IReadOnlyCollection<EmailAttachment> Attachments { get; init; } = Array.Empty<EmailAttachment>();

    /// <summary>
    /// Gets the custom headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets provider-visible tags.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the requested priority.
    /// </summary>
    public EmailPriority Priority { get; init; } = EmailPriority.Normal;

    /// <summary>
    /// Gets the requested send time.
    /// </summary>
    public DateTimeOffset? SendAt { get; init; }

    /// <summary>
    /// Gets the idempotency key.
    /// </summary>
    public string? IdempotencyKey { get; init; }

    /// <summary>
    /// Gets application metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
