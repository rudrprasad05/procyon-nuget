namespace Procyon.Email.Configuration;

/// <summary>
/// Configures operational logging behaviour for email sending.
/// </summary>
public sealed class EmailLoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether recipient addresses may be logged.
    /// </summary>
    public bool LogRecipients { get; set; }
}
