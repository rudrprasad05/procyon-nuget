namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents a provider failure translated into a provider-neutral exception.
/// </summary>
public class EmailProviderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailProviderException" /> class.
    /// </summary>
    /// <param name="providerName">The provider that failed.</param>
    /// <param name="message">The provider-neutral failure message.</param>
    public EmailProviderException(string providerName, string message)
        : base(message)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailProviderException" /> class.
    /// </summary>
    /// <param name="providerName">The provider that failed.</param>
    /// <param name="message">The provider-neutral failure message.</param>
    /// <param name="innerException">The exception that caused the provider failure.</param>
    public EmailProviderException(string providerName, string message, Exception innerException)
        : base(message, innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the provider that failed.
    /// </summary>
    public string ProviderName { get; }
}
