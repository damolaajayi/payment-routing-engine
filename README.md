# payment-routing-engine

A production-grade payment orchestration and routing platform built with .NET.

## Overview

`payment-routing-engine` is a backend system that models how modern payment platforms create, route, retry, fail over, and track transactions across multiple payment providers.

Instead of sending every payment request directly to one provider, the system acts as a routing engine that can:

- accept payment creation requests
- generate internal payment references
- enforce idempotency for safe retries
- queue payment processing asynchronously
- route payments to supported providers
- retry transient provider failures
- fail over to alternative providers
- track every provider attempt
- process asynchronous provider webhooks
- maintain transaction state and audit history
- expose traceable logs and operational visibility

This project is designed to demonstrate production-grade backend engineering, not basic CRUD.

---

## Goals

The goal of this project is to model the core concerns of a real payment orchestration platform:

- reliability
- provider abstraction
- retry and failover handling
- idempotency
- auditability
- observability
- asynchronous processing
- clean architecture
- extensibility

It is also intended to serve as a strong portfolio project for backend, fintech, and distributed systems engineering roles.

---

## Current Capabilities

### Payment creation

- Accepts payment creation requests through a REST API
- Generates internal payment references server-side
- Supports client-provided `clientReference` for external business mapping
- Supports client-provided `idempotencyKey` for safe request retries
- Persists transaction records and initial lifecycle state

### CQRS and custom dispatcher

- Uses a custom lightweight CQRS dispatcher instead of MediatR
- Supports commands, queries, handlers, and pipeline behaviors
- Separates write operations from read operations
- Keeps controllers thin and focused on HTTP concerns

### Validation

- Uses FluentValidation for command validation
- Validation runs through a CQRS pipeline behavior
- Handlers stay focused on orchestration and business logic

### Error handling

- Uses centralized global exception middleware
- Returns consistent error responses
- Handles validation errors, not found errors, bad requests, and unexpected failures

### Provider abstraction

- Uses a common `IPaymentProviderClient` contract
- Supports multiple provider implementations
- Includes a mock provider for local simulation
- Includes Paystack provider integration structure

### Provider resolver and routing

- Resolves provider clients dynamically
- Supports provider routing through a routing service
- Currently supports simple provider selection logic
- Can be extended to database-driven routing rules

### Retry and failover

- Classifies failures using `FailureCategory`
- Retries retryable failures such as:
  - provider transient errors
  - timeouts
  - network failures
- Fails over to another provider when applicable
- Records every provider attempt

### RabbitMQ asynchronous processing

- Queues payment processing requests through RabbitMQ
- API returns quickly after enqueueing work
- Worker service consumes payment processing messages
- Supports controlled retry count
- Moves failed messages to a dead letter queue after max retries

### Webhook handling

- Receives Paystack webhook callbacks
- Verifies webhook signature
- Updates transaction state from provider callback
- Designed to support idempotent webhook processing

### Rate limiting

- Uses ASP.NET Core rate limiting middleware
- Applies stricter limits to sensitive payment-processing endpoints
- Helps protect against abuse and accidental repeated processing

### Observability

- Uses Serilog for structured logging
- Uses correlation IDs for request tracing
- Uses OpenTelemetry for distributed tracing
- Traces API requests, HTTP calls, and application flow

### Dockerized infrastructure

- Supports Docker Compose local environment
- Runs API, Worker, PostgreSQL, and RabbitMQ
- Provides RabbitMQ Management Dashboard
- Uses environment variables and `.env` for local configuration

---

## Architecture

The system follows a modular, layered architecture with clear separation of concerns.

```text
Client
  ↓
API Layer
  ↓
Application Layer
  ↓
Domain Layer
  ↓
Infrastructure Layer
  ↓
PostgreSQL / RabbitMQ / External Providers
```

### High-level components

#### API Layer

Responsible for:

- HTTP endpoints
- request models
- response models
- thin controller actions
- global exception handling
- rate limiting
- Swagger/OpenAPI

#### Application Layer

Responsible for:

- commands and queries
- handlers
- CQRS abstractions
- validation contracts
- provider abstractions
- persistence abstractions
- orchestration workflows

#### Domain Layer

Responsible for:

- entities
- enums
- business rules
- transaction state transitions
- payment attempt tracking

#### Infrastructure Layer

Responsible for:

- EF Core persistence
- PostgreSQL database access
- repository implementations
- RabbitMQ publishing
- provider clients
- provider resolver
- logging pipeline behaviors
- external integrations

#### Worker Layer

Responsible for:

- consuming RabbitMQ messages
- dispatching background payment processing commands
- acknowledging successful messages
- retrying failed messages
- moving failed messages to DLQ

