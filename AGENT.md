# AGENT.md

## Project identity

This repository is `payment-routing-engine`, a production-grade .NET payment orchestration and routing platform.

The purpose of this project is to model how a real payment platform:
- accepts payment requests
- routes transactions across providers
- handles retries and failover
- tracks transaction lifecycle
- processes webhooks
- emits events
- exposes operational visibility

This is not a tutorial project and should not be treated like one.

Any code, documentation, naming, architecture, or automation added to this repository should reinforce the idea that this is a serious backend systems project with fintech and reliability engineering depth.

---

## Primary objective

Build a world-class portfolio-grade backend system that demonstrates:

- strong domain modeling
- clean architecture
- production-minded design
- resilient workflows
- observability
- testability
- thoughtful documentation

The repository should feel like an internal platform maintained by a careful backend engineer.

---

## Non-goals

Avoid turning this project into:

- a basic CRUD demo
- a generic “clean architecture” skeleton with no domain depth
- an over-engineered abstraction playground
- an AI-generated code dump
- a repository with fancy buzzwords but weak implementation

Do not add complexity that does not support payment routing, resilience, auditability, or maintainability.

---

## Engineering standards

### 1. Favor clarity over cleverness
- Prefer explicit names
- Keep business rules readable
- Avoid overly magical patterns
- Make workflows easy to follow

### 2. Protect the domain
- Core orchestration logic belongs in domain/application layers
- Provider-specific details must stay in infrastructure/adapters
- Avoid leaking transport or persistence concerns into domain models

### 3. Design for failure
This system must assume:
- providers timeout
- providers return inconsistent responses
- webhooks arrive late or duplicated
- retries can happen
- clients can send duplicate requests

Failure handling is a core part of the product, not an afterthought.

### 4. Make operations visible
- Log important decisions
- Record transaction history
- Expose metrics where useful
- Preserve auditability

### 5. Keep the code portfolio-grade
Every file should support one of these outcomes:
- stronger domain design
- better resilience
- better developer experience
- clearer observability
- better tests
- better documentation

---

## Architecture expectations

The codebase should generally move toward these boundaries:

- **Presentation/API**
  - controllers or minimal API endpoints
  - request/response contracts
  - validation
  - auth hooks later if needed

- **Application**
  - orchestration use cases
  - command/query handlers if used
  - transaction coordination
  - retry/failover policies
  - idempotency coordination

- **Domain**
  - entities
  - value objects
  - enums
  - domain services
  - domain events
  - business invariants

- **Infrastructure**
  - database persistence
  - provider clients
  - queues
  - logging integrations
  - outbox implementation
  - telemetry integrations

- **Workers / Background Processing**
  - retries
  - event dispatch
  - reconciliation
  - webhook-related async handling

Keep these boundaries intentional. Do not collapse everything into services that know too much.

---

## Naming guidance

Use names that reflect the payment domain clearly.

Prefer:
- `PaymentTransaction`
- `PaymentAttempt`
- `ProviderSelector`
- `RetryPolicyEvaluator`
- `WebhookSignatureValidator`
- `TransactionHistoryEntry`
- `PaymentOrchestrator`

Avoid vague names like:
- `Helper`
- `Manager`
- `Processor` unless the meaning is precise
- `Utils`
- `CommonService`
- `BaseManager`

Names should communicate responsibility without needing extra explanation.

---

## Domain expectations

The repository should model realistic payment concepts such as:

- transaction creation
- provider selection
- provider attempts
- provider response mapping
- retry eligibility
- failover decisions
- transaction status transitions
- webhook reconciliation
- audit history
- idempotency

State transitions should be explicit and testable.

Whenever possible, preserve a clear history of:
- what happened
- why it happened
- which provider was involved
- whether the event was synchronous or asynchronous

---

## Reliability expectations

This project should eventually support patterns like:

- idempotency keys
- retry with backoff
- dead-letter or failed retry handling
- provider failover
- outbox pattern
- duplicate webhook protection
- timeout-aware provider calls
- correlation IDs for tracing

Any contribution related to payment execution should consider transient failure and duplication risk.

---

## Testing guidance

Tests should be treated as a first-class part of the system.

Prioritize:
- domain tests for status transitions and invariants
- application tests for orchestration rules
- integration tests for persistence and messaging
- webhook tests for signature/duplicate handling
- provider adapter tests for response mapping

Avoid relying only on shallow controller tests.

When adding new behavior:
- add tests for happy path
- add tests for failure path
- add tests for edge cases when the behavior is business-critical

---

## Documentation expectations

Documentation should remain strong throughout the project.

Important docs to maintain over time:
- `README.md`
- architecture overview
- domain glossary
- sequence diagrams
- setup instructions
- design decision records if added

When major architecture choices are made, document:
- what was chosen
- why it was chosen
- trade-offs
- future alternatives

---

## Style guidance for AI agents and contributors

When generating or modifying code in this repository:

### Do
- write code that is explicit and readable
- respect architectural boundaries
- preserve domain language
- include reasonable error handling
- add comments only where they truly help
- keep methods focused
- design with observability in mind
- think through transaction lifecycle impact

### Do not
- introduce unnecessary abstractions
- generate placeholder classes with no clear role
- hide business logic inside controllers
- mix provider-specific logic into core orchestration
- use generic names that weaken the domain model
- create dead code or speculative components “for future use”

---

## Contribution priorities

When choosing what to build next, prefer work in this order:

1. domain correctness
2. orchestration flow
3. persistence model
4. provider adapter contracts
5. retries and failover
6. webhook handling
7. outbox/events
8. observability
9. test depth
10. local developer experience

Do not prioritize cosmetic complexity over core system behavior.

---

## Quality bar

A good contribution should make the repository feel more like:
- a real payment platform
- a maintainable internal service
- a strong systems design case study

A weak contribution makes it feel more like:
- a tutorial
- a toy demo
- a collection of disconnected patterns

Always optimize toward the first category.

---

## Recommended mindset

Treat this repository as if:
- it will be read by senior backend engineers
- it may be discussed in an interview
- every design choice should be explainable
- operational failure scenarios matter
- elegance comes from clarity and correctness

The standard is not “works on my machine.”
The standard is “credible as a serious backend platform.”

---

## Short project summary

`payment-routing-engine` is a .NET backend platform for orchestrating payments across providers with retries, failover, idempotency, webhook handling, event-driven workflows, and operational visibility.

Every change should strengthen that identity.