namespace Procyon.Email.Resend.Configuration;

/// <summary>
/// Configures non-secret Resend provider settings.
/// </summary>
public sealed class ResendEmailOptions
{
    /// <summary>
    /// Gets the provider name used by Procyon.Email configuration.
    /// </summary>
    public const string ProviderName = "Resend";

    /// <summary>
    /// Gets the configuration section path for Resend provider settings.
    /// </summary>
    public const string SectionPath = "Procyon:Email:Resend";

    /// <summary>
    /// Gets or sets the Resend API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.resend.com";

    /// <summary>
    /// Gets or sets the environment variable name containing the Resend API key.
    /// </summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "PROCYON_EMAIL_RESEND_API_KEY";
}
