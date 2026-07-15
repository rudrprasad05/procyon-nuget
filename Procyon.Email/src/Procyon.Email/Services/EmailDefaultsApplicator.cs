using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;

namespace Procyon.Email.Services;

internal sealed class EmailDefaultsApplicator(IOptionsMonitor<EmailOptions> options)
{
    public EmailMessage Apply(EmailMessage message)
    {
        var current = options.CurrentValue;
        var from = message.From ?? ToEmailAddress(current.DefaultSender);
        var replyTo = message.ReplyTo;
        var defaultReplyTo = ToEmailAddress(current.DefaultReplyTo);
        if (replyTo.Count == 0 && defaultReplyTo is not null)
        {
            replyTo = [defaultReplyTo];
        }

        return message with
        {
            From = from,
            ReplyTo = replyTo
        };
    }

    private static EmailAddress? ToEmailAddress(EmailAddressOptions? options)
    {
        return string.IsNullOrWhiteSpace(options?.Address)
            ? null
            : new EmailAddress(options.Address, options.Name);
    }
}
