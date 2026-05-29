# Runbooks: Common Issues

## Service Won't Start

### Symptom
Docker container exits immediately, or Kubernetes pod stays in `CrashLoopBackOff`.

### Diagnosis

```bash
docker compose ps                          # Show all container statuses
docker compose logs <service-name>         # View container logs
kubectl describe pod <pod-name> -n deskmatch
kubectl logs <pod-name> -n deskmatch --previous
```

### Resolution

1. **Environment variable missing** — Check `infrastructure/docker/.env` or Kubernetes ConfigMap/Secret. Ensure `ConnectionStrings__DefaultConnection`, `Jwt__Secret`, and `ASPNETCORE_URLS` are set.
2. **Port already in use** — Change the host port in `docker-compose.yml` or check `netstat -ano | findstr :5001`.
3. **DLL not found** — Ensure the Dockerfile copies all required projects and the publish command succeeds. Rebuild with `--no-cache`.
4. **Startup probe failure** — Increase `initialDelaySeconds` and `failureThreshold` in Kubernetes probes.

---

## Database Connection Issues

### Symptom
`NpgsqlException`, `Host not found`, `Connection refused`, or health check fails with DB error.

### Diagnosis

```bash
docker exec -it deskmatch-postgres pg_isready -U deskmatch
docker compose logs postgres
kubectl logs deployment/postgres -n deskmatch
```

### Resolution

1. **PostgreSQL not started yet** — Wait for the health check (10s interval, 5 retries). In K8s, check with `kubectl wait --for=condition=ready pod -l app=postgres -n deskmatch --timeout=120s`.
2. **Wrong connection string** — Verify hostname: `postgres` (Docker) or `postgres.deskmatch.svc.cluster.local` (K8s). Never use `localhost` from within a container.
3. **Authentication failed** — Check `POSTGRES_USER` and `POSTGRES_PASSWORD` match both the PostgreSQL service and the app connection string.
4. **Database doesn't exist** — Check that `init-multiple-databases.sh` ran successfully. The databases should be `deskmatch_auth`, `deskmatch_core`, `deskmatch_notification`.

### Quick Fix

```bash
docker compose restart postgres
docker compose restart core-service
```

---

## OpenSearch Cluster Issues

### Symptom
Search endpoints return 503, `OpenSearchConnectionException`, or cluster health is red.

### Diagnosis

```bash
curl -u admin:DeskMatch123! http://localhost:9200/_cluster/health?pretty
docker compose logs opensearch
```

### Resolution

1. **vm.max_map_count too low** — Linux requires at least 262144. Fix:
   ```bash
   sudo sysctl -w vm.max_map_count=262144
   ```
   On Docker Desktop, this setting is under Settings → Advanced.

2. **Cluster health is red** — If this is a single-node dev setup, check for corrupted shards. Delete and recreate:
   ```bash
   curl -X DELETE -u admin:DeskMatch123! http://localhost:9200/offices
   docker compose restart opensearch-init
   ```

3. **Out of disk space** — Check `GET _cat/allocation?v`. Free up space or increase volume size.

4. **Init container failed** — Check `docker compose logs opensearch-init`. The script may fail if OpenSearch wasn't ready. Restart it:
   ```bash
   docker compose up -d opensearch-init
   ```

---

## Redis Connection Issues

### Symptom
Core-service returns 503 or slow responses. Logs show `RedisConnectionException` or `StackExchange.Redis.RedisConnectionException`.

### Diagnosis

```bash
docker exec -it deskmatch-redis redis-cli ping
docker compose logs redis
curl localhost:6379
```

### Resolution

1. **Redis not started** — `docker compose up -d redis`.
2. **Connection string mismatch** — Should be `redis:6379,password=,ssl=false,abortConnect=false` in Docker Compose.
3. **Redis memory full** — Check with `INFO memory`. If `used_memory` ≈ `maxmemory`, increase limits or clear cache:
   ```bash
   docker exec -it deskmatch-redis redis-cli FLUSHALL
   ```
4. **Degraded mode** — If `abortConnect=false`, the service may start without Redis. The cache layer will be bypassed (all queries hit the database directly). Fix Redis and restart the service.

---

## JWT Token Expired

### Symptom
API returns 401 Unauthorized with message "token expired".

### Diagnosis

```bash
# Decode token at https://jwt.io or via CLI
echo "eyJ..." | cut -d'.' -f2 | base64 -d
```

Check the `exp` claim.

### Resolution

1. **Client-side**: Call `POST /api/auth/refresh` with the refresh token to get a new access token.
2. **Refresh token also expired** (7 days): User must re-authenticate via `POST /api/auth/login`.
3. **Clock skew**: Ensure all containers/services have synchronized clocks. In Docker, the host clock is shared.
4. **JWT_SECRET mismatch**: All services must use the **same** `Jwt__Secret` value. If a service has a different secret, tokens issued by auth-service will fail validation.

---

## Docker Build Failures

### Symptom
`docker compose build` or `docker build` fails with an error.

### Common Issues

| Error                                  | Cause & Fix                                                   |
|----------------------------------------|---------------------------------------------------------------|
| `COPY failed: file not found`         | Check file paths in Dockerfile. Context is set to `../../.` (project root) |
| `RUN dotnet restore` failed           | NuGet source unreachable. Check `nuget.config` and network connectivity |
| `error NU1101: Unable to find package`| Package not available. Run `dotnet restore` locally first, check NuGet sources |
| `npm ERR!` during frontend build      | `package-lock.json` outdated. Delete and regenerate: `rm package-lock.json && npm install` |
| Out of disk space                     | `docker system prune -a` to remove unused images/volumes     |

