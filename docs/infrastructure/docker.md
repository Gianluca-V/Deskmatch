# Docker Documentation

## Multi-Stage Builds

All .NET services use **multi-stage Dockerfiles** to minimize final image size.

### .NET Backend Pattern

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["*.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DeskMatch.ServiceAssembly.dll"]
```

### Frontend Pattern

```dockerfile
# Stage 1: Build
FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Stage 2: Serve
FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
CMD ["nginx", "-g", "daemon off;"]
```

### Dependency Layering

Docker COPY order is optimized for layer caching:

1. Copy `.csproj` files → restore NuGet packages (cached unless csproj changes)
2. Copy source code → build/publish (rebuilt on any source change)

## Docker Compose Services

### Compose File Location

```
infrastructure/docker/
├── docker-compose.yml          # Main compose file
├── .env                         # Environment overrides
├── postgres/
│   ├── init-multiple-databases.sh
│   ├── init-auth.sql
│   ├── init-core.sql
│   └── init-notification.sql
├── opensearch/
│   ├── init-index.sh
│   ├── offices-mapping.json
│   └── seed-data.json
├── prometheus/
│   └── prometheus.yml
├── grafana/
│   └── datasources.yml
└── loki/
    └── loki-config.yml
```

### Service Breakdown

#### Infrastructure

| Service         | Image                        | Purpose                         |
|-----------------|------------------------------|---------------------------------|
| postgres        | postgres:16-alpine           | Primary database (3 databases)  |
| redis           | redis:7-alpine               | Cache layer (AOF persistence)   |
| opensearch      | opensearchproject/opensearch:2.x | Full-text + geo search    |
| opensearch-dashboards | opensearchproject/opensearch-dashboards:2.x | Admin UI |
| opensearch-init | alpine/curl                  | Creates index + seeds data, exits |

#### Monitoring

| Service    | Image                   | Port | Purpose                  |
|------------|-------------------------|------|--------------------------|
| prometheus | prom/prometheus:latest  | 9090 | Metrics collection       |
| grafana    | grafana/grafana:latest  | 3000 | Dashboards + alerting    |
| loki       | grafana/loki:latest     | 3100 | Log aggregation          |

#### Application

All built from local Dockerfiles in `apps/*/Dockerfile`.

### Health Checks

Each service defines a health check:

| Service              | Check Type | Command                                    |
|----------------------|------------|--------------------------------------------|
| postgres             | CMD-SHELL  | `pg_isready -U deskmatch`                  |
| redis                | CMD        | `redis-cli ping`                           |
| opensearch           | CMD-SHELL  | `curl .../_cluster/health`                 |
| loki                 | CMD-SHELL  | `wget .../ready`                           |
| api-gateway          | CMD-SHELL  | `curl -f http://localhost:5000/health`     |
| Backend services     | HTTP       | `GET /health` (configured via Kubernetes)  |

### Depends On / Startup Order

```
postgres, redis, opensearch  (healthy first)
    │
    ├── auth-service
    ├── core-service
    ├── notification-service
    ├── search-service
    │
    └── api-gateway  (depends on all above)
            │
            ├── frontend-web
            └── frontend-admin
```

## Volume Management

| Volume             | Mount                             | Persistence |
|--------------------|-----------------------------------|-------------|
| postgres-data      | /var/lib/postgresql/data          | Database files |
| redis-data         | /data                             | AOF append-only file |
| opensearch-data    | /usr/share/opensearch/data        | Lucene index files |
| prometheus-data    | /prometheus                       | TSDB time series |
| grafana-data       | /var/lib/grafana                  | Dashboards, users |
| loki-data          | /tmp/loki                          | Log chunks |

Volumes are persisted across `docker compose down`. Use `docker compose down -v` to remove all volumes.

## Network Configuration

All services connect to a single user-defined bridge network:

```yaml
networks:
  deskmatch-network:
    driver: bridge
```

Service-to-service communication uses the container name as hostname (e.g., `postgres`, `redis`, `opensearch`, `auth-service`, `api-gateway`).

## Environment Variables Reference

### PostgreSQL

| Variable                     | Default        |
|------------------------------|----------------|
| POSTGRES_USER                | deskmatch      |
| POSTGRES_PASSWORD            | deskmatch123!  |
| POSTGRES_MULTIPLE_DATABASES  | deskmatch_auth,deskmatch_core,deskmatch_notification |

### Redis

| Variable                     | Default |
|------------------------------|---------|
| Connection string            | redis:6379,password=,ssl=false,abortConnect=false |

### OpenSearch

| Variable                              | Default       |
|---------------------------------------|---------------|
| discovery.type                        | single-node   |
| plugins.security.disabled             | true          |
| OPENSEARCH_INITIAL_ADMIN_PASSWORD     | DeskMatch123! |

### JWT

| Variable                              | Default                        |
|---------------------------------------|--------------------------------|
| JWT_SECRET                            | super-secret-key-change-in-production-min-32-chars!! |
| JWT_ISSUER                            | DeskMatch                      |
| JWT_AUDIENCE                          | DeskMatch                      |
| JWT_ACCESS_TOKEN_EXPIRATION_MINUTES   | 15                             |
| JWT_REFRESH_TOKEN_EXPIRATION_DAYS     | 7                              |

### Notification

| Variable               | Default                    |
|------------------------|----------------------------|
| NOTIFICATION_PROVIDER  | smtp                       |
| SMTP_HOST              | mailhog (dev)              |
| SMTP_PORT              | 1025                       |
| SMTP_FROM_EMAIL        | noreply@deskmatch.com      |
| SMTP_FROM_NAME         | DeskMatch                  |

### Grafana

| Variable                          | Default |
|-----------------------------------|---------|
| GF_SECURITY_ADMIN_PASSWORD        | admin   |

## Useful Commands

```bash
# Rebuild a single service
docker compose up -d --build core-service

# View logs for multiple services
docker compose logs -f api-gateway auth-service

# Stop everything
docker compose down

# Stop and remove volumes (reset everything)
docker compose down -v

# Run a single infrastructure service
docker compose up -d postgres redis

# Shell into a running container
docker exec -it deskmatch-core-service /bin/sh

# Check resource usage
docker stats
```