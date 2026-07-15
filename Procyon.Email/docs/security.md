# Security

## API keys

API keys and provider secrets must come from environment variables. Resend uses `PROCYON_EMAIL_RESEND_API_KEY` by default. Configuration may change the environment variable name, but the secret value must not be stored in `appsettings.json`, README examples, tests, exceptions, logs, diagnostic output, or generated files.

## Logging

Procyon.Email uses `Microsoft.Extensions.Logging`. Logs should include operational data such as provider name, recipient count, duration, retry count, provider message ID, and failure category. Logs must not include API keys, full email bodies, attachment contents, password reset links, verification tokens, authentication tokens, or sensitive headers.

Recipient logging is configurable and disabled by default because email addresses can be personal data.

## Email content

Email bodies commonly contain tokens, private links, account identifiers, and customer data. Provider implementations should avoid logging request payloads by default and should redact sensitive fields in diagnostics.

## Attachments

Attachments can contain sensitive content and can be large. The core package validates per-attachment size limits. Future provider implementations should avoid buffering unbounded content and should not log attachment bytes.

## Headers

Custom headers can contain correlation identifiers or sensitive application data. Provider implementations should treat headers as potentially sensitive and avoid broad header logging.

## Tokens and verification links

Password reset links, email verification links, and one-time tokens must be treated as secrets. They should not be emitted to logs, exceptions, telemetry attributes, or provider-neutral results.
