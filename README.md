# DeskMatch
Smart desk booking and workspace management platform. DeskMatch enables companies to list their office spaces and users to discover, book, and review workspaces across the globe.

## Architecture Diagram

```
                         ┌──────────────────────────────────────┐
                         │              INTERNET                │
                         └──────────┬───────────────────────────┘
                                    │
                                    ▼
                         ┌──────────────────┐
                         │   NGINX Ingress  │
                         │  (Kubernetes LB) │
                         └────────┬─────────┘
                                  │
                    ┌─────────────┼─────────────┐
                    ▼             ▼             ▼
          ┌─────────────┐ ┌───────────┐ ┌───────────────┐
          │ frontend-web │ │ frontend- │ │  API Gateway  │
          │   (React)    │ │  admin    │ │  (YARP .NET)  │
          │   :3001      │ │  (React)  │ │    :5000      │
          └─────────────┘ │  :3002    │ └───────┬───────┘
                          └───────────┘         │
                                                │ JWT Auth + Routing
                        ┌───────────────────────┼───────────────────────┐
                        ▼                       ▼                       ▼
              ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
              │ AuthService │         │ CoreService │         │SearchService│
              │   :5001     │         │   :5002     │         │   :5004    │
              │  CQRS/VSA   │         │  CQRS/VSA   │         │  CQRS/VSA   │
              └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
                     │                       │                       │
                     ▼                       ▼                       ▼
              ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
              │  PostgreSQL │         │  PostgreSQL │         │  OpenSearch │
              │ (auth DB)   │         │ (core DB)   │         │  (offices)  │
              └─────────────┘         └──────┬──────┘         └─────────────┘
                                             │
                                             ▼
                                    ┌─────────────┐         ┌──────────────┐
                                    │    Redis    │         │Notification  │
                                    │   (cache)   │         │  Service     │
                                    └─────────────┘         │   :5005      │
                                                            │  MailKit/SMTP│
                                                            └──────┬───────┘
                                                                   │
                                                                   ▼
                                                            ┌─────────────┐
                                                            │  PostgreSQL │
                                                            │ (notif DB)  │
                                                            └─────────────┘

                         ┌──────────────────────────────────────┐
                         │           MONITORING STACK           │
                         │  Prometheus  │  Grafana  │  Loki    │
                         │    :9090     │   :3000   │  :3100   │
                         └──────────────────────────────────────┘
```

## Tech Stack

| Category              | Technology                                                              |
|-----------------------|-------------------------------------------------------------------------|
| **Backend**           | .NET 8, ASP.NET Core, C# 12                                             |
| **Frontend**          | React 18, Vite 5, React Router 6, TanStack Query 5                    |
| **API Gateway**       | YARP (Yet Another Reverse Proxy) 2.2                                   |
| **Database**          | PostgreSQL 16 (per-service databases)                                   |
| **Cache**             | Redis 7                                                                 |
| **Search**            | OpenSearch 2.x                                                          |
| **Message Queue**     | RabbitMQ (future event bus)                                             |
| **Object Storage**    | MinIO (S3-compatible)                                                   |
| **Email**             | MailKit 4.8 (SMTP / SendGrid)                                           |
| **Observability**     | Prometheus, Grafana, Loki, OpenTelemetry                                |
| **Logging**           | Serilog (console + file + Grafana Loki sink)                            |
| **Validation**        | FluentValidation 11                                                     |
| **CQRS / Mediator**   | Custom ICommand/IQuery (no external library)                        |
| **Auth / Identity**   | ASP.NET Core Identity + JWT Bearer                                      |
| **Container**         | Docker (multi-stage builds)                                             |
| **Orchestration**     | Kubernetes (kustomize overlays) + Helm 3                                |
| **CI/CD**             | GitHub Actions                                                          |
| **Feature Flags**     | Microsoft.FeatureManagement 3.5                                         |
| **Code Analysis**     | Roslyn Analyzers (latest), editorconfig                                 |

## Quick Start

### Prerequisites

- **Docker Desktop** (or Docker Engine + Docker Compose)
- **.NET 8 SDK** (for local dev)
- **Node.js 20+** (for local frontend dev)

### Docker Compose (recommended)

```bash
cd infrastructure/docker
cp .env.example .env       # Edit secrets if needed
docker compose up -d --build
```

Once all services are healthy:

| Service                   | URL                           |
|---------------------------|-------------------------------|
| API Gateway (Swagger)     | http://localhost:5000/swagger |
| Frontend Web              | http://localhost:3001         |
| Frontend Admin            | http://localhost:3002         |
| Grafana                   | http://localhost:3000         |
| Prometheus                | http://localhost:9090         |
| OpenSearch Dashboards     | http://localhost:5601         |

