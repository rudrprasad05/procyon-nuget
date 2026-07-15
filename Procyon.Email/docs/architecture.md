# Architecture

Procyon.Email keeps application code provider-independent. Applications inject `IEmailSender`, build or create an `EmailMessage`, and receive an `EmailSendResult`. Application-facing code does not reference Resend models, clients, responses, options, or exceptions.

## Public API boundary

`Procyon.Email.Abstractions` contains the stable application boundary:

- `IEmailSender`
- `IEmailMessageBuilder`
- `EmailMessage`
- `EmailAddress`
- `EmailAttachment`
- `EmailSendResult`
- `EmailSendStatus`
- `EmailPriority`
- `EmailProviderCapabilities`
- `UnsupportedFeatureBehaviour`
- provider-neutral email exceptions

Provider packages must translate provider errors and responses into these neutral results or exceptions before anything reaches application code.

## Package dependency graph

```text
Procyon.Email.Abstractions
    <- Procyon.Email
        <- Procyon.Email.Resend
```

`Procyon.Email.Abstractions` has no dependency on the core package or any provider. `Procyon.Email` depends only on the abstractions package. `Procyon.Email.Resend` depends on the core package and therefore transitively depends on abstractions.

## Provider model

`IEmailProvider` lives in `Procyon.Email`, not in `Procyon.Email.Abstractions`. The decision is intentional:

- Applications should not consume or implement provider infrastructure.
- Provider packages, including third-party packages, still need a stable contract to plug into core orchestration.
- Keeping `IEmailProvider` in core avoids expanding the application-facing abstractions package with infrastructure concepts.

Providers expose a configured name, capabilities, and a send operation. Core services resolve the selected provider explicitly from DI. There is no assembly scanning or reflection-based discovery.

## Capability model

Providers declare `EmailProviderCapabilities` as flags. The core capability validator checks requested message features, including attachments, inline attachments, CC, BCC, reply-to, custom headers, tags, scheduled sending, and idempotency. Unsupported features default to `Throw` so important fields are not silently discarded.

## Provider DTOs

Provider transport DTOs remain internal to provider packages. Resend request and response shapes are internal to `Procyon.Email.Resend`, and future providers should follow the same rule. This prevents application code from binding to provider-specific wire formats.

## Future providers

Future providers register explicitly with the fluent builder:

```csharp
builder.Services
    .AddProcyonEmail(builder.Configuration)
    .UseResend();
```

Amazon SES, Azure Communication Services Email, SMTP, and other providers should add equivalent provider package extensions without requiring application send code changes.
