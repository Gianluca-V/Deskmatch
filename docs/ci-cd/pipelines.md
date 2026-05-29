# CI/CD Pipelines

## Overview

DeskMatch uses **GitHub Actions** for Continuous Integration and Continuous Deployment. The workflow files reside in `.github/workflows/`.

## GitHub Actions Workflows

### ci.yml — Continuous Integration

**Trigger**: Push to `main`, `develop`, and Pull Requests.

**Jobs**:

| Job                | Description                                          |
|--------------------|------------------------------------------------------|
| Build .NET         | Restore, build, test all .NET projects              |
| Build Frontend     | Install, lint, build both React apps                 |
| Docker Lint        | Lint Dockerfiles with Hadolint                        |
| Code Analysis      | Roslyn analyzers, editorconfig validation            |

#### Build Matrix (.NET)

```yaml
strategy:
  matrix:
    project:
      - auth-service
      - core-service
      - search-service
      - notification-service
      - api-gateway
    configuration: [Debug, Release]
```

Steps:
1. Checkout code
2. Setup .NET 8 SDK
3. Restore NuGet packages (cached)
4. Build with `dotnet build -c ${{ matrix.configuration }}`
5. Run tests with `dotnet test -c ${{ matrix.configuration }}`

#### Build Matrix (Frontend)

```yaml
strategy:
  matrix:
    app: [frontend-web, frontend-admin]
```

Steps:
1. Setup Node.js 20
2. Install dependencies (`npm ci`)
3. Lint (`npm run lint`)
4. Build (`npm run build`)

### docker-publish.yml — Docker Image Build & Push

**Trigger**: Push to `main` or tag starting with `v*`.

| Job              | Description                                   |
|------------------|-----------------------------------------------|
| Build & Push     | Build all Docker images, push to registry     |

Steps:
1. Login to Docker Hub / GitHub Container Registry
2. Build and push images with `docker/build-push-action`

Image tagging strategy:
- `main` branch → `latest` tag
- Git tags (`v1.0.0`) → `1.0.0`, `1.0`, `1` tags
- All builds → `sha-${GITHUB_SHA::8}` (immutable)

Images built:
```
deskmatch/api-gateway:latest
deskmatch/auth-service:latest
deskmatch/core-service:latest
deskmatch/search-service:latest
deskmatch/notification-service:latest
deskmatch/frontend-web:latest
deskmatch/frontend-admin:latest
```

### deploy-kubernetes.yml — Kubernetes Deployment

**Trigger**: On successful Docker publish (workflow_call) or manual dispatch.

| Job                  | Description                                         |
|----------------------|-----------------------------------------------------|
| Deploy Development   | Apply kustomize overlay → dev cluster                |
| Deploy Staging       | Apply kustomize overlay → staging cluster (on tag)  |
| Deploy Production    | Helm upgrade → production cluster (on release)      |

Steps:
1. Setup kubectl and authenticate
2. Set image tags via kustomize (`kustomize edit set image`)
3. Apply kustomize overlay
4. Wait for rollout with `kubectl rollout status`
5. Run smoke tests (optional)

## Deployment to Kubernetes

### Kustomize Flow

```bash
# Set image tags dynamically
cd infrastructure/kubernetes/development
kustomize edit set image deskmatch/auth-service=${REGISTRY}/auth-service:${TAG}

# Apply
kubectl apply -k .
```

### Helm Flow (Production)

```bash
helm upgrade --install deskmatch infrastructure/helm/deskmatch \
  -n deskmatch \
  -f infrastructure/helm/deskmatch/values-production.yaml \
  --set auth-service.image.tag=${TAG} \
  --set core-service.image.tag=${TAG} \
  --wait --atomic --timeout 10m
```

## Future: ArgoCD Integration

For GitOps-based deployment, ArgoCD can be configured to synchronize the `deskmatch` namespace from the repository:

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: deskmatch
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://github.com/your-org/DeskMatch
    targetRevision: main
    path: infrastructure/kubernetes/production
  destination:
    server: https://kubernetes.default.svc
    namespace: deskmatch
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
```

Benefits:
- Declarative state in Git is the single source of truth
- Automatic drift detection and reconciliation
- Visual diff of changes before sync
- Rollback via Git revert

## Pipeline Environment Variables

### Required Secrets

| Secret                 | Purpose                          |
|------------------------|----------------------------------|
| DOCKERHUB_USERNAME     | Docker Hub login                 |
| DOCKERHUB_TOKEN        | Docker Hub access token          |
| KUBE_CONFIG            | Kubernetes kubeconfig (base64)   |
| HELM_REPO_SECRET       | Helm repository credentials      |

### Environment-Specific Variables

| Variable               | Dev           | Staging         | Production      |
|------------------------|---------------|-----------------|-----------------|
| ASPNETCORE_ENVIRONMENT | Development   | Staging         | Production      |
| DOCKER_TAG             | latest        | latest          | ${GIT_TAG}      |
| DEPLOY_TIMEOUT         | 5m            | 10m             | 15m             |

## Running CI Locally

### .NET

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

### Frontend

```bash
cd apps/frontend-web
npm ci
npm run lint
npm run build
```

### Docker

```bash
docker build -t deskmatch/auth-service -f apps/auth-service/Dockerfile .
```

### Act (run GitHub Actions locally)

```bash
act push
act -j build-dotnet
```