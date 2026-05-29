# Development Setup

## IDE Configuration

### Visual Studio Code

Recommended extensions:

- **C# Dev Kit** (Microsoft) — IntelliSense, debugging, solution explorer
- **ES7+ React/Redux/React-Native snippets** — React snippets
- **Prettier** — Code formatting
- **ESLint** — Linting
- **Docker** — Dockerfile and compose support
- **Kubernetes** (Microsoft) — K8s manifest support

Workspace settings (`.vscode/settings.json`):

```json
{
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.organizeImports": true
  },
  "omnisharp.enableEditorConfigSupport": true,
  "omnisharp.enableRoslynAnalyzers": true
}
```

### JetBrains Rider

- Open `DeskMatch.sln`
- Rider auto-detects the solution structure, shared properties from `Directory.Build.props`, and the `nuget.config`
- Enable `EditorConfig` support (Settings → Editor → Code Style → Enable EditorConfig support)
- Use the built-in Docker and Kubernetes plugins

### Visual Studio 2022

- Open `DeskMatch.sln`
- Ensure **ASP.NET and web development** and **.NET desktop development** workloads are installed
- Enable Docker support: Right-click project → Add → Container Orchestrator Support

## Running Services Individually

For local development, run infrastructure dependencies via Docker and services via `dotnet run`.

### 1. Start infrastructure

```bash
cd infrastructure/docker
docker compose up -d postgres redis opensearch
```

Verify:

```bash
docker compose ps
```

### 2. Run a backend service

```bash
cd apps/auth-service
dotnet run --urls http://localhost:5001
```

Use `dotnet watch run` for hot reload.

### 3. Run a frontend

```bash
cd apps/frontend-web
npm install
npm run dev
```

Access at http://localhost:5173.

## Service Startup Order

```
1. postgres, redis, opensearch          (infrastructure)
2. opensearch-init                      (index creation + seed data)
3. auth-service                         (other services depend on JWT validation)
4. core-service, notification-service   (independent of each other)
5. search-service                       (depends on OpenSearch)
6. api-gateway                          (depends on all backend services)
7. frontend-web, frontend-admin         (depends on api-gateway)
```

## Database Migrations

### EF Core Tooling

Install EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

### Create a migration

```bash
cd apps/auth-service
dotnet ef migrations add AddNewColumn --context AuthDbContext
```

### Apply migrations

```bash
dotnet ef database update --context AuthDbContext
```

### SQL scripts

Migrations in Development are auto-applied at startup via `DbContext.Database.Migrate()`. Production deployments should run migrations as part of the CI/CD pipeline or use idempotent SQL scripts generated via:

```bash
dotnet ef migrations script --idempotent --output migrate.sql
```

## Debugging Tips

### Backend (Rider / VS Code / VS)

- Set breakpoints in controllers, command handlers, and validators.
- In VS Code, use the `.NET Core Launch` configuration. Attach to the running process or use `dotnet run` with debug attach.
- In Rider, right-click the project → Debug.
- Inspect CQRS pipeline behavior by adding a logging decorator/handler.

### HTTP Requests

Use `tools/*.http` files or the built-in Swagger UI.

### Inspect Database

```bash
docker exec -it deskmatch-postgres psql -U deskmatch -d deskmatch_core
```

Useful commands:

```sql
\dt                  -- list tables
\d "Offices"         -- describe table
SELECT * FROM "Offices";
```

### Inspect Redis

```bash
docker exec -it deskmatch-redis redis-cli
```

Useful commands:

```
KEYS *               -- list all keys
GET offices:list     -- get cached value
FLUSHALL             -- clear all cache
TTL offices:list     -- check TTL
```

### Inspect OpenSearch

```bash
curl -s "http://localhost:9200/offices/_search?pretty" -u admin:DeskMatch123!
```

## Code Style and Conventions

### .editorconfig

The root `.editorconfig` defines:

- C# naming conventions (PascalCase for public, camelCase for private, `_` prefix for fields)
- Indentation: 4 spaces for C#, 2 spaces for JSX/JSON/YAML
- `var` usage preferences
- Nullable analysis enabled

### Directory.Build.props

All projects inherit:

```xml
<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

### C# Conventions

- Use **file-scoped namespaces**.
- Use **primary constructors** where applicable.
- Command/Query names: `CreateOfficeCommand`, `GetOfficeByIdQuery`.
- Handler names: `CreateOfficeCommandHandler`, `GetOfficeByIdQueryHandler`.
- Validator names: `CreateOfficeCommandValidator`.
- Controllers use `[ApiController]` attribute routing.
- Return `Results<T>` pattern or `IActionResult`.

### React Conventions

- Components: PascalCase files (`OfficeCard.jsx`).
- Hooks: camelCase with `use` prefix (`useOffices.js`).
- Services: camelCase files in `src/services/` (`officeService.js`).
- Use functional components with hooks exclusively.
- State management: TanStack Query for server state, React Context for app state.