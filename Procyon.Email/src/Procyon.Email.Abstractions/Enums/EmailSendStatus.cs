namespace Procyon.Email.Abstractions;

/// <summary>
/// Describes the provider-neutral result status of a send request.
/// </summary>
public enum EmailSendStatus
{
    /// <summary>
    /// The provider accepted the email for delivery.
    /// </summary>
    Accepted = 0,

    /// <summary>
    /// The provider rejected the email request.
    /// </summary>
    Rejected = 1,

    /// <summary>
    /// The send operation failed before provider acceptance.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// The send operation was not attempted because email delivery is disabled.
    /// </summary>
    Disabled = 3
}