### Resolution

1. Clean build:
   ```bash
   docker compose build --no-cache <service-name>
   ```
2. For NuGet issues, check that `nuget.config` has valid sources. If behind a proxy, configure it in `nuget.config`.
3. For npm issues, clear the npm cache:
   ```bash
   npm cache clean --force
   npm ci
   ```

---

## Kubernetes Pod CrashLoopBackOff

### Symptom
`kubectl get pods` shows `CrashLoopBackOff` for one or more pods.

### Diagnosis

```bash
kubectl describe pod <pod-name> -n deskmatch
kubectl logs <pod-name> -n deskmatch --previous
kubectl get events -n deskmatch --sort-by='.lastTimestamp'
```

### Resolution

1. **Image pull error** — The image doesn't exist in the registry. Check `image: deskmatch/auth-service:latest` exists locally or in the registry.
2. **ConfigMap/Secret missing** — `kubectl get configmap deskmatch-config -n deskmatch`. If missing, apply:
   ```bash
   kubectl apply -k infrastructure/kubernetes/base
   ```
3. **OOMKilled** — Pod exceeds memory limit. Increase `limits.memory` in the deployment or the Helm values.
4. **Readiness probe never passes** — The service may take longer than `initialDelaySeconds` to start. Increase it and the pod is `CrashLoopBackOff`, check if `/health` endpoint is actually working. Port-forward and test:
   ```bash
   kubectl port-forward <pod-name> 5001:5001 -n deskmatch
   curl http://localhost:5001/health
   ```

---

## How to Restart Services

### Docker Compose

```bash
# Restart a single service
docker compose restart core-service

# Restart everything
docker compose restart

# Rebuild and restart
docker compose up -d --build core-service

# Full reset
docker compose down && docker compose up -d --build
```

### Kubernetes

```bash
# Restart a deployment (rolling restart)
kubectl rollout restart deployment/auth-service -n deskmatch

# Delete pod (it will be recreated by the ReplicaSet)
kubectl delete pod -l app=auth-service -n deskmatch

# Scale to 0 and back
kubectl scale deployment/auth-service --replicas=0 -n deskmatch
kubectl scale deployment/auth-service --replicas=2 -n deskmatch
```

---

## How to Check Logs

### Docker Compose

```bash
# All services
docker compose logs

# Single service, follow mode
docker compose logs -f api-gateway

# Last 100 lines
docker compose logs --tail=100 core-service

# With timestamps
docker compose logs -t auth-service

# Filter by keyword (using grep)
docker compose logs core-service | Select-String "Error"
```

### Kubernetes

```bash
# Current logs
kubectl logs deployment/core-service -n deskmatch

# Previous instance logs (if container crashed)
kubectl logs deployment/core-service -n deskmatch --previous

# Follow mode
kubectl logs -f deployment/core-service -n deskmatch

# All pods for a deployment
kubectl logs -l app=core-service -n deskmatch --all-containers=true

# Last 1 hour
kubectl logs deployment/core-service -n deskmatch --since=1h
```

### Loki / Grafana

- Open Grafana: http://localhost:3000
- Explore → Select Loki data source
- Query: `{app="core-service"} |= "Error"`
- Filter by time range

---

## Rollback Procedure

### Docker Compose

```bash
# Pull a previous image tag
docker compose down
# Edit docker-compose.yml and change image tag or rebuild from a known-good commit
docker compose up -d --build
```

### Kubernetes (kustomize)

```bash
# Revert the kustomize changes in git
git revert <bad-commit-hash>
kubectl apply -k infrastructure/kubernetes/production
```

### Kubernetes (Helm)

```bash
# List history
helm history deskmatch -n deskmatch

# Rollback to previous revision
helm rollback deskmatch -n deskmatch

# Rollback to specific revision
helm rollback deskmatch 3 -n deskmatch

# Verify
kubectl get pods -n deskmatch
helm status deskmatch -n deskmatch
```

### Emergency Fallback

If a full rollback isn't possible:

1. **Scale down the faulty service**:
   ```bash
   kubectl scale deployment/core-service --replicas=0 -n deskmatch
   ```
2. **Manually edit the deployment** to use a known-good image:
   ```bash
   kubectl set image deployment/core-service core-service=deskmatch/core-service:v1.0.0 -n deskmatch
   ```
3. **Scale back up**:
   ```bash
   kubectl scale deployment/core-service --replicas=2 -n deskmatch
   ```

---

## Quick Reference: Health Check URLs

| Service              | URL                                          | Expected |
|----------------------|----------------------------------------------|----------|
| API Gateway          | http://localhost:5000/health                 | 200 OK   |
| Auth Service         | http://localhost:5001/health                 | 200 OK   |
| Core Service         | http://localhost:5002/health                 | 200 OK   |
| Search Service       | http://localhost:5004/health                 | 200 OK   |
| Notification Service | http://localhost:5005/health                 | 200 OK   |
| PostgreSQL           | `docker exec deskmatch-postgres pg_isready`  | accepting connections |
| Redis                | `docker exec deskmatch-redis redis-cli ping` | PONG     |
| OpenSearch           | http://localhost:9200/_cluster/health         | green/yellow status |
| Prometheus           | http://localhost:9090/-/healthy              | Healthy  |
| Grafana              | http://localhost:3000/api/health             | ok       |