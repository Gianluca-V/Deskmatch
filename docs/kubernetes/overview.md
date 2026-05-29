# Kubernetes Documentation

## Cluster Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Namespace: deskmatch                                       │
│                                                             │
│  ┌───────────────────────┐  ┌───────────────────────┐     │
│  │   NGINX Ingress       │  │   HorizontalPod        │     │
│  │   Controller          │  │   Autoscalers          │     │
│  └──────────┬────────────┘  └───────────────────────┘     │
│             │                                               │
│    ┌────────┼────────┬────────┬────────┐                   │
│    ▼        ▼        ▼        ▼        ▼                   │
│  ┌──────┐┌──────┐┌──────┐┌──────┐┌──────┐                 │
│  │Front ││Front ││ API  ││ Auth ││ Core │                 │
│  │ Web  ││Admin ││ GW   ││ Svc  ││ Svc  │                 │
│  └──────┘└──────┘└──────┘└───┬──┘└──┬───┘                 │
│                              │       │                     │
│    ┌──────┐┌──────┐          │       │                     │
│    │Search││Notif │          │       │                     │
│    │ Svc  ││ Svc  │          │       │                     │
│    └──┬───┘└──┬───┘          │       │                     │
│       │       │              │       │                     │
│  ─ ─ ─│─ ─ ─ ─│─ ─ ─ ─ ─ ─ ─│─ ─ ─ ─│─ ─ ─                │
│       ▼       ▼              ▼       ▼                     │
│  ┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐     │
│  │Open    ││ Postgre││ Redis  ││ Prom   ││ Grafana│     │
│  │Search  ││ SQL    ││        ││ etheus ││ + Loki │     │
│  └────────┘└────────┘└────────┘└────────┘└────────┘     │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │  ConfigMap: deskmatch-config                         │ │
│  │  Secret:    deskmatch-secrets                        │ │
│  │  PVC:       postgres-pvc (10Gi)                      │ │
│  └──────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Namespace Strategy

A single `deskmatch` namespace hosts all resources. Environment isolation is achieved via **Kustomize overlays**:

```
infrastructure/kubernetes/
├── base/                    # Common manifests
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secrets.yaml
│   ├── ingress.yaml
│   ├── hpa.yaml
│   ├── api-gateway-deployment.yaml
│   ├── auth-service-deployment.yaml
│   ├── core-service-deployment.yaml
│   ├── search-service-deployment.yaml
│   ├── notification-service-deployment.yaml
│   ├── frontend-web-deployment.yaml
│   ├── frontend-admin-deployment.yaml
│   ├── postgres-deployment.yaml
│   ├── redis-deployment.yaml
│   ├── opensearch-deployment.yaml
│   ├── opensearch-dashboards-deployment.yaml
│   ├── prometheus-deployment.yaml
│   ├── grafana-deployment.yaml
│   └── loki-deployment.yaml
├── development/
│   └── kustomization.yaml    # Override replicas, resources, images
├── staging/
│   └── kustomization.yaml
└── production/
    └── kustomization.yaml
```

## Deployment Details

### API Gateway

```yaml
replicas: 2
image: deskmatch/api-gateway:latest
port: 5000
service: ClusterIP:5000
probes:
  readiness: GET /health (initialDelay: 10s, period: 10s)
  liveness:  GET /health (initialDelay: 30s, period: 20s)
  startup:   GET /health (initialDelay: 5s, period: 5s, failureThreshold: 30)
resources:
  requests: 128Mi mem, 100m CPU
  limits:   256Mi mem, 250m CPU
```

### Backend Services (auth, core, search, notification)

All follow the same pattern with port variations:

| Service             | Port | Replicas | Dependencies (env)                          |
|---------------------|------|----------|---------------------------------------------|
| auth-service        | 5001 | 2        | PostgreSQL, JWT secret                      |
| core-service        | 5002 | 2        | PostgreSQL, Redis, OpenSearch, JWT          |
| search-service      | 5004 | 2        | OpenSearch, JWT                             |
| notification-service | 5005 | 2        | PostgreSQL, SMTP, JWT                       |

