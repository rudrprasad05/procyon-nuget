namespace Procyon.Email.Abstractions;

/// <summary>
/// Describes the provider-neutral delivery priority requested for an email message.
/// </summary>
public enum EmailPriority
{
    /// <summary>
    /// Uses the provider or application default priority.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Requests lower-priority delivery when the provider supports it.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Requests higher-priority delivery when the provider supports it.
    /// </summary>
    High = 2
}
