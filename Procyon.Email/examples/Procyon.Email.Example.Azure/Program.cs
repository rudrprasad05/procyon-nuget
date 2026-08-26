using System.Net;
using System.Text;
using Procyon.Email.Abstractions;
using Procyon.Email.Azure.DependencyInjection;
using Procyon.Email.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load(Path.Combine(builder.Environment.ContentRootPath, ".env"));
builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddProcyonEmail(builder.Configuration)
    .UseAzure();

var app = builder.Build();

app.MapGet("/", () => Results.Content(RenderPage(), "text/html; charset=utf-8"));

app.MapPost("/send", async (
    HttpContext context,
    IEmailSender emailSender,
    CancellationToken cancellationToken) =>
{
    var form = await context.Request.ReadFormAsync(cancellationToken);

    try
    {
        var request = ComposeRequest.FromForm(form);
        var message = BuildMessage(emailSender, request);
        var result = await emailSender.SendAsync(message, cancellationToken);

        var feedback = result.Succeeded
            ? SendFeedback.Success(
                "Email accepted",
                $"Provider {result.ProviderName} accepted the message with id {result.ProviderMessageId ?? "not supplied"}.")
            : SendFeedback.Warning(
                "Email was not accepted",
                $"{result.Status}: {result.ErrorMessage ?? result.ErrorCode ?? "The provider returned a failure."}");

        return Results.Content(RenderPage(request, feedback), "text/html; charset=utf-8");
    }
    catch (EmailUnsupportedFeatureException exception)
    {
        return Results.Content(
            RenderPage(ComposeRequest.FromForm(form), SendFeedback.Error("Unsupported email feature", exception.Message)),
            "text/html; charset=utf-8");
    }
    catch (EmailValidationException exception)
    {
        return Results.Content(
            RenderPage(ComposeRequest.FromForm(form), SendFeedback.Error("Validation failed", exception.Message)),
            "text/html; charset=utf-8");
    }
    catch (EmailConfigurationException exception)
    {
        return Results.Content(
            RenderPage(ComposeRequest.FromForm(form), SendFeedback.Error("Configuration failed", exception.Message)),
            "text/html; charset=utf-8");
    }
});

app.Run();

static EmailMessage BuildMessage(IEmailSender emailSender, ComposeRequest request)
{
    var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
     ? Guid.NewGuid().ToString()
     : request.IdempotencyKey.Trim();

    var builder = emailSender.Create()
        .To(new EmailAddress(request.ToAddress, request.ToName))
        .Subject(request.Subject)
        .HtmlBody(CreateHtmlBody(request))
        .TextBody(CreateTextBody(request))
        .Priority(request.Priority)
        .IdempotencyKey(idempotencyKey)
        .Header("X-Procyon-Example", "email-compose-azure")
        .Tag("source", "procyon-email-example-azure")
        .Tag("priority", request.Priority.ToString().ToLowerInvariant())
        .Metadata("example", "Procyon.Email.Example.Azure")
        .Metadata("requested-by", "interactive-form");

    AddAddresses(request.CcAddresses, builder.Cc);
    AddAddresses(request.BccAddresses, builder.Bcc);

    if (!string.IsNullOrWhiteSpace(request.ReplyToAddress))
    {
        builder.ReplyTo(new EmailAddress(request.ReplyToAddress, request.ReplyToName));
    }

    if (request.IncludeAttachment)
    {
        var bytes = Encoding.UTF8.GetBytes(CreateAttachmentText(request, idempotencyKey));
        builder.Attachment(new EmailAttachment(
            "procyon-email-example-azure.txt",
            "text/plain",
            bytes));
    }

    return builder.Build();
}

static void AddAddresses(string addresses, Func<EmailAddress, IEmailMessageBuilder> add)
{
    foreach (var value in SplitAddresses(addresses))
    {
        add(new EmailAddress(value));
    }
}

static IReadOnlyCollection<string> SplitAddresses(string addresses)
{
    return addresses
        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .ToArray();
}