All share:
- ConfigMap `deskmatch-config` and Secret `deskmatch-secrets` via `envFrom`
- Health probes at `GET /health`
- Standard resource requests/limits

### Frontends

| Service          | Port | Replicas | Image                           |
|------------------|------|----------|---------------------------------|
| frontend-web      | 80   | 2        | deskmatch/frontend-web:latest   |
| frontend-admin    | 80   | 2        | deskmatch/frontend-admin:latest |

### Infrastructure

| Resource      | Replicas | Storage          | Notes                      |
|---------------|----------|------------------|----------------------------|
| postgres      | 1        | 10Gi PVC         | Single instance, read probe: pg_isready |
| redis         | 1        | —                | No PVC in K8s (ephemeral cache) |
| opensearch    | 1        | —                | Single-node discovery mode |
| opensearch-dashboards | 1 | —                | Connects to opensearch |
| prometheus    | 1        | —                | Scrapes /metrics on each service |
| grafana       | 1        | —                | Admin password: `admin`    |
| loki           | 1        | —                | Receives Serilog Loki sink |

## Service Types

All services use **ClusterIP** (internal only). External access is via **NGINX Ingress**.

## Ingress Configuration

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: deskmatch-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
    - host: api.deskmatch.local      → api-gateway:5000
    - host: web.deskmatch.local      → frontend-web:80
    - host: admin.deskmatch.local    → frontend-admin:80
    - host: grafana.deskmatch.local  → grafana:3000
    - host: kibana.deskmatch.local   → opensearch-dashboards:5601
```

For local development, add these entries to your hosts file:

```
127.0.0.1 api.deskmatch.local
127.0.0.1 web.deskmatch.local
127.0.0.1 admin.deskmatch.local
127.0.0.1 grafana.deskmatch.local
127.0.0.1 kibana.deskmatch.local
```

## HPA Setup

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: api-gateway-hpa
spec:
  scaleTargetRef:
    name: api-gateway
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          averageUtilization: 70
```

Each backend service can be auto-scaled by enabling `autoscaling.enabled: true` in the Helm values.

## Secrets Management

Secrets are stored in `deskmatch-secrets` (Opaque type) and referenced via `secretKeyRef` in deployments:

| Key                     | Purpose                    |
|-------------------------|----------------------------|
| POSTGRES_PASSWORD       | Database password          |
| JWT_SECRET              | JWT signing key            |
| OPENSEARCH_PASSWORD     | OpenSearch admin password  |
| REDIS_PASSWORD           | Redis password             |
| RABBITMQ_PASSWORD       | RabbitMQ password          |
| SMTP_PASSWORD           | SMTP password              |
| SMTP_USER               | SMTP username              |
| SENDGRID_API_KEY        | SendGrid API key           |
| STORAGE_ACCESS_KEY      | MinIO access key           |
| STORAGE_SECRET_KEY      | MinIO secret key           |

**Production note**: Use a secrets management solution (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, or Sealed Secrets) instead of base64-encoded Kubernetes Secrets.

## Deployment Commands

### Apply base manifests

```bash
kubectl apply -k infrastructure/kubernetes/base
```

### Apply environment overlay

```bash
# Development
kubectl apply -k infrastructure/kubernetes/development

# Staging
kubectl apply -k infrastructure/kubernetes/staging

# Production
kubectl apply -k infrastructure/kubernetes/production
```

### Check rollout status

```bash
kubectl rollout status deployment/auth-service -n deskmatch
```

### Port-forward for local access

```bash
kubectl port-forward svc/api-gateway 5000:5000 -n deskmatch
kubectl port-forward svc/frontend-web 3001:80 -n deskmatch
kubectl port-forward svc/grafana 3000:3000 -n deskmatch
```