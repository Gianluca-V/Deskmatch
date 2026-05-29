# Getting Started

## Prerequisites

### Required Software

| Tool              | Minimum Version | Check Command            |
|-------------------|-----------------|--------------------------|
| Docker Desktop    | 24.0+           | `docker --version`       |
| .NET 8 SDK        | 8.0.100         | `dotnet --version`       |
| Node.js           | 20.x LTS        | `node --version`         |
| Git               | 2.40+           | `git --version`          |

### Optional Tools

| Tool              | Purpose                          |
|-------------------|----------------------------------|
| kubectl           | Interact with local K8s cluster  |
| Helm 3            | Deploy via Helm chart            |
| k6                | Load testing                     |
| pgAdmin / DBeaver | Inspect PostgreSQL databases     |
| RedisInsight      | Inspect Redis cache              |

## Clone and Setup

```bash
git clone https://github.com/your-org/DeskMatch.git
cd DeskMatch
```

Copy environment file:

```bash
cp infrastructure/docker/.env.example infrastructure/docker/.env
```

Edit secrets in `infrastructure/docker/.env` if needed, especially:
- `JWT_SECRET` — must be at least 32 characters in production
- `POSTGRES_PASSWORD`
- `OPENSEARCH_INITIAL_ADMIN_PASSWORD`

## Docker Compose: Build and Run

Start all services:

```bash
cd infrastructure/docker
docker compose up -d --build
```

Check service health:

```bash
docker compose ps
```

All containers should show `healthy` or `Up` status.

Pull up logs for a specific service:

```bash
docker compose logs -f auth-service
docker compose logs -f core-service
```

### What Gets Started

| Container                  | Internal Port | Host Port |
|----------------------------|---------------|-----------|
| deskmatch-postgres         | 5432          | 5432      |
| deskmatch-redis            | 6379          | 6379      |
| deskmatch-opensearch       | 9200          | 9200      |
| deskmatch-opensearch-init  | —             | —         |
| deskmatch-auth-service     | 5001          | 5001      |
| deskmatch-core-service     | 5002          | 5002      |
| deskmatch-search-service   | 5004          | 5004      |
| deskmatch-notification-service | 5005      | 5005      |
| deskmatch-api-gateway      | 5000          | 5000      |
| deskmatch-frontend-web      | 80            | 3001      |
| deskmatch-frontend-admin    | 80            | 3002      |
| deskmatch-prometheus       | 9090          | 9090      |
| deskmatch-grafana           | 3000          | 3000      |
| deskmatch-loki              | 3100          | 3100      |
| deskmatch-opensearch-dashboards | 5601     | 5601      |

## Accessing Services

### Swagger UI

The API Gateway aggregates Swagger docs:

- **Gateway**: http://localhost:5000/swagger
- **Auth Service**: http://localhost:5001/swagger
- **Core Service**: http://localhost:5002/swagger
- **Search Service**: http://localhost:5004/swagger
- **Notification Service**: http://localhost:5005/swagger

### Frontend URLs

- **Customer Web**: http://localhost:3001
- **Admin Dashboard**: http://localhost:3002

### Monitoring

- **Grafana**: http://localhost:3000 (admin/admin)
- **Prometheus**: http://localhost:9090
- **OpenSearch Dashboards**: http://localhost:5601

### Default Credentials

| System          | Username | Password       |
|-----------------|----------|----------------|
| Admin user      | admin@deskmatch.com | (hashed — use POST /api/auth/login) |
| Grafana         | admin    | admin          |
| OpenSearch      | admin    | DeskMatch123!  |
| PostgreSQL      | deskmatch | deskmatch123! |

## Development Workflow

### Backend (Hot Reload)

Run a single service with `dotnet watch`:

```bash
cd apps/core-service
dotnet watch run
```

This will auto-rebuild on code changes. The service connects to infrastructure (postgres, redis, opensearch) from Docker.

### Frontend (Vite Dev Server)

```bash
cd apps/frontend-web
npm install
npm run dev        # starts on http://localhost:5173
```

The Vite dev server proxies `/api` requests to the gateway at `http://localhost:5000`.

## Common Issues and Solutions

### Port conflicts

If ports are already in use, edit `infrastructure/docker/docker-compose.yml` and change the host port (left side of the colon):

```yaml
ports:
  - "15432:5432"   # host:container
```

### Docker image build fails

```bash
docker compose build --no-cache <service-name>
```

### OpenSearch fails to start (vm.max_map_count)

Linux/macOS:

```bash
sudo sysctl -w vm.max_map_count=262144
```

Docker Desktop (Windows): the setting is in Docker Desktop settings → Advanced.

### Connection refused to PostgreSQL

Wait for the health check (10s interval, 5 retries). If it persists:

```bash
docker compose logs postgres
```

### JWT token expired

Access tokens expire after 15 minutes. Call `POST /api/auth/refresh` with the refresh token to get a new access token.

### Database migrations

Migrations run automatically at startup in Development mode. For production, use:

```bash
dotnet ef database update --project apps/auth-service
```