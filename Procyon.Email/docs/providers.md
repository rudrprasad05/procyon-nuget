# Providers

Provider registration is explicit and fluent:

```csharp
builder.Services
    .AddProcyonEmail(builder.Configuration)
    .UseResend();
```

The core package does not reference provider packages. Provider discovery is not based on assembly scanning.

## Resend

`Procyon.Email.Resend` currently provides provider registration, non-secret options, environment-variable validation, provider capabilities, and internal request/response mapping scaffolding. Full Resend HTTP delivery is intentionally deferred.

## Planned providers

Amazon SES support is planned as a separate provider package that maps Procyon messages to SES requests internally.

Azure Communication Services Email support is planned as a separate provider package that maps Procyon messages to Azure-specific transport models internally.

SMTP support is planned as a provider package focused on broad compatibility and development-friendly usage.

## Capability differences

Providers vary in support for attachments, inline attachments, tags, scheduled sending, idempotency, batch sending, and webhooks. Each provider declares `EmailProviderCapabilities`, and the core layer validates messages against those capabilities before sending.

## Unsupported features

`UnsupportedFeatureBehaviour` controls validation outcomes:

- `Throw`: fail fast with `EmailUnsupportedFeatureException`.
- `LogWarning`: log a warning and continue.
- `Ignore`: continue silently.

The default is `Throw`.
