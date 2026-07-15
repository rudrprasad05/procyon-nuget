namespace Procyon.Email.Configuration;

/// <summary>
/// Configures provider-neutral retry behaviour.
/// </summary>
public sealed class EmailRetryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether retries are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of send attempts.
    /// </summary>
    public int MaximumAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial retry delay in seconds.
    /// </summary>
    public int InitialDelaySeconds { get; set; } = 2;

    /// <summary>
    /// Gets or sets a value indicating whether exponential backoff should be used.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether retrying requires an idempotency key.
    /// </summary>
    public bool RequireIdempotencyKey { get; set; } = true;
}
