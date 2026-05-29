# Microservices

## 1. API Gateway

| Attribute       | Detail                               |
|-----------------|--------------------------------------|
| **Purpose**     | Single entry point, reverse proxy, JWT validation |
| **Technology**  | YARP 2.2 (Yet Another Reverse Proxy) |
| **Port**        | 5000                                 |
| **Database**    | None (stateless)                     |
| **Dependencies**| BuildingBlocks (auth middleware, Serilog, OTel) |

### Routing Table

```
/api/auth/**            → auth-service:5001
/api/offices/**         → core-service:5002
/api/companies/**       → core-service:5002
/api/reservations/**    → core-service:5002
/api/reviews/**         → core-service:5002
/api/search/**          → search-service:5004
/api/notifications/**   → notification-service:5005
/health                 → Internal health check
/metrics                → Prometheus metrics
/swagger                → Aggregated Swagger docs
```

The gateway validates the JWT on every request before forwarding. Unauthenticated routes (`/api/auth/login`, `/api/auth/register`) are configured as anonymous pass-through.

---

## 2. Auth Service

| Attribute       | Detail                               |
|-----------------|--------------------------------------|
| **Purpose**     | User registration, login, JWT issuance, token refresh, role management |
| **Port**        | 5001                                 |
| **Database**    | PostgreSQL: `deskmatch_auth`         |
| **Schema**      | `Users`, `RefreshTokens`, enum `userrole` |
| **Libraries**   | ASP.NET Core Identity, JwtBearer, EF Core 8, Npgsql |

### Endpoints

| Method | Path                      | Auth | Description                    |
|--------|---------------------------|------|--------------------------------|
| POST   | `/api/auth/register`      | No   | Register new user              |
| POST   | `/api/auth/login`         | No   | Login, return access+refresh tokens |
| POST   | `/api/auth/refresh`       | No   | Rotate access token using refresh token |
| POST   | `/api/auth/revoke`        | Yes  | Revoke refresh token           |
| GET    | `/api/auth/me`            | Yes  | Get current user profile       |
| GET    | `/api/auth/users`         | Yes  | List users (Admin only)        |
| GET    | `/health`                 | No   | Health check                   |
| GET    | `/metrics`                | No   | Prometheus metrics             |

### JWT Configuration

| Setting                        | Default   |
|--------------------------------|-----------|
| Access Token Expiration        | 15 minutes |
| Refresh Token Expiration       | 7 days    |
| Algorithm                      | HMAC SHA-256 (symmetric key) |

### Seed Data

A default admin user is inserted on init:
- Email: `admin@deskmatch.com`
- Role: `Admin`

---

## 3. Core Service

| Attribute       | Detail                               |
|-----------------|--------------------------------------|
| **Purpose**     | Manage offices, companies, reservations, reviews |
| **Port**        | 5002                                 |
| **Database**    | PostgreSQL: `deskmatch_core`         |
| **Cache**       | Redis 7                              |
| **Search**      | OpenSearch (indexes office on create/update) |
| **Schema**      | `Companies`, `Offices`, `Reservations`, `Reviews` |
| **Libraries**   | Custom CQRS (ICommand/IQuery), FluentValidation, EF Core 8 |

### Endpoints

| Method | Path                              | Auth | Description                        |
|--------|-----------------------------------|------|------------------------------------|
| GET    | `/api/offices`                    | No   | List offices (paginated, filtered) |
| GET    | `/api/offices/{id}`               | No   | Get office by ID                   |
| POST   | `/api/offices`                    | Yes  | Create office (Manager/Admin)      |
| PUT    | `/api/offices/{id}`               | Yes  | Update office (Manager/Admin)      |
| DELETE | `/api/offices/{id}`               | Yes  | Soft-delete office (Admin)         |
| GET    | `/api/companies`                  | No   | List companies                     |
| POST   | `/api/companies`                  | Yes  | Create company (Admin)             |
| GET    | `/api/offices/{id}/reservations`  | No   | List reservations for office       |
| POST   | `/api/reservations`               | Yes  | Create reservation                 |
| PUT    | `/api/reservations/{id}`          | Yes  | Update/cancel reservation          |
| GET    | `/api/offices/{id}/reviews`       | No   | List reviews for office            |
| POST   | `/api/reviews`                    | Yes  | Create review                      |

### Dependencies

