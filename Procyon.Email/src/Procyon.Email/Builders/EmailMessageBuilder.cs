using Procyon.Email.Abstractions;

namespace Procyon.Email.Builders;

internal sealed class EmailMessageBuilder : IEmailMessageBuilder
{
    private readonly List<EmailAddress> _to = [];
    private readonly List<EmailAddress> _cc = [];
    private readonly List<EmailAddress> _bcc = [];
    private readonly List<EmailAddress> _replyTo = [];
    private readonly List<EmailAttachment> _attachments = [];
    private readonly Dictionary<string, string> _headers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _tags = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    private EmailAddress? _from;
    private string? _subject;
    private string? _htmlBody;
    private string? _textBody;
    private EmailPriority _priority = EmailPriority.Normal;
    private DateTimeOffset? _sendAt;
    private string? _idempotencyKey;

    public IEmailMessageBuilder From(EmailAddress from)
    {
        _from = from;
        return this;
    }

    public IEmailMessageBuilder To(EmailAddress to)
    {
        _to.Add(to);
        return this;
    }

    public IEmailMessageBuilder Cc(EmailAddress cc)
    {
        _cc.Add(cc);
        return this;
    }

    public IEmailMessageBuilder Bcc(EmailAddress bcc)
    {
        _bcc.Add(bcc);
        return this;
    }

    public IEmailMessageBuilder ReplyTo(EmailAddress replyTo)
    {
        _replyTo.Add(replyTo);
        return this;
    }

    public IEmailMessageBuilder Subject(string subject)
    {
        _subject = subject;
        return this;
    }

    public IEmailMessageBuilder HtmlBody(string htmlBody)
    {
        _htmlBody = htmlBody;
        return this;
    }

    public IEmailMessageBuilder TextBody(string textBody)
    {
        _textBody = textBody;
        return this;
    }

    public IEmailMessageBuilder Attachment(EmailAttachment attachment)
    {
        _attachments.Add(attachment);
        return this;
    }

    public IEmailMessageBuilder Header(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    public IEmailMessageBuilder Tag(string name, string value)
    {
        _tags[name] = value;
        return this;
    }

    public IEmailMessageBuilder Metadata(string name, string value)
    {
        _metadata[name] = value;
        return this;
    }

    public IEmailMessageBuilder SendAt(DateTimeOffset sendAt)
    {
        _sendAt = sendAt;
        return this;
    }

    public IEmailMessageBuilder IdempotencyKey(string idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
        return this;
    }

    public IEmailMessageBuilder Priority(EmailPriority priority)
    {
        _priority = priority;
        return this;
    }

    public EmailMessage Build()
    {
        return new EmailMessage
        {
            From = _from,
            To = _to.ToArray(),
            Cc = _cc.ToArray(),
            Bcc = _bcc.ToArray(),
            ReplyTo = _replyTo.ToArray(),
            Subject = _subject,
            HtmlBody = _htmlBody,
            TextBody = _textBody,
            Attachments = _attachments.ToArray(),
            Headers = new Dictionary<string, string>(_headers, StringComparer.OrdinalIgnoreCase),
            Tags = new Dictionary<string, string>(_tags, StringComparer.OrdinalIgnoreCase),
            Priority = _priority,
            SendAt = _sendAt,
            IdempotencyKey = _idempotencyKey,
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.OrdinalIgnoreCase)
        };
    }
}
