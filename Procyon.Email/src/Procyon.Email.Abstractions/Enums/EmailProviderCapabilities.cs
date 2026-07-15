namespace Procyon.Email.Abstractions;

/// <summary>
/// Describes optional email features supported by a provider.
/// </summary>
[Flags]
public enum EmailProviderCapabilities
{
    /// <summary>
    /// Indicates that no optional provider features are available.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates support for HTML email bodies.
    /// </summary>
    HtmlBody = 1 << 0,

    /// <summary>
    /// Indicates support for plain-text email bodies.
    /// </summary>
    TextBody = 1 << 1,

    /// <summary>
    /// Indicates support for file attachments.
    /// </summary>
    Attachments = 1 << 2,

    /// <summary>
    /// Indicates support for inline attachments.
    /// </summary>
    InlineAttachments = 1 << 3,

    /// <summary>
    /// Indicates support for carbon-copy recipients.
    /// </summary>
    Cc = 1 << 4,

    /// <summary>
    /// Indicates support for blind-carbon-copy recipients.
    /// </summary>
    Bcc = 1 << 5,

    /// <summary>
    /// Indicates support for reply-to recipients.
    /// </summary>
    ReplyTo = 1 << 6,

    /// <summary>
    /// Indicates support for custom headers.
    /// </summary>
    CustomHeaders = 1 << 7,

    /// <summary>
    /// Indicates support for provider-visible message tags.
    /// </summary>
    Tags = 1 << 8,

    /// <summary>
    /// Indicates support for scheduled sending.
    /// </summary>
    ScheduledSending = 1 << 9,

    /// <summary>
    /// Indicates support for batch sending.
    /// </summary>
    BatchSending = 1 << 10,

    /// <summary>
    /// Indicates support for idempotency keys.
    /// </summary>
    Idempotency = 1 << 11,

    /// <summary>
    /// Indicates support for provider delivery webhooks.
    /// </summary>
    Webhooks = 1 << 12
}
