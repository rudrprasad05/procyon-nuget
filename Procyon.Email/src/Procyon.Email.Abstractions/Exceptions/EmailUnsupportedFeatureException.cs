namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents a requested message feature that is not supported by the selected provider.
/// </summary>
public class EmailUnsupportedFeatureException : EmailValidationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailUnsupportedFeatureException" /> class.
    /// </summary>
    /// <param name="featureName">The unsupported feature name.</param>
    /// <param name="providerName">The selected provider name.</param>
    public EmailUnsupportedFeatureException(string featureName, string providerName)
        : base($"The selected email provider '{providerName}' does not support '{featureName}'.")
    {
        FeatureName = featureName;
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the unsupported feature name.
    /// </summary>
    public string FeatureName { get; }

    /// <summary>
    /// Gets the selected provider name.
    /// </summary>
    public string ProviderName { get; }
}
