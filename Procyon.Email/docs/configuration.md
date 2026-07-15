# Configuration

Configuration is rooted at `Procyon:Email`.

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
      "Logging": {
        "LogRecipients": false
      },
      "Resend": {
        "ApiBaseUrl": "https://api.resend.com",
        "ApiKeyEnvironmentVariable": "PROCYON_EMAIL_RESEND_API_KEY"
      }
    }
  }
}
```

## Current options

| Option | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Enables or disables send attempts. |
| `Provider` | none | Selected provider name, such as `Resend`. |
| `DefaultSender` | none | Default `From` address applied when a message does not specify one. |
| `DefaultReplyTo` | none | Default reply-to address applied when a message does not specify one. |
| `Delivery:TimeoutSeconds` | `30` | Provider send timeout value reserved for provider implementations. |
| `Delivery:MaximumRecipientsPerMessage` | `50` | Maximum total `To`, `Cc`, and `Bcc` recipients. |
| `Delivery:MaximumAttachmentSizeMb` | `20` | Maximum size for each attachment. |
| `Retry:Enabled` | `true` | Enables retry coordination. |
| `Retry:MaximumAttempts` | `3` | Maximum provider send attempts. |
| `Retry:InitialDelaySeconds` | `2` | Initial retry delay. |
| `Retry:UseExponentialBackoff` | `true` | Uses exponential delay growth. |
| `Retry:RequireIdempotencyKey` | `true` | Restricts retries to messages with an idempotency key. |
| `UnsupportedFeatureBehaviour` | `Throw` | Controls unsupported provider feature handling. |
| `Logging:LogRecipients` | `false` | Reserved switch for recipient logging. |
| `Resend:ApiBaseUrl` | `https://api.resend.com` | Resend API base URL. |
| `Resend:ApiKeyEnvironmentVariable` | `PROCYON_EMAIL_RESEND_API_KEY` | Environment variable name containing the Resend API key. |

## Validation

The core package validates selected provider, default sender, email address formats, positive timeout and delivery limit values, and retry values. The Resend provider validates its base URL, API-key environment variable name, and presence of the required environment variable when `Provider` is `Resend`.

Validation uses the .NET options pattern with `IOptions<T>`, `IOptionsMonitor<T>`, `IValidateOptions<T>`, and `ValidateOnStart`.

## Secret lookup

API key values are never accepted from normal configuration properties. For Resend, configuration may name the environment variable:

```json
{
  "Procyon": {
    "Email": {
      "Resend": {
        "ApiKeyEnvironmentVariable": "PROCYON_EMAIL_RESEND_API_KEY"
      }
    }
  }
}
```

The actual value must be set in the process environment, user secrets, deployment environment, or another environment-provider mechanism outside committed configuration files.
