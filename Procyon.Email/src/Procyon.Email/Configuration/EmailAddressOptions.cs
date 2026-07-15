namespace Procyon.Email.Configuration;

/// <summary>
/// Represents an email address configured through application configuration.
/// </summary>
public sealed class EmailAddressOptions
{
    /// <summary>
    /// Gets or sets the configured email address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the optional display name.
    /// </summary>
    public string? Name { get; set; }
}
