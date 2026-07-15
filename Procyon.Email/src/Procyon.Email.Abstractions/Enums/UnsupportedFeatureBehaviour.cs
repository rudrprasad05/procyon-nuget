namespace Procyon.Email.Abstractions;

/// <summary>
/// Controls how Procyon.Email handles message features unsupported by the selected provider.
/// </summary>
public enum UnsupportedFeatureBehaviour
{
    /// <summary>
    /// Throw an <see cref="EmailUnsupportedFeatureException" /> when a requested feature is unsupported.
    /// </summary>
    Throw = 0,

    /// <summary>
    /// Log a warning and continue without blocking the send operation.
    /// </summary>
    LogWarning = 1,

    /// <summary>
    /// Ignore unsupported features without logging.
    /// </summary>
    Ignore = 2
}
