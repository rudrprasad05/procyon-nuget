namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents the provider-neutral result of an email send operation.
/// </summary>
public sealed record EmailSendResult
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
    /// Gets the selected provider name.
    /// </summary>
    public string? ProviderName { get; init; }

    /// <summary>
    /// Gets the provider message identifier when available.
    /// </summary>
    public string? ProviderMessageId { get; init; }

    /// <summary>
    /// Gets a provider-neutral error code when available.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Gets a provider-neutral error message when available.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the number of attempted sends.
    /// </summary>
    public int AttemptCount { get; init; } = 1;

    /// <summary>
    /// Creates an accepted send result.
    /// </summary>
    /// <param name="providerName">The selected provider name.</param>
    /// <param name="providerMessageId">The provider message identifier.</param>
    /// <param name="attemptCount">The number of attempted sends.</param>
    /// <returns>An accepted send result.</returns>
    public static EmailSendResult Accepted(
        string providerName,
        string? providerMessageId,
        int attemptCount = 1)
    {
        return new EmailSendResult
        {
            Succeeded = true,
            Status = EmailSendStatus.Accepted,
            ProviderName = providerName,
            ProviderMessageId = providerMessageId,
            AttemptCount = attemptCount
        };
    }

    /// <summary>
    /// Creates a failed send result.
    /// </summary>
    /// <param name="providerName">The selected provider name.</param>
    /// <param name="status">The provider-neutral failure status.</param>
    /// <param name="errorCode">The provider-neutral error code.</param>
    /// <param name="errorMessage">The provider-neutral error message.</param>
    /// <param name="attemptCount">The number of attempted sends.</param>
    /// <returns>A failed send result.</returns>
    public static EmailSendResult Failed(
        string? providerName,
        EmailSendStatus status,
        string? errorCode,
        string? errorMessage,
        int attemptCount = 1)
    {
        return new EmailSendResult
        {
            Succeeded = false,
            Status = status,
            ProviderName = providerName,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            AttemptCount = attemptCount
        };
    }
}
