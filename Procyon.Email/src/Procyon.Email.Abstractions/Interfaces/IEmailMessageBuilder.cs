namespace Procyon.Email.Abstractions;

/// <summary>
/// Builds provider-neutral <see cref="EmailMessage" /> instances.
/// </summary>
public interface IEmailMessageBuilder
{
    /// <summary>
    /// Sets the sender address.
    /// </summary>
    /// <param name="from">The sender address.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder From(EmailAddress from);

    /// <summary>
    /// Adds a primary recipient.
    /// </summary>
    /// <param name="to">The recipient address.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder To(EmailAddress to);

    /// <summary>
    /// Adds a carbon-copy recipient.
    /// </summary>
    /// <param name="cc">The recipient address.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Cc(EmailAddress cc);

    /// <summary>
    /// Adds a blind-carbon-copy recipient.
    /// </summary>
    /// <param name="bcc">The recipient address.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Bcc(EmailAddress bcc);

    /// <summary>
    /// Adds a reply-to recipient.
    /// </summary>
    /// <param name="replyTo">The reply-to address.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder ReplyTo(EmailAddress replyTo);

    /// <summary>
    /// Sets the message subject.
    /// </summary>
    /// <param name="subject">The message subject.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Subject(string subject);

    /// <summary>
    /// Sets the HTML body.
    /// </summary>
    /// <param name="htmlBody">The HTML body.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder HtmlBody(string htmlBody);

    /// <summary>
    /// Sets the plain-text body.
    /// </summary>
    /// <param name="textBody">The plain-text body.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder TextBody(string textBody);

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    /// <param name="attachment">The attachment to add.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Attachment(EmailAttachment attachment);

    /// <summary>
    /// Adds a custom header.
    /// </summary>
    /// <param name="name">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Header(string name, string value);

    /// <summary>
    /// Adds a provider-visible tag.
    /// </summary>
    /// <param name="name">The tag name.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Tag(string name, string value);

    /// <summary>
    /// Adds application metadata.
    /// </summary>
    /// <param name="name">The metadata name.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Metadata(string name, string value);

    /// <summary>
    /// Sets the requested send time.
    /// </summary>
    /// <param name="sendAt">The requested send time.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder SendAt(DateTimeOffset sendAt);

    /// <summary>
    /// Sets the idempotency key.
    /// </summary>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder IdempotencyKey(string idempotencyKey);

    /// <summary>
    /// Sets the message priority.
    /// </summary>
    /// <param name="priority">The message priority.</param>
    /// <returns>The current builder.</returns>
    IEmailMessageBuilder Priority(EmailPriority priority);

    /// <summary>
    /// Builds the provider-neutral message.
    /// </summary>
    /// <returns>The built message.</returns>
    EmailMessage Build();
}
