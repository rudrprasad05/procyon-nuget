using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;

namespace Procyon.Email.Utilities;

internal static class EmailAddressConverter
{
    public static EmailAddress? ToEmailAddress(EmailAddressOptions? options)
    {
        return string.IsNullOrWhiteSpace(options?.Address)
            ? null
            : new EmailAddress(options.Address, options.Name);
    }
}
