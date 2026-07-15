using Procyon.Email.Abstractions;

namespace Procyon.Email.Providers;

/// <summary>
/// Defines the provider contract implemented by Procyon.Email provider packages.
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Gets the provider name used for configuration selection.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the provider capabilities.
    /// </summary>
    EmailProviderCapabilities Capabilities { get; }

    /// <summary>
    /// Sends a provider-neutral email message through this provider.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A token that cancels the send operation.</param>
    /// <returns>The provider-level result.</returns>
    Task<EmailProviderResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
