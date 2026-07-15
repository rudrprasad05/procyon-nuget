# Procyon.Email

Procyon.Email is the provider-independent email package family for Procyon. Applications describe email messages through stable Procyon contracts and send them by injecting `IEmailSender`; provider-specific clients, DTOs, responses, configuration objects, and exceptions stay behind provider packages.

## Packages

| Package | Purpose |
| --- | --- |
| `Procyon.Email.Abstractions` | Provider-neutral contracts, models, result types, enums, and exceptions. |
| `Procyon.Email` | Provider-independent orchestration, configuration, validation, DI, provider resolution, and retry coordination. |
| `Procyon.Email.Resend` | Resend provider scaffolding for Procyon.Email. Full HTTP delivery is intentionally not implemented yet. |

## Dependency graph

```text
Procyon.Email.Abstractions
    <- Procyon.Email
        <- Procyon.Email.Resend
```

Consumers should normally install only the selected provider package:

```bash
dotnet add package Procyon.Email.Resend
```

That provider package transitively brings in `Procyon.Email` and `Procyon.Email.Abstractions`.

## Future usage shape

```csharp
using Procyon.Email.Abstractions;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Resend.DependencyInjection;

builder.Services
    .AddProcyonEmail(builder.Configuration)
    .UseResend();

public sealed class WelcomeEmail(IEmailSender emailSender)
{
    public Task SendAsync(CancellationToken cancellationToken)
    {
        var message = emailSender.Create()
            .To(new EmailAddress("user@example.com"))
            .Subject("Welcome")
            .HtmlBody("<p>Welcome.</p>")
            .Build();

        return emailSender.SendAsync(message, cancellationToken);
    }
}
```

## Configuration overview

Configuration is rooted at `Procyon:Email`:

```json
{
  "Procyon": {
    "Email": {
      "Enabled": true,
      "Provider": "Resend",
      "DefaultSender": {
        "Address": "no-reply@example.com",
        "Name": "Example"
      },
      "DefaultReplyTo": {
        "Address": "support@example.com",
        "Name": "Support"
      },
      "Delivery": {
        "TimeoutSeconds": 30,
        "MaximumRecipientsPerMessage": 50,
        "MaximumAttachmentSizeMb": 20
      },
      "Retry": {
        "Enabled": true,
        "MaximumAttempts": 3,
        "InitialDelaySeconds": 2,
        "UseExponentialBackoff": true,
        "RequireIdempotencyKey": true
      },
      "UnsupportedFeatureBehaviour": "Throw",
      "Resend": {
        "ApiBaseUrl": "https://api.resend.com",
        "ApiKeyEnvironmentVariable": "PROCYON_EMAIL_RESEND_API_KEY"
      }
    }
  }
}
```

API keys must come from environment variables only. The default Resend variable name is:

```env
PROCYON_EMAIL_RESEND_API_KEY=
```

The variable name may be configured, but the secret value must not be stored in `appsettings.json`, README examples, tests, logs, exceptions, or package artifacts.

## Current status

This task establishes architecture and scaffolding only. The Resend provider validates configuration, registers through DI, exposes provider capabilities, and contains internal request/response mapping placeholders. It does not send email over the Resend HTTP API yet and is not production-ready for delivery.

## Roadmap

1. Phase 1: architecture and scaffolding.
2. Phase 2: Resend sending implementation.
3. Phase 3: templates and development providers.
4. Phase 4: webhooks and delivery events.
5. Phase 5: Amazon SES.
6. Phase 6: Azure Communication Services Email.
7. Phase 7: queue and durable outbox.

## Documentation

- [Architecture](docs/architecture.md)
- [Configuration](docs/configuration.md)
- [Providers](docs/providers.md)
- [Security](docs/security.md)
- [Versioning](docs/versioning.md)
- [Roadmap](docs/roadmap.md)
- [Examples](examples/README.md)
