namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents an invalid Procyon.Email configuration state.
/// </summary>
public class EmailConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfigurationException" /> class.
    /// </summary>
    /// <param name="message">The provider-neutral configuration error message.</param>
    public EmailConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailConfigurationException" /> class.
    /// </summary>
    /// <param name="message">The provider-neutral configuration error message.</param>
    /// <param name="innerException">The exception that caused the configuration error.</param>
    public EmailConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
