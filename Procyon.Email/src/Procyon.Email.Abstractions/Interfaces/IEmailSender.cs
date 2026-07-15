namespace Procyon.Email.Abstractions;

/// <summary>
/// Sends provider-neutral email messages.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Creates a provider-neutral message builder.
    /// </summary>
    /// <returns>A new message builder.</returns>
    IEmailMessageBuilder Create();

    /// <summary>
    /// Sends an email message using the configured provider.
    /// </summary>
    /// <param name="message">The provider-neutral email message.</param>
    /// <param name="cancellationToken">A token that cancels the send operation.</param>
    /// <returns>The provider-neutral send result.</returns>
    Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
