# payment-routing-engine

A production-grade payment orchestration and routing platform built with .NET.

## Overview

`payment-routing-engine` is a backend system that simulates how modern payment platforms process, route, retry, and track transactions across multiple providers.

Instead of sending every payment request to a single provider, the system acts as a routing engine that can:

- accept payment requests from clients
- select the best provider based on routing rules
- retry failed transactions using configurable strategies
- fail over to alternative providers
- process asynchronous provider webhooks
- maintain transaction state and audit history
- emit domain events for downstream consumers
- expose operational insights for support and engineering teams

This project is designed to demonstrate production-grade backend engineering, not basic CRUD.

---

## Goals

The goal of this project is to model the core concerns of a real payment orchestration platform:

- reliability
- provider abstraction
- resilience
- idempotency
- auditability
- observability
- extensibility

It is also intended to serve as a strong portfolio project for backend and fintech engineering roles.

---

## Core Features

### Payment orchestration
- Accept payment requests through a REST API
- Validate request payloads and business rules
- Persist transaction records and lifecycle state

### Intelligent provider routing
- Route payments to a provider using configurable routing rules
- Support multiple provider adapters behind a common contract
- Allow future extension for cost-based or success-rate-based routing

### Retry and failover
- Retry transient failures with backoff
- Fail over to a secondary provider when appropriate
- Distinguish retryable vs non-retryable failures

### Idempotency
- Prevent duplicate transaction processing using idempotency keys
- Ensure safe retries from client applications

### Webhook handling
- Receive provider webhook callbacks
- Verify and process asynchronous payment updates
- Reconcile webhook updates against existing transaction state

### Status tracking and auditability
- Maintain full transaction history
- Record orchestration decisions and provider responses
- Support operational visibility into payment lifecycle

### Event-driven design
- Publish domain/integration events for downstream systems
- Support eventual consistency through an outbox pattern

### Observability
- Structured logging
- Metrics for provider success/failure rates
- Traceable transaction flows across orchestration steps

---

## Architecture

The system follows a modular, layered architecture with clear separation of concerns.

### High-level components

- **API Layer**  
  Exposes endpoints for payment initiation, status lookup, webhook handling, and operational queries.

- **Application Layer**  
  Coordinates orchestration workflows, routing decisions, retry behavior, and transaction state transitions.

- **Domain Layer**  
  Contains business rules, entities, value objects, enums, and orchestration logic.

- **Infrastructure Layer**  
  Handles database access, provider clients, message publishing, logging, and external integrations.

- **Background Processing**  
  Responsible for retries, webhook reconciliation, event dispatching, and scheduled operations.

---

## Example Flow

1. A client submits a payment request
2. The API validates the request and checks idempotency
3. A transaction record is created
4. The routing engine selects a provider
5. The provider adapter sends the request
6. If successful, the transaction is updated accordingly
7. If the provider times out or returns a transient error:
   - the system schedules a retry or
   - falls back to another provider
8. Provider webhooks update final transaction status
9. Domain events are emitted for downstream consumers
10. Metrics and logs are recorded throughout the flow

---

## Planned Tech Stack

- **.NET 8**
- **ASP.NET Core Web API**
- **PostgreSQL**
- **Entity Framework Core** or **Dapper** where appropriate
- **RabbitMQ** for asynchronous workflows
- **Serilog** for structured logging
- **OpenTelemetry** for tracing and metrics
- **Docker / Docker Compose**
- **xUnit** for testing
- **Testcontainers** for integration tests

---

## Initial Domain Concepts

### Entities
- `PaymentTransaction`
- `PaymentAttempt`
- `ProviderRoutingRule`
- `WebhookEvent`
- `OutboxMessage`

### Important enums
- `TransactionStatus`
- `PaymentProvider`
- `FailureCategory`
- `AttemptStatus`
- `RoutingDecisionType`

### Example statuses
- Pending
- Processing
- Succeeded
- Failed
- Retrying
- RequiresManualReview
- Cancelled

---

## API Scope

### Planned endpoints

#### Payments
- `POST /api/payments`
- `GET /api/payments/{transactionId}`
- `GET /api/payments`
- `POST /api/payments/{transactionId}/retry`

#### Webhooks
- `POST /api/webhooks/{provider}`

#### Operations / Monitoring
- `GET /api/providers/health`
- `GET /api/providers/metrics`
- `GET /api/transactions/{transactionId}/history`

---

## Non-Functional Priorities

This project prioritizes the following engineering concerns:

- clean architecture
- maintainability
- resilience under failure
- debuggability
- scalability
- safe retries
- deterministic state transitions
- realistic documentation

---

## What makes this project valuable

This project is intentionally designed to go beyond a demo API.

It demonstrates:
- backend system design
- fintech-oriented architecture
- provider abstraction
- asynchronous workflows
- fault tolerance
- practical testing strategy
- production-minded engineering decisions

---

## Roadmap

### Phase 1
- project bootstrap
- base solution structure
- transaction domain model
- create payment endpoint
- provider abstraction
- mock provider implementation

### Phase 2
- routing engine
- retry rules
- failover flow
- webhook endpoint
- transaction history

### Phase 3
- outbox pattern
- RabbitMQ integration
- background workers
- metrics and traces
- provider health dashboard endpoints

### Phase 4
- integration tests
- load/failure simulation
- containerized local environment
- improved operational tooling

---

## Local Development

Detailed setup instructions will be added as the project evolves.

Planned local environment:
- API service
- PostgreSQL
- RabbitMQ
- observability tooling
- mock provider simulator(s)

---

## Design Principles

- Prefer explicit domain language over vague service naming
- Keep provider-specific behavior behind adapters
- Make state transitions observable and testable
- Treat retries and failover as first-class design concerns
- Favor auditability over hidden automation
- Build for extension, not premature complexity

---

## Future Enhancements

- dynamic provider scoring
- rate limiting per provider
- fraud/risk hooks
- reconciliation jobs
- dashboard UI
- multi-tenant support
- support for payouts/refunds
- AI-assisted incident classification

---

## Status

This project is currently in active design and implementation.

The first milestone is to establish the domain model, orchestration workflow, and core provider routing foundation.

---

## Author

Built as part of a world-class backend engineering portfolio focused on:
- payment systems
- distributed backend architecture
- reliability engineering
- modern .NET development