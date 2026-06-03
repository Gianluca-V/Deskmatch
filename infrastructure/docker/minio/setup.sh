#!/bin/bash
# ──────────────────────────────────────────────────────────
# MinIO bucket setup
# ──────────────────────────────────────────────────────────
# Uso manual (si no está usando docker-compose):
#   chmod +x setup.sh && ./setup.sh
#
# docker-compose ejecuta esto automáticamente via minio-setup
# ──────────────────────────────────────────────────────────

MC_ALIAS=${MC_ALIAS:-myminio}
MINIO_ENDPOINT=${MINIO_ENDPOINT:-http://localhost:9000}
MINIO_ROOT_USER=${MINIO_ROOT_USER:-minioadmin}
MINIO_ROOT_PASSWORD=${MINIO_ROOT_PASSWORD:-minioadmin}
MINIO_BUCKET=${MINIO_BUCKET:-deskmatch}

echo "==> Conectando a MinIO en $MINIO_ENDPOINT ..."
mc alias set $MC_ALIAS $MINIO_ENDPOINT $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD

echo "==> Creando bucket '$MINIO_BUCKET' (si no existe) ..."
mc mb $MC_ALIAS/$MINIO_BUCKET --ignore-existing

echo "==> Habilitando lectura anónima (descargas públicas) ..."
mc anonymous set download $MC_ALIAS/$MINIO_BUCKET

echo "==> Listo. Bucket '$MINIO_BUCKET' configurado."
echo "    Consola:   http://localhost:9001"
echo "    API S3:    http://localhost:9000"
echo "    Bucket:    $MINIO_BUCKET"
