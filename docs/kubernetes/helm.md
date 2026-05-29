# Helm Chart Documentation

## Chart Structure

```
infrastructure/helm/deskmatch/
├── Chart.yaml                      # Chart metadata (v0.1.0, appVersion 1.0.0)
├── values.yaml                     # Default values (production base)
├── values-development.yaml         # Development overrides
├── values-staging.yaml             # Staging overrides
├── values-production.yaml          # Production overrides
└── templates/
    ├── _helpers.tpl                # Template helpers (name, labels, selectors)
    ├── namespace.yaml              # Namespace resource
    ├── configmap.yaml              # Application configuration
    ├── secrets.yaml                # Sensitive values
    ├── postgres.yaml               # PostgreSQL deployment + PVC + service
    ├── redis.yaml                  # Redis deployment + service
    ├── opensearch.yaml             # OpenSearch + Dashboards
    ├── deployment.yaml             # Backend services (generic template)
    ├── frontend-deployment.yaml    # Frontend deployments (generic template)
    ├── services.yaml               # ClusterIP services
    ├── ingress.yaml                # NGINX ingress rules
    ├── hpa.yaml                    # HorizontalPodAutoscalers
    └── monitoring.yaml             # Prometheus + Grafana + Loki
```

## Values Configuration

### Default Values (`values.yaml`)

Production-safe defaults with conservative resources:

```yaml
backendServices:
  api-gateway:
    replicas: 2
    resources:
      requests: { memory: "128Mi", cpu: "100m" }
      limits:   { memory: "256Mi", cpu: "250m" }
    probes:
      readiness: { initialDelaySeconds: 10, periodSeconds: 10 }
      liveness:  { initialDelaySeconds: 30, periodSeconds: 20 }
      startup:   { initialDelaySeconds: 5, periodSeconds: 5, failureThreshold: 30 }
```

### Per-Environment Overrides

| Setting              | Development        | Staging          | Production        |
|----------------------|-------------------|------------------|-------------------|
| replicas             | 1                 | 2                | 3+                |
| resources.requests   | min (64Mi/50m)    | medium (128Mi/100m) | high (256Mi/250m) |
| resources.limits     | low (128Mi/100m)  | medium (256Mi/250m) | high (512Mi/500m) |
| autoscaling.enabled  | false              | true (2-5)       | true (3-10)       |
| log level            | Debug              | Information      | Warning           |
| ingress.tls          | []                 | []               | Configured        |

### Key Values

```yaml
# Infrastructure
postgres:
  enabled: true
  image: { repository: postgres, tag: "16-alpine" }
  storage: { size: 10Gi }

redis:
  enabled: true
  image: { repository: redis, tag: "7-alpine" }

opensearch:
  enabled: true
  image: { repository: opensearchproject/opensearch, tag: "2.11.0" }

# Monitoring
monitoring:
  enabled: true
  prometheus: { image: { tag: "v2.50.0" } }
  grafana: { image: { tag: "10.3.0" }, adminPassword: "admin" }
  loki: { image: { tag: "2.9.0" } }

# Ingress
ingress:
  enabled: true
  className: nginx
  hosts:
    api: api.deskmatch.local
    web: web.deskmatch.local
    admin: admin.deskmatch.local
    grafana: grafana.deskmatch.local
    kibana: kibana.deskmatch.local

# Autoscaling
autoscaling:
  enabled: false
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
```

## Installation Commands

### Development

```bash
helm upgrade --install deskmatch infrastructure/helm/deskmatch \
  --namespace deskmatch \
  --create-namespace \
  --values infrastructure/helm/deskmatch/values-development.yaml \
  --set secrets.jwt.secret="$(openssl rand -base64 32)" \
  --set secrets.postgres.password="$(openssl rand -base64 16)" \
  --wait \
  --timeout 10m
```

### Staging

```bash
helm upgrade --install deskmatch infrastructure/helm/deskmatch \
  --namespace deskmatch \
  --create-namespace \
  --values infrastructure/helm/deskmatch/values-staging.yaml \
  --wait \
  --timeout 10m
```

### Production

```bash
helm upgrade --install deskmatch infrastructure/helm/deskmatch \
  --namespace deskmatch \
  --create-namespace \
  --values infrastructure/helm/deskmatch/values-production.yaml \
  --set-file secrets.jwt.secret=/run/secrets/jwt-secret \
  --set-file secrets.postgres.password=/run/secrets/db-password \
  --wait \
  --timeout 15m
```

### Template Rendering (dry-run)

```bash
# Render templates to stdout
helm template deskmatch infrastructure/helm/deskmatch \
  -f infrastructure/helm/deskmatch/values-development.yaml

# Lint chart
helm lint infrastructure/helm/deskmatch
```

### List and Status

```bash
helm list -n deskmatch
helm status deskmatch -n deskmatch
helm get values deskmatch -n deskmatch
```

## Upgrade and Rollback

### Upgrade

```bash
helm upgrade deskmatch infrastructure/helm/deskmatch \
  -n deskmatch \
  -f infrastructure/helm/deskmatch/values-production.yaml \
  --atomic \
  --cleanup-on-fail
```

The `--atomic` flag rolls back automatically if the upgrade fails.

### Rollback

```bash
# List revision history
helm history deskmatch -n deskmatch

# Rollback to previous revision
helm rollback deskmatch -n deskmatch

# Rollback to a specific revision
helm rollback deskmatch 3 -n deskmatch
```

### Uninstall

```bash
helm uninstall deskmatch -n deskmatch
```

Note: PersistentVolumeClaims are not deleted automatically. Remove them manually if needed:

```bash
kubectl delete pvc postgres-pvc -n deskmatch
```

## Template Design

### Generic Backend Deployment (`deployment.yaml`)

The backend template iterates over `.Values.backendServices` to generate deployments for `api-gateway`, `auth-service`, `core-service`, `search-service`, and `notification-service` using a single template file.

### Generic Frontend Deployment (`frontend-deployment.yaml`)

Similarly iterates over `.Values.frontends` for `frontend-web` and `frontend-admin`.

### ConfigMap

Environment variables from `.Values.config` are injected as a ConfigMap. All backend deployments reference it via:

```yaml
envFrom:
  - configMapRef:
      name: deskmatch-config
  - secretRef:
      name: deskmatch-secrets
```

### Secrets

Secret values from `.Values.secrets` are base64-encoded in the `secrets.yaml` template.

## Best Practices

1. **Always use `--wait`** to ensure deployments are healthy before marking the release as complete.
2. **Use `--atomic` for production upgrades** to auto-rollback on failure.
3. **Never commit actual secrets** to values files. Use `--set`, `--set-file`, or an external secrets manager.
4. **Lint before installing**: `helm lint infrastructure/helm/deskmatch`.
5. **Template dry-run** before applying to verify rendered output.
6. **Use Helm hooks** for migrations (pre-upgrade job that runs EF Core migrations before deployment update).