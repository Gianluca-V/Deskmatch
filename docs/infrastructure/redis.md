# Redis Documentation

## Cache Configuration

Redis 7 is used as a distributed cache layer, primarily by `core-service` to cache read-heavy query results.

### Connection

```bash
# Docker Compose
redis:6379,password=,ssl=false,abortConnect=false

# Kubernetes
redis.deskmatch.svc.cluster.local:6379,password=,ssl=false,abortConnect=false
```

### Persistence

Redis is configured with **AOF (Append-Only File)** persistence:

```yaml
command: redis-server --appendonly yes
```

This ensures cached data survives container restarts.

## Data Types Used

| Data Type   | Key Pattern              | Use Case                                     |
|-------------|--------------------------|----------------------------------------------|
| String      | `offices:{id}`           | Single office details (serialized JSON)      |
| String      | `offices:list:{hash}`    | Cached office list result (serialized JSON)  |
| String      | `companies:{id}`         | Single company details                       |
| String      | `companies:list:{hash}`  | Cached company list result                   |

Future possibilities:

| Data Type   | Use Case                                    |
|-------------|---------------------------------------------|
| Sorted Set  | Trending offices, leaderboard by rating     |
| Hash        | Rate limiting (field = user ID, value = count) |
| Set         | User session tracking                       |
| String      | Distributed locks for reservation creation  |

## Cache Invalidation Strategy

### Write-Through

On a write operation (create/update/delete), the cache entry is **immediately invalidated**:

```
1. Client sends PUT /api/offices/{id}
2. Handler updates PostgreSQL
3. Handler calls RedisCache.Remove($"offices:{id}")
4. Handler calls RedisCache.RemoveByPattern("offices:list:*")  // invalidate all list caches
```

### TTL-Based Expiration

Every cache entry is written with a **TTL (Time-To-Live)**. If no write invalidates it, the entry expires naturally.

### No Cache Stampede

When a cache entry expires, the first request rebuilds the cache; subsequent concurrent requests either:
- Wait for the first request (using an in-memory lock), or
- Serve slightly stale data (depending on the feature flag).

## TTL Policies

| Key Pattern             | TTL     | Rationale                                      |
|-------------------------|---------|------------------------------------------------|
| `offices:{id}`          | 10 min  | Office details change infrequently              |
| `offices:list:{hash}`   | 5 min   | List data is more dynamic (price changes, etc.) |
| `companies:{id}`        | 30 min  | Company profiles rarely change                  |
| `companies:list:{hash}` | 15 min  | Moderate change frequency                       |

## Redis SDK (DeskMatch.SDK.Redis)

The shared SDK provides:

- `IRedisCacheService` — abstraction over `IDistributedCache`
- `GetOrCreateAsync<T>(key, factory, ttl)` — cache-aside pattern with automatic population
- `RemoveAsync(key)` — single key invalidation
- `RemoveByPatternAsync(pattern)` — bulk invalidation via `KEYS` + `DEL`

### Usage Example

```csharp
public async Task<OfficeDto?> GetOfficeById(Guid id, CancellationToken ct)
{
    var key = $"offices:{id}";
    return await _cache.GetOrCreateAsync(key, async () =>
    {
        var office = await _dbContext.Offices
            .Where(o => o.Id == id)
            .ProjectTo<OfficeDto>()
            .FirstOrDefaultAsync(ct);
        return office;
    }, TimeSpan.FromMinutes(10));
}
```

## Monitoring Redis

### Prometheus Metrics

The Redis exporter exposes the following:

- `redis_memory_used_bytes`
- `redis_connected_clients`
- `redis_commands_processed_total`
- `redis_keyspace_hits_total`
- `redis_keyspace_misses_total`
- `redis_evicted_keys_total`

### Key Metrics to Watch

| Metric                  | Alert Condition              |
|-------------------------|------------------------------|
| Hit ratio < 80%         | Cache is not effective; adjust TTL or key strategy |
| Evicted keys increasing | Memory pressure; increase `maxmemory` or review TTL |
| Connected clients spike | Connection leak in a service |

### Inspect Locally

```bash
docker exec -it deskmatch-redis redis-cli
INFO stats
INFO keyspace
```

## Useful Commands

```bash
# Connect to Redis CLI
docker exec -it deskmatch-redis redis-cli

# List all keys
KEYS *

# Check TTL on a key
TTL offices:list:abc123

# Get a cached value
GET offices:b2c3d4e5-f6a7-8901-bcde-f12345678901

# Manual cache clear
FLUSHALL

# Monitor real-time commands
MONITOR
```