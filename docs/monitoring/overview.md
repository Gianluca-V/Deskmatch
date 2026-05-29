# Monitoring Documentation

## Overview

DeskMatch uses a three-pillar observability stack:

| Pillar      | Tool          | Purpose                                    |
|-------------|---------------|--------------------------------------------|
| Metrics     | Prometheus    | Scrape and store time-series metrics       |
| Logs        | Loki          | Aggregate and query structured logs        |
| Dashboards  | Grafana       | Visualize metrics and logs, alerting       |
| Traces      | OpenTelemetry | Distributed tracing (future)               |

## Prometheus Metrics

### Configuration

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: "api-gateway"          → api-gateway:5000/metrics
  - job_name: "auth-service"         → auth-service:5001/metrics
  - job_name: "core-service"         → core-service:5002/metrics
  - job_name: "search-service"       → search-service:5004/metrics
  - job_name: "notification-service" → notification-service:5005/metrics
  - job_name: "postgres"             → postgres-exporter:9187
  - job_name: "redis"                → redis-exporter:9121
  - job_name: "opensearch"           → opensearch:9200/_prometheus/metrics
```

### Application Metrics

Each .NET service exposes metrics via OpenTelemetry and the Prometheus exporter:

| Metric                                    | Type      | Description                            |
|-------------------------------------------|-----------|----------------------------------------|
| `http_requests_total`                     | Counter   | Total HTTP requests by method, status, endpoint |
| `http_request_duration_seconds`           | Histogram | Request latency distribution            |
| `http_requests_in_progress`               | Gauge     | Currently active requests               |
| `dotnet_runtime_memory_allocated_bytes`   | Gauge     | GC heap size                            |
| `dotnet_runtime_cpu_usage_ratio`          | Gauge     | Process CPU usage                       |
| `dotnet_runtime_thread_pool_queue_length`  | Gauge     | Thread pool saturation                  |

### Infrastructure Metrics

| Source      | Key Metrics                                              |
|-------------|----------------------------------------------------------|
| PostgreSQL  | Connections active, transactions/sec, locks, cache hits  |
| Redis       | Memory used, connected clients, hit/miss ratio, evictions |
| OpenSearch  | Cluster health, doc count, JVM heap, query latency       |

## Grafana Dashboards

### Data Sources

```yaml
datasources:
  - name: Prometheus        # http://prometheus:9090 (default)
  - name: Loki              # http://loki:3100
```

### Pre-built Dashboards

| Dashboard              | Panels                                                              |
|------------------------|---------------------------------------------------------------------|
| **API Overview**       | Request rate, latency P95/P99, error rate, active connections       |
| **Service Health**     | CPU, memory, GC pauses, thread pool per service                     |
| **Database**           | Connection pool, transaction rate, deadlocks, cache hit ratio       |
| **Cache**              | Hit/miss ratio, memory usage, eviction rate, key count              |
| **Search**             | Query latency, indexing rate, cluster health                        |
| **Business Metrics**   | Reservations per hour, active offices, user registrations           |

### Dashboard Location

Dashboard JSON files are stored in:

```
infrastructure/monitoring/grafana/dashboards/
```

### Importing

Dashboards can be imported via:
1. Grafana UI → Dashboards → Import → Upload JSON
2. Provisioning (place JSON in Grafana's provisioning directory)
3. Grafana API

## Loki Log Aggregation

### Configuration

Loki receives structured logs from each .NET service via the **Serilog Grafana Loki sink**.

```yaml
# loki-config.yml
auth_enabled: false

server:
  http_listen_port: 3100

ingester:
  lifecycler: ...
  chunk_idle_period: 5m
  chunk_retain_period: 30s

schema_config:
  configs:
    - from: 2024-01-01
      store: tsdb
      index:
        prefix: loki_index_
        period: 24h
      object_store: filesystem
      schema: v13
```

### Log Structure

```json
{
  "Timestamp": "2024-01-01T12:00:00Z",
  "Level": "Information",
  "MessageTemplate": "User {UserId} created office {OfficeId}",
  "Properties": {
    "UserId": "abc-123",
    "OfficeId": "def-456",
    "SourceContext": "DeskMatch.CoreService.Offices.Create.CreateOfficeCommandHandler",
    "CorrelationId": "xyz-789"
  }
}
```

### Querying in Grafana

Use label matchers:
```
{app="core-service"} |= "Office" | json
{app="auth-service"} | logfmt | level = "Error"
```

### Log Labels

| Label      | Source                                     |
|------------|--------------------------------------------|
| `app`      | Serilog property: service name             |
| `level`    | Serilog level (Information, Warning, Error)|
| `env`      | ASPNETCORE_ENVIRONMENT                      |

## Alerting Rules

### Proposed Alert Rules (Grafana/Prometheus)

| Alert                              | Condition                                      | Severity |
|------------------------------------|------------------------------------------------|----------|
| High Error Rate                    | `rate(http_requests_total{status=~"5.."}[5m]) > 0.05` | Critical |
| High Latency                       | `histogram_quantile(0.99, http_request_duration_seconds) > 2` | Warning  |
| Service Down                       | `up == 0`                                       | Critical |
| High CPU Usage                     | `rate(dotnet_runtime_cpu_usage_ratio[5m]) > 0.8`| Warning  |
| High Memory                         | `dotnet_runtime_memory_allocated_bytes > 200MB` | Warning  |
| Database Connection Pool Exhausted | `npgsql_connection_pool_total < 1`             | Critical |
| Redis Connection Failure           | `redis_connected_clients < 1`                  | Critical |
| OpenSearch Cluster Red             | `opensearch_cluster_health_status != "green"`  | Warning  |

### Alert Manager Integration

Configure Grafana Alerting or Prometheus Alertmanager to route alerts to:
- Email
- Slack
- PagerDuty (production)

## Health Check Endpoints

Every service exposes a health check:

```
GET /health    → 200 OK (healthy) or 503 (unhealthy)
GET /metrics   → Prometheus text format metrics
```

The health endpoint checks:
- Database connectivity
- Redis connectivity (core-service)
- OpenSearch connectivity (core-service, search-service)
- Disk space
- Self process status

Used by Kubernetes readiness, liveness, and startup probes.

## Monitoring Endpoints (Docker Compose)

| Service    | URL                        | Auth         |
|------------|----------------------------|--------------|
| Grafana    | http://localhost:3000      | admin/admin  |
| Prometheus | http://localhost:9090      | None         |
| Loki       | http://localhost:3100      | None         |

## Adding a New Dashboard

1. Create the dashboard in Grafana UI.
2. Export as JSON (Dashboard Settings → JSON Model).
3. Save to `infrastructure/monitoring/grafana/dashboards/{name}.json`.
4. Update the Grafana deployment to mount the new file if using provisioning.

## Logging Configuration (Serilog)

```json
"Serilog": {
  "MinimumLevel": "Debug",
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } },
    {
      "Name": "GrafanaLoki",
      "Args": {
        "uri": "http://loki:3100",
        "labels": [
          { "key": "app", "value": "core-service" },
          { "key": "env", "value": "Development" }
        ]
      }
    }
  ]
}
```