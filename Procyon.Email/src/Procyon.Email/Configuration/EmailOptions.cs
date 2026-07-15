using Procyon.Email.Abstractions;

namespace Procyon.Email.Configuration;

/// <summary>
/// Configures provider-independent Procyon email behaviour.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// Gets the configuration section path for Procyon email options.
    /// </summary>
    public const string SectionPath = "Procyon:Email";

    /// <summary>
    /// Gets or sets a value indicating whether email delivery is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the selected provider name.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Gets or sets the default sender address.
    /// </summary>
    public EmailAddressOptions? DefaultSender { get; set; }

    /// <summary>
    /// Gets or sets the default reply-to address.
    /// </summary>
    public EmailAddressOptions? DefaultReplyTo { get; set; }

    /// <summary>
    /// Gets or sets delivery limits and timeouts.
    /// </summary>
    public EmailDeliveryOptions Delivery { get; set; } = new();

    /// <summary>
    /// Gets or sets retry behaviour.
    /// </summary>
    public EmailRetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Gets or sets unsupported-feature handling.
    /// </summary>
    public UnsupportedFeatureBehaviour UnsupportedFeatureBehaviour { get; set; } = UnsupportedFeatureBehaviour.Throw;

    /// <summary>
    /// Gets or sets operational logging behaviour.
    /// </summary>
    public EmailLoggingOptions Logging { get; set; } = new();
}