| Dependency    | Purpose                                    |
|---------------|--------------------------------------------|
| Redis         | Cache office/company query results (TTL: 5 min) |
| OpenSearch    | Index offices on create/update for search-service |
| Auth service  | JWT validation (shared secret)             |

---

## 4. Search Service

| Attribute       | Detail                               |
|-----------------|--------------------------------------|
| **Purpose**     | Full-text and geo-location search across offices |
| **Port**        | 5004                                 |
| **Database**    | None (writes go to core-service)     |
| **Search**      | OpenSearch: `offices` index          |
| **Libraries**   | Custom CQRS, OpenSearch SDK              |

### Endpoints

| Method | Path                   | Auth | Description                       |
|--------|------------------------|------|-----------------------------------|
| GET    | `/api/search/offices`  | No   | Full-text search with filters     |
| GET    | `/api/search/nearby`   | No   | Geo-search (lat/lon + radius)     |
| GET    | `/api/search/suggest`  | No   | Autocomplete suggestions          |
| GET    | `/health`              | No   | Health check                      |
| GET    | `/metrics`             | No   | Prometheus metrics                |

### Search Capabilities

- Full-text search across `name`, `description`, `address`
- Faceted filtering by `city`, `country`, `amenities`
- Geo-distance queries (`geo_distance` + `geo_bounding_box`)
- Price range filtering
- Sorting by relevance, rating, price, distance

---

## 5. Notification Service

| Attribute       | Detail                               |
|-----------------|--------------------------------------|
| **Purpose**     | Send transactional emails, manage notification history |
| **Port**        | 5005                                 |
| **Database**    | PostgreSQL: `deskmatch_notification` |
| **Schema**      | `EmailTemplates`, `Notifications`    |
| **Libraries**   | MailKit, Custom CQRS                     |

### Endpoints

| Method | Path                         | Auth | Description                     |
|--------|------------------------------|------|---------------------------------|
| GET    | `/api/notifications`         | Yes  | List user notifications         |
| PUT    | `/api/notifications/{id}/read` | Yes | Mark notification as read       |
| POST   | `/api/notifications/send`    | Yes  | Send ad-hoc email (Admin)       |
| GET    | `/health`                    | No   | Health check                    |
| GET    | `/metrics`                   | No   | Prometheus metrics              |

### Email Templates (seed data)

| Template                  | Trigger                        |
|---------------------------|--------------------------------|
| `WelcomeEmail`            | User registration              |
| `ReservationConfirmation` | Reservation confirmed           |
| `ReservationCancelled`    | Reservation cancelled           |

Templates use `{{Placeholder}}` syntax with `FirstName`, `OfficeName`, `ReservationDate`, `StartTime`, `EndTime`, `OfficeAddress`.

---

## Authentication Flow

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Client  │     │ API Gateway  │     │ Auth Service │     │ Core Service │
└────┬────┘     └──────┬───────┘     └──────┬───────┘     └──────┬───────┘
     │                 │                    │                    │
     │  POST /auth/login                    │                    │
     │────────────────►│                    │                    │
     │                 │  Forward login     │                    │
     │                 │───────────────────►│                    │
     │                 │                    │ Validate creds     │
     │                 │                    │ Generate JWT       │
     │                 │◄───────────────────│                    │
     │  {access_token, │                    │                    │
     │   refresh_token} │                   │                    │
     │◄────────────────│                    │                    │
     │                 │                    │                    │
     │  GET /offices   │                    │                    │
     │  Authorization: Bearer {token}       │                    │
     │────────────────►│                    │                    │
     │                 │ Validate JWT       │                    │
     │                 │ Extract claims     │                    │
     │                 │───────────────────────────────────────►│
     │                 │                    │    Query + cache   │
     │                 │◄───────────────────────────────────────│
     │  [offices]      │                    │                    │
     │◄────────────────│                    │                    │
```

## Data Consistency Patterns

| Pattern              | Use Case                                   |
|----------------------|--------------------------------------------|
| **Database per service** | Each service owns its schema; no cross-service joins |
| **Synchronous API**  | Core-service calls OpenSearch SDK to index offices after create/update |
| **Idempotency**      | Reservation creation includes idempotency key to prevent double-booking |
| **Soft delete**      | Offices and users are marked `IsActive = false` rather than hard-deleted |
| **Eventual consistency** | OpenSearch index may lag behind PostgreSQL writes (milliseconds) |
| **Retry & Backoff**  | SDKs implement retry with exponential backoff for transient failures |