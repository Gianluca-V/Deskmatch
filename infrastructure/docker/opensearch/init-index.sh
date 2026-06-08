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

echo "Checking if index '$INDEX_NAME' exists..."
INDEX_EXISTS=$(curl -s -k -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
    -o /dev/null -w "%{http_code}" \
    "$OPENSEARCH_URL/$INDEX_NAME")

if [ "$INDEX_EXISTS" = "200" ]; then
    echo "Index '$INDEX_NAME' already exists. Updating mapping via reindex..."

    NEW_INDEX="${INDEX_NAME}_v2"
    echo "Creating new index '$NEW_INDEX' with updated mapping..."
    curl -s -k -X PUT -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
        -H "Content-Type: application/json" \
        -d "@${MAPPING_FILE}" \
        "$OPENSEARCH_URL/$NEW_INDEX" || true

    echo "Reindexing data from '$INDEX_NAME' to '$NEW_INDEX'..."
    curl -s -k -X POST -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
        -H "Content-Type: application/json" \
        -d "{\"source\": {\"index\": \"$INDEX_NAME\"}, \"dest\": {\"index\": \"$NEW_INDEX\"}}" \
        "$OPENSEARCH_URL/_reindex" > /dev/null || true

    echo "Deleting old index '$INDEX_NAME'..."
    curl -s -k -X DELETE -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
        "$OPENSEARCH_URL/$INDEX_NAME" || true

    echo "Creating alias '$INDEX_NAME' pointing to '$NEW_INDEX'..."
    curl -s -k -X POST -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
        -H "Content-Type: application/json" \
        -d "{\"actions\": [{\"add\": {\"index\": \"$NEW_INDEX\", \"alias\": \"$INDEX_NAME\"}}]}" \
        "$OPENSEARCH_URL/_aliases" || true

    echo "Mapping migration complete."
else
    echo "Creating index '$INDEX_NAME'..."
    curl -s -k -X PUT -u "admin:${OPENSEARCH_INITIAL_ADMIN_PASSWORD:-DeskMatch123!}" \
        -H "Content-Type: application/json" \
        -d "@${MAPPING_FILE}" \
        "$OPENSEARCH_URL/$INDEX_NAME" || true
    echo ""
fi

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
