# Procyon.Email examples

## Procyon.Email.Example

`Procyon.Email.Example` is a small ASP.NET Core application that demonstrates provider-neutral email composition and delivery through `IEmailSender`.

The example shows:

- `AddProcyonEmail(builder.Configuration).UseResend()`
- configuration binding from `Procyon:Email`
- environment-backed Resend API key configuration
- default sender and reply-to addresses
- recipient, CC, BCC, and reply-to fields
- HTML and plain-text message bodies
- priority, idempotency keys, custom headers, tags, metadata, and a generated attachment
- validation and provider error handling

Run it from the repository root:

```bash
dotnet run --project Procyon.Email/examples/Procyon.Email.Example/Procyon.Email.Example.csproj
```

Open `http://localhost:5294` and enter the recipient email address you want to test.

The example includes `.env.example` with dummy values. Copy those values into your local environment or local `.env` file, then replace `PROCYON_EMAIL_RESEND_API_KEY` with a real provider key when testing real delivery. Do not commit real API keys.
