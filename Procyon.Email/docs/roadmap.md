# Roadmap

## Phase 1: architecture and scaffolding

Create package structure, provider-neutral contracts, core orchestration, configuration validation, Resend scaffolding, documentation, and initial tests.

## Phase 2: Resend sending implementation

Implement the Resend HTTP boundary using `IHttpClientFactory`, translate provider responses, add retry-aware failure handling, and expand tests without calling the real API.

## Phase 3: templates and development providers

Add email templates, local/development providers, and example application coverage.

## Phase 4: webhooks and delivery events

Add provider-neutral delivery events and webhook processing abstractions.

## Phase 5: Amazon SES

Add an Amazon SES provider package.

## Phase 6: Azure

Add an Azure Communication Services Email provider package.

## Phase 7: queue and durable outbox

Add queue integration and durable outbox patterns for resilient delivery.
