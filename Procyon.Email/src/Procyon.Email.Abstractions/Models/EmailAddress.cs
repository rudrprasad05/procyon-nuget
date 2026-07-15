using System.Net.Mail;

namespace Procyon.Email.Abstractions;

/// <summary>
/// Represents a provider-neutral email address and optional display name.
/// </summary>
public sealed record EmailAddress
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddress" /> class.
    /// </summary>
    /// <param name="address">The email address.</param>
    /// <param name="name">The optional display name.</param>
    /// <exception cref="EmailValidationException">Thrown when the address is empty or invalid.</exception>
    public EmailAddress(string address, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new EmailValidationException("Email address is required.");
        }

        try
        {
            var parsed = new MailAddress(address, name);
            Address = parsed.Address;
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }
        catch (FormatException exception)
        {
            throw new EmailValidationException($"Email address '{address}' is invalid.", exception);
        }
    }

    /// <summary>
    /// Gets the normalized email address.
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Gets the optional display name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Returns a display-friendly address.
    /// </summary>
    /// <returns>The display name and address when a name is present; otherwise the address.</returns>
    public override string ToString()
    {
        return Name is null ? Address : $"{Name} <{Address}>";
    }
}
