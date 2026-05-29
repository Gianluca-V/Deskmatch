# Architecture Overview

## Principles

DeskMatch follows **Domain-Driven Design** with a **microservices** decomposition. Each service owns its bounded context and persists data in its own database schema. Services communicate synchronously via REST (through the API Gateway) and will adopt asynchronous messaging via RabbitMQ for cross-boundary side effects in future iterations.

## Bounded Contexts

```
┌──────────────────────────────────────────────────────────────┐
│  deskmatch_auth                        deskmatch_notification │
│  ┌──────────┐                          ┌──────────────────┐  │
│  │  Users   │                          │  Notifications   │  │
│  │  Roles   │                          │  EmailTemplates  │  │
│  │  Tokens  │                          │                  │  │
│  └──────────┘                          └──────────────────┘  │
│                                                              │
│  deskmatch_core                                              │
│  ┌──────────┐  ┌──────────┐  ┌───────────┐  ┌──────────┐  │
│  │Companies │  │ Offices  │  │Reservations│  │ Reviews  │  │
│  └──────────┘  └──────────┘  └───────────┘  └──────────┘  │
│                                                              │
│  OpenSearch (offices index)                                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  offices (geo-point, text, keyword, scaled_float)    │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

| Bounded Context     | Service              | Database / Store        |
|---------------------|----------------------|-------------------------|
| Authentication      | auth-service         | PostgreSQL: `deskmatch_auth` |
| Core Business       | core-service         | PostgreSQL: `deskmatch_core` |
| Notifications       | notification-service | PostgreSQL: `deskmatch_notification` |
| Full-text & Geo Search | search-service    | OpenSearch: `offices` index |
| Reverse Proxy       | api-gateway          | (stateless)             |

## Service Ownership Matrix

| Service              | Team/Context | Port | Dependencies                         |
|----------------------|-------------|------|--------------------------------------|
| api-gateway          | Platform    | 5000 | YARP, JWT Bearer, BuildingBlocks     |
| auth-service         | Auth/IAM    | 5001 | PostgreSQL, JWT, Identity EF          |
| core-service         | Core        | 5002 | PostgreSQL, Redis, OpenSearch, Custom CQRS, FluentValidation |
| search-service       | Search      | 5004 | OpenSearch, Custom CQRS |
| notification-service | Comms       | 5005 | PostgreSQL, MailKit, Custom CQRS |
| frontend-web         | Frontend    | 3001 | React 18, Vite 5, TanStack Query     |
| frontend-admin       | Frontend    | 3002 | React 18, Vite 5, Recharts           |

## Vertical Slice Architecture (VSA)

Each backend service uses **Vertical Slice Architecture** organized by feature rather than technical layer:

```
src/
├── Api/
│   └── Controllers/           # Thin controllers, delegate to handlers
├── Application/
│   ├── Offices/
│   │   ├── Create/            # CreateOfficeCommand + Handler + Validator
│   │   ├── Update/            # UpdateOfficeCommand + Handler + Validator
│   │   ├── GetById/           # Query + Handler
│   │   └── List/              # ListQuery + Handler
│   ├── Companies/
│   ├── Reservations/
│   └── Reviews/
├── Domain/
│   ├── Entities/              # Aggregate roots, entities, value objects
│   └── Enums/
└── Infrastructure/
    ├── Persistence/           # DbContext, repositories, migrations
    └── Services/              # Infrastructure service implementations
```

Key VSA rules:
- A feature folder contains everything it needs: command, handler, validator, DTOs.
- Vertical slices share nothing across features except domain entities.
- Controllers inject `IMediator` and delegate everything.

## CQRS Pattern

Commands and Queries are separated using the **custom CQRS interfaces** (`ICommand`, `ICommand<T>`, `IQuery<T>`, `ICommandHandler<T>`, `ICommandHandler<T, TResult>`, `IQueryHandler<TQuery, TResult>`) defined in `shared/domain`:

```
HTTP Request
    │
    ▼
Controller.Send(command/query)
    │
    ▼
Custom CQRS Pipeline
    ├── Pre-processor (validation, logging)
    ├── Command Handler / Query Handler
    └── Post-processor (caching, events)
    │
    ▼
Read / Write to Database, Redis, OpenSearch
```

| Aspect       | Commands                    | Queries                         |
|-------------|-----------------------------|---------------------------------|
| Naming      | `CreateXCommand`, `UpdateXCommand` | `GetXQuery`, `ListXQuery` |
| Validation  | FluentValidation validators | N/A                             |
| Return      | `Result<T>` or `Unit`       | `T` (projection / DTO)          |
| DB access   | Write via EF Core           | Read via EF Core projection, Redis cache, or OpenSearch |

## Communication Patterns

### Current: Synchronous REST

```
Client ──► api-gateway ──JWT──► auth-service
                     │
                     ├──► core-service
                     │        ├──► PostgreSQL (read/write)
                     │        ├──► Redis (cache)
                     │        └──► OpenSearch (index offices)
                     │
                     ├──► search-service ──► OpenSearch (query)
                     │
                     └──► notification-service ──► PostgreSQL + SMTP
```

All inter-service communication currently goes through the API Gateway. Each service validates the JWT independently using a shared secret.

### Future: Event-Driven

```
core-service ──► RabbitMQ ──► notification-service  (send email on booking)
              ──► RabbitMQ ──► search-service       (reindex office on update)
```

## Data Flow: Booking a Desk

```
1. User logs in (POST /api/auth/login)                 → JWT tokens returned
2. User searches offices (GET /api/search/offices)      → OpenSearch query
3. User views office details (GET /api/offices/{id})    → core-service + Redis cache
4. User creates reservation (POST /api/reservations)    → core-service writes to PostgreSQL
5. [future] ReservationConfirmed event                  → notification-service sends email
```

## Key Design Decisions

| Decision                          | Rationale                                                    |
|-----------------------------------|--------------------------------------------------------------|
| Per-service databases             | Loose coupling, independent scaling, no shared schema        |
| YARP as API Gateway               | Native .NET, same ecosystem, no extra infra dependency       |
| Custom CQRS + VSA                | Feature cohesion, testability, simple onboarding             |
| OpenSearch for search             | Full-text search, geo-queries, native Lucene integration     |
| OpenTelemetry + Serilog           | Unified observability, structured logs, Loki integration     |
| JWT (symmetric, shared secret)    | Simple for bounded microservices behind gateway              |
| FeatureManagement for feature flags | A/B testing, gradual rollouts, emergency kill switches     |