static string CreateHtmlBody(ComposeRequest request)
{
    var greeting = string.IsNullOrWhiteSpace(request.ToName)
        ? "Hello,"
        : $"Hello {WebUtility.HtmlEncode(request.ToName)},";
    var body = WebUtility.HtmlEncode(request.Message)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace("\n", "<br>", StringComparison.Ordinal);

    return $$"""
        <!doctype html>
        <html>
        <body style="font-family: Arial, sans-serif; color: #17202a; line-height: 1.5;">
          <p>{{greeting}}</p>
          <p>{{body}}</p>
          <p>Regards,<br>Procyon Email Example (Azure)</p>
        </body>
        </html>
        """;
}

static string CreateTextBody(ComposeRequest request)
{
    var greeting = string.IsNullOrWhiteSpace(request.ToName)
        ? "Hello,"
        : $"Hello {request.ToName},";

    return $"""
        {greeting}

        {request.Message}

        Regards,
        Procyon Email Example (Azure)
        """;
}

static string CreateAttachmentText(ComposeRequest request, string idempotencyKey)
{
    return $"""
        Procyon.Email example message (Azure)

        To: {request.ToAddress}
        Subject: {request.Subject}
        Priority: {request.Priority}
        Idempotency key: {idempotencyKey}
        Created UTC: {DateTimeOffset.UtcNow:O}
        """;
}

