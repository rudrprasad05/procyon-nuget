# Versioning

Procyon.Email starts at `0.1.0` to match the current repository package convention.

## 0.x policy

During the initial `0.x` lifecycle, package versions remain aligned across:

- `Procyon.Email.Abstractions`
- `Procyon.Email`
- `Procyon.Email.Resend`

Breaking changes may occur while the API is being stabilized, but they should be documented clearly in release notes.

## Breaking changes

Changes to `IEmailSender`, `EmailMessage`, provider-neutral result types, configuration keys, or package dependency direction are breaking changes. Provider-specific DTO changes are not application-facing when those DTOs remain internal.

## Dependency compatibility

`Procyon.Email.Resend` depends on `Procyon.Email`, and `Procyon.Email` depends on `Procyon.Email.Abstractions`. Provider packages should depend on the matching core package version during the initial lifecycle.

## Release and tag conventions

The existing repository currently has a generic `v0.1.0` tag. For future email-specific releases, use package-family tags to avoid ambiguity:

```text
procyon-email-v0.1.0
```

Do not create a tag for architecture-only scaffolding unless explicitly requested.