### Kubernetes (local dev)

```bash
kubectl apply -k infrastructure/kubernetes/development
```

Monitor rollout:

```bash
kubectl get pods -n deskmatch --watch
```

### Kubernetes (Helm)

```bash
helm upgrade --install deskmatch infrastructure/helm/deskmatch \
  -f infrastructure/helm/deskmatch/values-development.yaml \
  --namespace deskmatch --create-namespace
```

## Project Structure

```
DeskMatch/
├── apps/                          # Application services
│   ├── api-gateway/               # YARP reverse proxy + JWT validation
│   ├── auth-service/              # Authentication & user management
│   ├── core-service/              # Offices, companies, reservations, reviews
│   ├── notification-service/      # Email templates & notifications
│   ├── search-service/            # OpenSearch full-text + geo search
│   ├── frontend-web/              # Customer-facing React SPA
│   └── frontend-admin/            # Admin dashboard React SPA
├── shared/                        # Shared libraries
│   ├── building-blocks/           # Auth, exceptions, logging, middleware, observability
│   ├── domain/                    # Abstractions, base classes, CQRS primitives
│   └── sdk/                       # OpenSearch, Redis, Notification, Storage SDKs
├── infrastructure/
│   ├── docker/                    # Docker Compose + init scripts + configs
│   ├── kubernetes/                # Kustomize overlays (base/dev/staging/prod)
│   ├── helm/deskmatch/            # Helm chart (13 templates)
│   ├── monitoring/                # Grafana dashboards, Prometheus rules
│   └── terraform/                 # IaC (future)
├── tests/
│   ├── contract/                  # Pact contract tests
│   ├── e2e/                       # End-to-end tests
│   ├── integration/               # Integration tests
│   └── load/                      # Load / stress tests (k6)
├── tools/                         # Dev utilities
├── docs/                          # Project documentation
│   ├── architecture/              # Architecture & microservices docs
│   ├── onboarding/                # Getting started & dev setup
│   ├── infrastructure/            # Docker, Postgres, Redis docs
│   ├── kubernetes/                # K8s & Helm docs
│   ├── opensearch/                # Search indexes & queries
│   ├── monitoring/                # Observability & alerting
│   ├── ci-cd/                     # Pipeline documentation
│   ├── api/                       # API reference
│   └── runbooks/                  # Troubleshooting guide
├── Directory.Build.props          # Shared MSBuild properties
├── DeskMatch.sln                  # .NET solution file
├── .env.example                   # Environment variables template
├── .editorconfig                  # Code style rules
└── nuget.config                   # NuGet source configuration
```

## Documentation Index

| Section                               | Description                                   |
|---------------------------------------|-----------------------------------------------|
| [Architecture Overview](docs/architecture/overview.md) | High-level design, CQRS, VSA, bounded contexts |
| [Microservices](docs/architecture/microservices.md)   | Per-service detail, routing, auth flow        |
| [Getting Started](docs/onboarding/getting-started.md) | Prerequisites, clone, run, access             |
| [Development Setup](docs/onboarding/development-setup.md) | IDE config, debugging, code style      |
| [Docker](docs/infrastructure/docker.md)               | Multi-stage builds, compose, volumes, networks |
| [PostgreSQL](docs/infrastructure/postgres.md)         | Schemas, migrations, init scripts              |
| [Redis](docs/infrastructure/redis.md)                 | Cache config, TTL, invalidation                |
| [Kubernetes](docs/kubernetes/overview.md)             | Deployments, services, ingress, HPA            |
| [Helm Charts](docs/kubernetes/helm.md)                | Chart structure, values, install/upgrade       |
| [OpenSearch](docs/opensearch/indexes.md)              | Index mapping, queries, geo-search             |
| [Monitoring](docs/monitoring/overview.md)             | Prometheus, Grafana, Loki, alerting            |
| [CI/CD](docs/ci-cd/pipelines.md)                      | GitHub Actions, Docker publish, K8s deploy     |
| [API Reference](docs/api/overview.md)                 | Auth, endpoints, request/response examples     |
| [Runbooks](docs/runbooks/common-issues.md)            | Troubleshooting common problems                |

## Contributing

1. Fork the repository and create a feature branch from `main`.
2. Follow the code style defined in `.editorconfig` and `Directory.Build.props`.
3. Write tests for new features in the appropriate test project.
4. Ensure all existing tests pass before submitting a PR.
5. Use [Conventional Commits](https://www.conventionalcommits.org/) for commit messages.
6. Squash commits before merging; keep PRs focused.

## License

MIT

---

**DeskMatch** — Find your perfect workspace.