static string RenderPage(ComposeRequest? values = null, SendFeedback? feedback = null)
{
    values ??= ComposeRequest.Default;
    var priorityOptions = RenderPriorityOptions(values.Priority);
    var attachmentChecked = values.IncludeAttachment ? " checked" : string.Empty;
    var feedbackHtml = feedback is null
        ? string.Empty
        : $$"""
          <section class="feedback {{feedback.Kind}}" role="status">
            <strong>{{Html(feedback.Title)}}</strong>
            <span>{{Html(feedback.Message)}}</span>
          </section>
          """;

    return $$"""
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Procyon.Email Example (Azure)</title>
          <style>
            :root {
              color-scheme: light;
              --bg: #f6f7f9;
              --panel: #ffffff;
              --text: #17202a;
              --muted: #5d6d7e;
              --line: #d7dde5;
              --accent: #0078d4;
              --accent-dark: #005a9e;
              --danger: #b42318;
              --warn: #8a5a00;
              --success: #146c43;
            }

            * {
              box-sizing: border-box;
            }

            body {
              margin: 0;
              background: var(--bg);
              color: var(--text);
              font-family: Arial, sans-serif;
              font-size: 16px;
              line-height: 1.45;
            }

            header {
              border-bottom: 1px solid var(--line);
              background: var(--panel);
            }

            .shell {
              width: min(1120px, calc(100% - 32px));
              margin: 0 auto;
            }

            .topbar {
              display: flex;
              align-items: center;
              justify-content: space-between;
              gap: 16px;
              min-height: 72px;
            }

            .brand {
              display: flex;
              align-items: center;
              gap: 12px;
              min-width: 0;
            }

            .mark {
              display: grid;
              place-items: center;
              width: 40px;
              height: 40px;
              border-radius: 8px;
              background: #0078d4;
              color: #fff;
              font-weight: 700;
            }

            h1 {
              margin: 0;
              font-size: 1.2rem;
              font-weight: 700;
            }

            .subtitle {
              margin: 2px 0 0;
              color: var(--muted);
              font-size: .92rem;
            }

            main {
              padding: 28px 0 40px;
            }

            form {
              display: grid;
              grid-template-columns: minmax(0, 2fr) minmax(280px, 1fr);
              gap: 20px;
              align-items: start;
            }

            .section {
              background: var(--panel);
              border: 1px solid var(--line);
              border-radius: 8px;
              padding: 20px;
            }

            .stack {
              display: grid;
              gap: 16px;
            }

            h2 {
              margin: 0 0 14px;
              font-size: 1rem;
            }

            label {
              display: grid;
              gap: 6px;
              color: var(--text);
              font-weight: 600;
            }

            label span {
              color: var(--muted);
              font-size: .88rem;
              font-weight: 400;
            }

            input,
            textarea,
            select {
              width: 100%;
              border: 1px solid var(--line);
              border-radius: 6px;
              padding: 10px 12px;
              color: var(--text);
              background: #fff;
              font: inherit;
            }

            textarea {
              min-height: 210px;
              resize: vertical;
            }

            input:focus,
            textarea:focus,
            select:focus {
              outline: 3px solid rgba(0, 120, 212, .18);
              border-color: var(--accent);
            }

            .grid {
              display: grid;
              grid-template-columns: repeat(2, minmax(0, 1fr));
              gap: 14px;
            }

            .check {
              display: flex;
              align-items: flex-start;
              gap: 10px;
              font-weight: 600;
            }

            .check input {
              width: 18px;
              height: 18px;
              margin-top: 3px;
            }

            .actions {
              display: flex;
              justify-content: flex-end;
              padding-top: 4px;
            }

            button {
              border: 0;
              border-radius: 6px;
              padding: 11px 18px;
              min-height: 44px;
              color: #fff;
              background: var(--accent);
              font: inherit;
              font-weight: 700;
              cursor: pointer;
            }

            button:hover {
              background: var(--accent-dark);
            }

            .feedback {
              display: grid;
              gap: 4px;
              margin-bottom: 18px;
              border: 1px solid var(--line);
              border-left-width: 5px;
              border-radius: 8px;
              background: #fff;
              padding: 14px 16px;
            }

            .feedback.success {
              border-left-color: var(--success);
            }

            .feedback.warning {
              border-left-color: var(--warn);
            }

            .feedback.error {
              border-left-color: var(--danger);
            }

            .note {
              margin: 0;
              color: var(--muted);
              font-size: .9rem;
            }

            @media (max-width: 860px) {
              form {
                grid-template-columns: 1fr;
              }

              .grid {
                grid-template-columns: 1fr;
              }

              .topbar {
                align-items: flex-start;
                flex-direction: column;
                justify-content: center;
                padding: 14px 0;
              }
            }
          </style>
        </head>
        <body>
          <header>
            <div class="shell topbar">
              <div class="brand">
                <div class="mark" aria-hidden="true">P</div>
                <div>
                  <h1>Procyon.Email Example (Azure)</h1>
                  <p class="subtitle">Compose a professional message and send it through Azure Communication Services.</p>
                </div>
              </div>
              <p class="subtitle">Provider: Azure</p>
            </div>
          </header>
          <main class="shell">
            {{feedbackHtml}}
            <form method="post" action="/send">
              <section class="section stack">
                <div>
                  <h2>Message</h2>
                  <p class="note">Enter a real recipient address, then write the message exactly as you want it delivered.</p>
                </div>
                <div class="grid">
                  <label>
                    Recipient email
                    <input name="toAddress" type="email" autocomplete="email" required value="{{Attr(values.ToAddress)}}">
                  </label>
                  <label>
                    Recipient name
                    <input name="toName" autocomplete="name" value="{{Attr(values.ToName)}}">
                  </label>
                </div>
                <label>
                  Subject
                  <input name="subject" required maxlength="160" value="{{Attr(values.Subject)}}">
                </label>
                <label>
                  Professional email prompt
                  <span>Use clear context, requested action, and any deadline or next step.</span>
                  <textarea name="message" required>{{Html(values.Message)}}</textarea>
                </label>
              </section>

              <aside class="section stack">
                <div>
                  <h2>Delivery features</h2>
                  <p class="note">These fields demonstrate Procyon.Email builder features and provider-neutral validation.</p>
                </div>
                <label>
                  CC recipients
                  <span>Separate multiple addresses with commas.</span>
                  <input name="ccAddresses" value="{{Attr(values.CcAddresses)}}">
                </label>
                <label>
                  BCC recipients
                  <input name="bccAddresses" value="{{Attr(values.BccAddresses)}}">
                </label>
                <div class="grid">
                  <label>
                    Reply-to email
                    <input name="replyToAddress" type="email" value="{{Attr(values.ReplyToAddress)}}">
                  </label>
                  <label>
                    Reply-to name
                    <input name="replyToName" value="{{Attr(values.ReplyToName)}}">
                  </label>
                </div>
                <label>
                  Priority
                  <select name="priority">{{priorityOptions}}</select>
                </label>
                <label>
                  Idempotency key
                  <span>Leave empty to generate one for this send.</span>
                  <input name="idempotencyKey" value="{{Attr(values.IdempotencyKey)}}">
                </label>
                <label class="check">
                  <input name="includeAttachment" type="checkbox" value="true"{{attachmentChecked}}>
                  <span>Attach a generated send summary</span>
                </label>
                <div class="actions">
                  <button type="submit">Send email</button>
                </div>
              </aside>
            </form>
          </main>
        </body>
        </html>
        """;
}

