# Providers

Provider registration is explicit and fluent:

```csharp
builder.Services
    .AddProcyonEmail(builder.Configuration)
    .UseResend();
```

Azure Communication Services Email registration uses the same pattern:

```csharp
builder.Services
    .AddProcyonEmail(builder.Configuration)
    .UseAzure();
```

The core package does not reference provider packages. Provider discovery is not based on assembly scanning.

## Resend

`Procyon.Email.Resend` provides provider registration, non-secret options, environment-variable validation, provider capabilities, request mapping, HTTP delivery through the Resend `/emails` endpoint, and response mapping.

## Azure

`Procyon.Email.Azure` provides provider registration, Azure Communication Services connection-string validation, provider capabilities, request mapping, HTTP delivery through the Azure `/emails:send` endpoint, HMAC request signing, and response mapping.

## Planned providers

Amazon SES support is planned as a separate provider package that maps Procyon messages to SES requests internally.

SMTP support is planned as a provider package focused on broad compatibility and development-friendly usage.

## Capability differences

Providers vary in support for attachments, inline attachments, tags, scheduled sending, idempotency, batch sending, and webhooks. Each provider declares `EmailProviderCapabilities`, and the core layer validates messages against those capabilities before sending. Resend and Azure currently support HTML and text bodies, attachments, CC, BCC, reply-to, custom headers, tags, and idempotency through their provider packages.

## Unsupported features

`UnsupportedFeatureBehaviour` controls validation outcomes:

- `Throw`: fail fast with `EmailUnsupportedFeatureException`.
- `LogWarning`: log a warning and continue.
- `Ignore`: continue silently.

The default is `Throw`.
