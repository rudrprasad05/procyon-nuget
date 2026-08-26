namespace Procyon.Email.Azure.Configuration;

/// <summary>
/// Configures Azure Communication Services email provider settings.
/// </summary>
public sealed class AzureEmailOptions
{
    /// <summary>
    /// Gets the provider name used by Procyon.Email configuration.
    /// </summary>
    public const string ProviderName = "Azure";

    /// <summary>
    /// Gets the configuration section path for Azure provider settings.
    /// </summary>
    public const string SectionPath = "Procyon:Email:Azure";

    /// <summary>
    /// Gets or sets the Azure Communication Services connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the default sender email address from a verified Azure Email Communication Services domain.
    /// </summary>
    public string? SenderEmail { get; set; }

    /// <summary>
    /// Gets or sets the Azure Communication Services email API base URL.
    /// </summary>
    public string? ApiBaseUrl { get; set; }
}
