namespace Procyon.Email.Configuration;

/// <summary>
/// Configures provider-neutral delivery limits and timeouts.
/// </summary>
public sealed class EmailDeliveryOptions
{
    /// <summary>
    /// Gets or sets the send timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of recipients allowed per message.
    /// </summary>
    public int MaximumRecipientsPerMessage { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum attachment size in megabytes.
    /// </summary>
    public int MaximumAttachmentSizeMb { get; set; } = 20;
}
