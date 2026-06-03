#!/bin/bash
set -e

OPENSEARCH_URL="${OPENSEARCH_URL:-http://localhost:9200}"
INDEX_NAME="offices"
MAPPING_FILE="/usr/share/opensearch/init/offices-mapping.json"
SEED_FILE="/usr/share/opensearch/init/seed-data.json"

echo "Waiting for OpenSearch to be ready..."
until curl -s -k -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" "$OPENSEARCH_URL/_cluster/health" > /dev/null 2>&1; do
    echo "  Still waiting for OpenSearch..."
    sleep 5
done
echo "OpenSearch is ready."

echo "Creating index '$INDEX_NAME'..."
curl -s -k -X PUT -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
    -H "Content-Type: application/json" \
    -d "@${MAPPING_FILE}" \
    "$OPENSEARCH_URL/$INDEX_NAME" || true
echo ""

echo "Indexing seed data..."
if [ -f "$SEED_FILE" ]; then
    curl -s -k -X POST -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
        -H "Content-Type: application/x-ndjson" \
        --data-binary "@${SEED_FILE}" \
        "$OPENSEARCH_URL/_bulk" > /dev/null
    echo "Seed data indexed successfully."
else
    echo "No seed data file found at $SEED_FILE, skipping."
fi

echo "OpenSearch initialization complete."