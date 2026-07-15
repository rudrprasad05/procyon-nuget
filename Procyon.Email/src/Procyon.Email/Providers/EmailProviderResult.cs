using Procyon.Email.Abstractions;

namespace Procyon.Email.Providers;

/// <summary>
/// Represents a provider-level result before it is mapped to the public send result.
/// </summary>
public sealed record EmailProviderResult
{
    /// <summary>
    /// Gets a value indicating whether the provider accepted the message.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets the provider-neutral send status.
    /// </summary>
    public EmailSendStatus Status { get; init; }

    /// <summary>
    /// Gets the provider message identifier when available.
    /// </summary>
    public string? ProviderMessageId { get; init; }

    /// <summary>
    /// Gets the provider-neutral error code when available.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets the provider-neutral error message when available.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates an accepted provider result.
    /// </summary>
    /// <param name="providerMessageId">The provider message identifier.</param>
    /// <returns>An accepted provider result.</returns>
    public static EmailProviderResult Accepted(string? providerMessageId)
    {
        return new EmailProviderResult
        {
            Succeeded = true,
            Status = EmailSendStatus.Accepted,
            ProviderMessageId = providerMessageId
        };
    }
}
