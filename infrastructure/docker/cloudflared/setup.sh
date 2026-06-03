#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CREDENTIALS_DIR="$SCRIPT_DIR/../.cloudflared"
IMAGE="cloudflare/cloudflared:latest"
TUNNEL_NAME="deskmatch"
DOMAIN="deskmatch.vespelabs.com"

mkdir -p "$CREDENTIALS_DIR"

echo "==> 1. Autenticando con Cloudflare (abre el navegador)..."
docker run --rm -v "$CREDENTIALS_DIR:/home/nonroot/.cloudflared" "$IMAGE" tunnel login

echo ""
echo "==> 2. Creando tunnel '$TUNNEL_NAME'..."
docker run --rm -v "$CREDENTIALS_DIR:/home/nonroot/.cloudflared" "$IMAGE" tunnel create "$TUNNEL_NAME"

echo ""
echo "==> 3. Creando DNS CNAME para $DOMAIN..."
docker run --rm -v "$CREDENTIALS_DIR:/home/nonroot/.cloudflared" "$IMAGE" tunnel route dns "$TUNNEL_NAME" "$DOMAIN"

echo ""
echo "==> 4. Copiando credentials.json a $CREDENTIALS_DIR/credentials.json..."
# cloudflared guarda el JSON con un UUID como nombre; lo renombramos fijo
cp "$CREDENTIALS_DIR"/*.json "$CREDENTIALS_DIR/credentials.json" 2>/dev/null || true

echo ""
echo "=== LISTO ==="
echo "Ahora ejecuta desde infrastructure/docker/:"
echo "  docker compose up -d cloudflared"
