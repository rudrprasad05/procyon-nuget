namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents an invalid provider-neutral email message.
/// </summary>
public class EmailValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidationException" /> class.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    public EmailValidationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidationException" /> class.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    /// <param name="innerException">The exception that caused the validation error.</param>
    public EmailValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