static string RenderPriorityOptions(EmailPriority selected)
{
    return string.Join(
        Environment.NewLine,
        Enum.GetValues<EmailPriority>().Select(priority =>
        {
            var selectedAttribute = priority == selected ? " selected" : string.Empty;
            return $"""<option value="{priority}"{selectedAttribute}>{priority}</option>""";
        }));
}

static string Html(string? value)
{
    return WebUtility.HtmlEncode(value ?? string.Empty);
}

static string Attr(string? value)
{
    return Html(value).Replace("\"", "&quot;", StringComparison.Ordinal);
}

internal sealed record ComposeRequest(
    string ToAddress,
    string ToName,
    string CcAddresses,
    string BccAddresses,
    string ReplyToAddress,
    string ReplyToName,
    string Subject,
    string Message,
    EmailPriority Priority,
    string IdempotencyKey,
    bool IncludeAttachment)
{
    public static ComposeRequest Default { get; } = new(
        ToAddress: string.Empty,
        ToName: string.Empty,
        CcAddresses: string.Empty,
        BccAddresses: string.Empty,
        ReplyToAddress: string.Empty,
        ReplyToName: string.Empty,
        Subject: "Follow-up from Procyon.Email (Azure)",
        Message: "Thank you for your time today. I wanted to share a concise follow-up and confirm the next step. Please reply when you have a chance, and I will coordinate the remaining details.",
        Priority: EmailPriority.Normal,
        IdempotencyKey: string.Empty,
        IncludeAttachment: true);

    public static ComposeRequest FromForm(IFormCollection form)
    {
        return new ComposeRequest(
            ToAddress: Value(form, "toAddress"),
            ToName: Value(form, "toName"),
            CcAddresses: Value(form, "ccAddresses"),
            BccAddresses: Value(form, "bccAddresses"),
            ReplyToAddress: Value(form, "replyToAddress"),
            ReplyToName: Value(form, "replyToName"),
            Subject: Value(form, "subject"),
            Message: Value(form, "message"),
            Priority: ParsePriority(Value(form, "priority")),
            IdempotencyKey: Value(form, "idempotencyKey"),
            IncludeAttachment: string.Equals(Value(form, "includeAttachment"), "true", StringComparison.OrdinalIgnoreCase));
    }

    private static string Value(IFormCollection form, string key)
    {
        return form.TryGetValue(key, out var value) ? value.ToString().Trim() : string.Empty;
    }

    private static EmailPriority ParsePriority(string value)
    {
        return Enum.TryParse<EmailPriority>(value, ignoreCase: true, out var priority)
            ? priority
            : EmailPriority.Normal;
    }
}

internal sealed record SendFeedback(string Kind, string Title, string Message)
{
    public static SendFeedback Success(string title, string message)
    {
        return new SendFeedback("success", title, message);
    }

    public static SendFeedback Warning(string title, string message)
    {
        return new SendFeedback("warning", title, message);
    }

    public static SendFeedback Error(string title, string message)
    {
        return new SendFeedback("error", title, message);
    }
}