---

## Solution Structure

```text
payment-routing-engine/
├── src/
│   ├── PaymentRoutingEngine.Api/
│   ├── PaymentRoutingEngine.Application/
│   ├── PaymentRoutingEngine.Domain/
│   ├── PaymentRoutingEngine.Infrastructure/
│   └── PaymentRoutingEngine.Workers/
├── tests/
│   ├── PaymentRoutingEngine.UnitTests/
│   ├── PaymentRoutingEngine.IntegrationTests/
│   └── PaymentRoutingEngine.ArchitectureTests/
├── docker-compose.yml
├── .env.example
├── README.md
├── AGENT.md
└── PaymentRoutingEngine.sln
```

---

## Request Lifecycle

### Create payment

```text
POST /api/payments
  ↓
Controller maps request to command
  ↓
Dispatcher sends command
  ↓
Validation behavior validates command
  ↓
Handler checks idempotency
  ↓
Domain creates PaymentTransaction
  ↓
Transaction saved to PostgreSQL
  ↓
API returns created transaction
```

### Process payment asynchronously

```text
POST /api/payments/{transactionId}/process
  ↓
API publishes ProcessPaymentMessage to RabbitMQ
  ↓
Worker consumes message
  ↓
Worker dispatches ProcessPaymentCommand
  ↓
Routing service selects provider
  ↓
Provider resolver resolves provider client
  ↓
Provider client processes payment
  ↓
Attempt is recorded
  ↓
Transaction status is updated
  ↓
Worker acknowledges message
```

### Retry and DLQ flow

```text
Worker processing fails
  ↓
Retry count is incremented
  ↓
Message is republished to process-payment-queue
  ↓
If max retry count is exceeded
  ↓
Message is moved to process-payment-dlq
```

---

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- C#
- PostgreSQL
- Entity Framework Core
- RabbitMQ
- FluentValidation
- Serilog
- OpenTelemetry
- Docker / Docker Compose
- Swagger / OpenAPI
- xUnit
- Testcontainers planned

---

## Domain Concepts

### Entities

- `PaymentTransaction`
- `PaymentAttempt`
- `TransactionStatusHistoryEntry`
- `ProviderRoutingRule`
- `WebhookEvent`
- `OutboxMessage`

### Important enums

- `TransactionStatus`
- `PaymentProvider`
- `FailureCategory`
- `AttemptStatus`
- `RoutingDecisionType`
- `WebhookProcessingStatus`

### Transaction statuses

- Pending
- Processing
- Succeeded
- Failed
- Retrying
- RequiresManualReview
- Cancelled

### Failure categories

- None
- Validation
- ProviderTransient
- ProviderPermanent
- Timeout
- Network
- Duplicate
- Unknown

---

## API Endpoints

### Payments

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/payments` | Create a payment transaction |
| `GET` | `/api/payments/{transactionId}` | Get payment details |
| `POST` | `/api/payments/{transactionId}/process` | Queue payment processing |

### Webhooks

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/webhooks/paystack` | Receive Paystack webhook callback |

### Monitoring

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/health` | API health check |
| `GET` | `/swagger` | Swagger UI |

---

## Example Create Payment Request

```json
{
  "idempotencyKey": "idem-20260429-001",
  "clientReference": "ORDER-1001",
  "customerId": "cust-001",
  "amountMinor": 150000,
  "currency": "NGN",
  "description": "Payment for order ORDER-1001"
}
```

### Field meaning

| Field | Purpose |
|---|---|
| `idempotencyKey` | Client-generated key used to safely retry the same request |
| `clientReference` | Client/business system reference, such as order ID or invoice ID |
| `customerId` | Identifier of the customer making the payment |
| `amountMinor` | Amount in the smallest currency unit |
| `currency` | Three-letter currency code, for example `NGN` |
| `description` | Human-readable payment description |

The server generates the internal payment `reference`.

---

## Example Create Payment Response

```json
{
  "transactionId": "3f47f3d7-5e59-4d18-82a0-08dc2c3b9a20",
  "reference": "PAY-4F9A21C8B123",
  "idempotencyKey": "idem-20260429-001",
  "amountMinor": 150000,
  "currency": "NGN",
  "status": "Pending",
  "createdAtUtc": "2026-04-29T10:30:00Z"
}
```

---

## Local Development

Do not store secrets or real connection strings in `appsettings.json`.

Use one of these instead:

- Docker Compose `.env`
- .NET user secrets
- environment variables

---

## Environment Variables

Create a local `.env` file from `.env.example`.

### `.env.example`

```env
POSTGRES_DB=payment_routing_engine
POSTGRES_USER=db-user
POSTGRES_PASSWORD=change_me

