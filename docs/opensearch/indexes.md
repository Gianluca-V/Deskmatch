# OpenSearch Documentation

## Overview

OpenSearch 2.x is used for full-text search and geo-location queries on office spaces. The `search-service` performs all search operations against the `offices` index, while `core-service` writes to it when offices are created or updated.

## Office Index Mapping

```json
{
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 0,
    "analysis": {
      "analyzer": {
        "office_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "asciifolding"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name":         { "type": "text", "analyzer": "office_analyzer", "fields": { "raw": { "type": "keyword" } } },
      "description":  { "type": "text", "analyzer": "office_analyzer" },
      "city":         { "type": "keyword" },
      "country":      { "type": "keyword" },
      "address":      { "type": "text" },
      "capacity":     { "type": "integer" },
      "pricePerHour": { "type": "scaled_float", "scaling_factor": 100 },
      "amenities":    { "type": "keyword" },
      "location":     { "type": "geo_point" },
      "rating":       { "type": "float" },
      "reviewCount":  { "type": "integer" },
      "createdAt":    { "type": "date" },
      "updatedAt":    { "type": "date" }
    }
  }
}
```

### Field Explanations

| Field         | Type           | Purpose                                                |
|---------------|----------------|--------------------------------------------------------|
| `name`        | text + keyword | Full-text search + exact match / aggregation           |
| `description` | text           | Full-text search with asciifolding (accent-insensitive)|
| `city`        | keyword        | Exact match filter, aggregation (facet)                |
| `country`     | keyword        | Exact match filter, aggregation                        |
| `capacity`    | integer        | Range filter (e.g., capacity >= 5)                     |
| `pricePerHour`| scaled_float   | Range filter, sorting (stored as integer cents)         |
| `amenities`   | keyword        | Multi-value filter (e.g., amenities = ["WiFi", "Coffee"]) |
| `location`    | geo_point      | Geo-distance and geo-bounding-box queries              |
| `rating`      | float          | Sort by rating, range filter                           |
| `reviewCount` | integer        | Sort by popularity                                     |

### Custom Analyzer

`office_analyzer` uses:
- `standard` tokenizer (splits on whitespace/punctuation)
- `lowercase` filter (case-insensitive search)
- `asciifolding` filter (handles accented characters like "café" → "cafe")

## Search Queries

### Full-Text Search

```
GET /offices/_search
{
  "query": {
    "bool": {
      "must": [
        { "multi_match": { "query": "modern workspace", "fields": ["name^3", "description^2", "address"] } }
      ],
      "filter": [
        { "term": { "city": "New York" } },
        { "terms": { "amenities": ["WiFi", "Coffee"] } },
        { "range": { "pricePerHour": { "gte": 10, "lte": 50 } } },
        { "range": { "capacity": { "gte": 5 } } }
      ]
    }
  },
  "sort": [
    { "_score": "desc" },
    { "rating": "desc" }
  ]
}
```

### Geo-Search: Nearby Offices

```
GET /offices/_search
{
  "query": {
    "bool": {
      "must": { "match_all": {} },
      "filter": {
        "geo_distance": {
          "distance": "10km",
          "location": { "lat": 40.7128, "lon": -74.0060 }
        }
      }
    }
  },
  "sort": [
    {
      "_geo_distance": {
        "location": { "lat": 40.7128, "lon": -74.0060 },
        "order": "asc",
        "unit": "km"
      }
    }
  ]
}
```

### Geo-Search: Bounding Box

```
GET /offices/_search
{
  "query": {
    "bool": {
      "filter": {
        "geo_bounding_box": {
          "location": {
            "top_left":     { "lat": 40.8, "lon": -74.1 },
            "bottom_right": { "lat": 40.6, "lon": -73.9 }
          }
        }
      }
    }
  }
}
```

### Autocomplete / Suggestions

```
GET /offices/_search
{
  "suggest": {
    "office-suggest": {
      "prefix": "downt",
      "completion": { "field": "name.suggest" }
    }
  }
}
```

(Requires a `completion` sub-field on `name` if implemented.)

## Indexing Flow

### On Office Create (core-service)

```
1. Core Service receives POST /api/offices
2. Validates + persists to PostgreSQL
3. Indexes to OpenSearch:
   POST /offices/_doc/{id}
   {
     "name": "...",
     "description": "...",
     "city": "...",
     "location": { "lat": ..., "lon": ... },
     ...
   }
4. Return to client
```

### On Office Update (core-service)

```
1. Core Service receives PUT /api/offices/{id}
2. Validates + updates PostgreSQL
3. Updates OpenSearch:
   PUT /offices/_doc/{id}  (full replace)
```

### On Office Delete (core-service)

```
1. Soft-delete in PostgreSQL (IsActive = false)
2. Remove from OpenSearch:
   DELETE /offices/_doc/{id}
```

## Reindexing Strategy

### When to Reindex

- Index mapping changes (add/remove fields, change analyzers)
- Data corruption or sync issues
- Performance optimization (change shard count)

### Procedure

1. Create a new index with the updated mapping (`offices_v2`).
2. Reindex data from the old index:

```
POST /_reindex
{
  "source": { "index": "offices" },
  "dest":   { "index": "offices_v2" }
}
```

3. Update aliases:

```
POST /_aliases
{
  "actions": [
    { "remove": { "index": "offices", "alias": "offices_read" } },
    { "add":    { "index": "offices_v2", "alias": "offices_read" } }
  ]
}
```

4. Update service configuration to point to the new alias/index.
5. Delete the old index after verification.

### Full Reindex from PostgreSQL

If OpenSearch data is completely lost or corrupted:

```bash
# Trigger from core-service admin endpoint (future)
POST /api/admin/reindex-office
```

This endpoint iterates all active offices in PostgreSQL and bulk-indexes them to OpenSearch.

## Index Lifecycle

| Phase        | Action                                  |
|--------------|-----------------------------------------|
| Hot          | Active index, single shard, 0 replicas (dev) |
| Warm         | N/A (small dataset, single node)        |
| Delete       | N/A (offices are not time-series data)  |

For production, consider:
- **ISMPolicies** to roll over indices by size or age
- Adding **replicas** for high availability (`number_of_replicas: 1`)

## Monitoring

### Cluster Health

```bash
GET /_cluster/health
```

### Index Stats

```bash
GET /offices/_stats
GET /_cat/indices?v
```

### Prometheus Metrics (via `/_prometheus/metrics`)

- `opensearch_cluster_health_status`
- `opensearch_indices_docs_primary`
- `opensearch_jvm_memory_used_bytes`
- `opensearch_indices_search_query_time_seconds`

## Seed Data

On first start, `opensearch-init` container indexes 3 seed offices matching the PostgreSQL seed data:

```json
{
  "index": { "_index": "offices", "_id": "b2c3d4e5-f6a7-8901-bcde-f12345678901" }
}
{ "name": "Downtown Hub", "city": "New York", "country": "United States", "capacity": 50, "pricePerHour": 25.00, "location": { "lat": 40.7128, "lon": -74.0060 }, "amenities": ["WiFi", "Meeting Rooms", "Coffee", "Printing", "Parking"] }
...
```

## Local Development

```bash
# Access OpenSearch API
curl -u admin:DeskMatch123! http://localhost:9200

# List indices
curl -u admin:DeskMatch123! http://localhost:9200/_cat/indices?v

# Search offices
curl -u admin:DeskMatch123! -X GET "http://localhost:9200/offices/_search?pretty" -H "Content-Type: application/json" -d '{"query": {"match_all": {}}}'
```