DB_CONNECTION_STRING=Host=postgres;Port=5432;Database=payment_routing_engine;Username=db-user;Password=change_me

PAYSTACK_SECRET_KEY=sk_test_replace_me
PAYSTACK_WEBHOOK_SECRET=replace_me
```

Do not commit your real `.env` file.

---

## Docker Compose Setup

The Docker Compose environment runs:

- API
- Worker
- PostgreSQL
- RabbitMQ

Start everything:

```bash
docker compose --env-file .env up --build
```

Stop everything:

```bash
docker compose down
```

Stop and remove volumes:

```bash
docker compose down -v
```

---

## Local URLs

| Service | URL |
|---|---|
| API Swagger | `http://localhost:8080/swagger` |
| API Health | `http://localhost:8080/health` |
| RabbitMQ Dashboard | `http://localhost:15672` |
| PostgreSQL | `localhost:5432` |

RabbitMQ default local login:

```text
guest / guest
```

---

## Connection Strings

When running from your local machine:

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=payment_routing_engine;Username=db-user;Password=change_me"
  }
}
```

When running inside Docker Compose:

```text
Host=postgres;Port=5432;Database=payment_routing_engine;Username=db-user;Password=change_me
```

Why?

Inside Docker, `localhost` means the current container. Containers should use the Docker Compose service name `postgres`.

---

## Database Migrations

Run migrations from the host machine:

```bash
dotnet ef database update \
  --project src/PaymentRoutingEngine.Infrastructure \
  --startup-project src/PaymentRoutingEngine.Api
```

If you are running on Windows CMD, use one line:

```bash
dotnet ef database update --project src/PaymentRoutingEngine.Infrastructure --startup-project src/PaymentRoutingEngine.Api
```

---

## RabbitMQ Behavior

The main queue is:

```text
process-payment-queue
```

The dead letter queue is:

```text
process-payment-dlq
```

When a payment processing message fails:

1. The worker increments the retry count.
2. The message is republished to the main queue.
3. If max retries is exceeded, the message is moved to the DLQ.

RabbitMQ dashboard:

```text
http://localhost:15672
```

Check:

- Ready messages
- Unacked messages
- Ack rate
- Redelivered messages
- DLQ messages

---

## Observability

The system includes:

- Serilog structured logging
- Correlation ID middleware
- CQRS request logging behavior
- OpenTelemetry tracing
- Console exporter for local tracing

Every request can be traced through:

```text
HTTP request → CQRS handler → database/provider operation → response
```

---

## Security Notes

- Never commit real secrets.
- Keep `.env` ignored.
- Use user secrets for local API/Worker development.
- Use environment variables in Docker.
- Verify webhook signatures before updating transaction status.
- Use idempotency keys for safe client retries.
- Use rate limiting to reduce abuse on sensitive endpoints.

---

## Design Principles

- Keep controllers thin.
- Keep mapping explicit; avoid AutoMapper for this project.
- Prefer constructor/static-factory mapping for response DTOs.
- Keep provider-specific behavior behind provider clients.
- Keep routing decisions outside controllers.
- Treat retries and failover as first-class concerns.
- Make state transitions observable and testable.
- Favor auditability over hidden automation.
- Build for extension, not premature complexity.

---

## Current Status

Implemented:

- Modular clean architecture
- CQRS with custom dispatcher
- FluentValidation pipeline
- Global exception middleware
- Thin controllers
- PostgreSQL persistence
- Payment transaction creation
- Payment lookup
- Async payment processing through RabbitMQ
- Provider resolver
- Mock provider
- Paystack provider structure
- Retry and failover orchestration
- Controlled RabbitMQ retry count
- Dead letter queue
- Rate limiting
- Paystack webhook handling
- Serilog logging
- OpenTelemetry tracing
- Docker Compose environment

---

## Roadmap

### Next improvements

- Add RabbitMQ delayed retry/backoff
- Add outbox pattern for reliable event publishing
- Add provider health tracking
- Add real provider metrics
- Add integration tests with Testcontainers
- Add architecture tests
- Add structured health check response
- Add CI pipeline
- Add deployment notes for AWS

### Future enhancements

- dynamic provider scoring
- database-driven routing rules
- rate limiting per customer
- fraud/risk scoring hooks
- reconciliation jobs
- dashboard UI
- multi-tenant support
- support for payouts/refunds
- AI-assisted incident classification

---

## Author

Built as part of a world-class backend engineering portfolio focused on:

- payment systems
- distributed backend architecture
- reliability engineering
- observability
- modern .NET development
- AI-ready backend systems
