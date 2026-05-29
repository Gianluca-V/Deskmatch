# PostgreSQL Documentation

## Database Architecture

DeskMatch uses a **database-per-service** pattern with three logical databases inside a single PostgreSQL instance:

| Database                  | Service              | Purpose                               |
|---------------------------|----------------------|---------------------------------------|
| `deskmatch_auth`          | auth-service         | Users, roles, refresh tokens          |
| `deskmatch_core`          | core-service         | Companies, offices, reservations, reviews |
| `deskmatch_notification`  | notification-service | Email templates, notifications        |

## Schema: `deskmatch_auth`

```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TYPE userrole AS ENUM ('Admin', 'Manager', 'User');

CREATE TABLE "Users" (
    "Id"           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Email"        VARCHAR(256) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "FirstName"    VARCHAR(128) NOT NULL,
    "LastName"     VARCHAR(128) NOT NULL,
    "Role"         userrole NOT NULL DEFAULT 'User',
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE,
    "RefreshToken" TEXT,
    "RefreshTokenExpiryTime" TIMESTAMPTZ,
    "CreatedAt"    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"    TIMESTAMPTZ
);

CREATE TABLE "RefreshTokens" (
    "Id"        UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId"    UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Token"     TEXT NOT NULL,
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "RevokedAt" TIMESTAMPTZ
);
```

### Seed Data

| Email              | Role   |
|--------------------|--------|
| admin@deskmatch.com | Admin  |

## Schema: `deskmatch_core`

```sql
CREATE TABLE "Companies" (
    "Id"          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name"        VARCHAR(256) NOT NULL,
    "Description" TEXT,
    "LogoUrl"     VARCHAR(512),
    "WebsiteUrl"  VARCHAR(512),
    "OwnerId"     UUID,
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"   TIMESTAMPTZ
);

CREATE TABLE "Offices" (
    "Id"           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "CompanyId"    UUID NOT NULL REFERENCES "Companies"("Id") ON DELETE CASCADE,
    "Name"         VARCHAR(256) NOT NULL,
    "Description"  TEXT,
    "Address"      VARCHAR(512),
    "City"         VARCHAR(128),
    "Country"      VARCHAR(128),
    "Latitude"     DOUBLE PRECISION,
    "Longitude"    DOUBLE PRECISION,
    "Capacity"     INTEGER NOT NULL DEFAULT 1,
    "PricePerHour" NUMERIC(10,2) NOT NULL DEFAULT 0,
    "PricePerDay"  NUMERIC(10,2),
    "PricePerMonth" NUMERIC(10,2),
    "Amenities"    JSONB,
    "Images"       JSONB,
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt"    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"    TIMESTAMPTZ
);

CREATE TABLE "Reservations" (
    "Id"          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OfficeId"    UUID NOT NULL REFERENCES "Offices"("Id") ON DELETE CASCADE,
    "UserId"      UUID NOT NULL,
    "StartTime"   TIMESTAMPTZ NOT NULL,
    "EndTime"     TIMESTAMPTZ NOT NULL,
    "Status"      VARCHAR(32) NOT NULL DEFAULT 'Pending',
    "TotalPrice"  NUMERIC(10,2),
    "Notes"       TEXT,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"   TIMESTAMPTZ
);

CREATE TABLE "Reviews" (
    "Id"        UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OfficeId"  UUID NOT NULL REFERENCES "Offices"("Id") ON DELETE CASCADE,
    "UserId"    UUID NOT NULL,
    "Rating"    INTEGER NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    "Comment"   TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);
```

### Seed Data: Offices

| Name           | City          | Country        | Capacity | Price/Hour |
|----------------|---------------|----------------|----------|------------|
| Downtown Hub   | New York      | United States  | 50       | $25.00     |
| Tech Campus    | San Francisco | United States  | 120      | $35.00     |
| Creative Loft  | London        | United Kingdom | 30       | $20.00     |

## Schema: `deskmatch_notification`

```sql
CREATE TABLE "EmailTemplates" (
    "Id"        UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name"      VARCHAR(128) NOT NULL UNIQUE,
    "Subject"   VARCHAR(256) NOT NULL,
    "Body"      TEXT NOT NULL,
    "IsHtml"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE TABLE "Notifications" (
    "Id"        UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId"    UUID NOT NULL,
    "Type"      VARCHAR(64) NOT NULL,
    "Title"     VARCHAR(256) NOT NULL,
    "Message"   TEXT NOT NULL,
    "IsRead"    BOOLEAN NOT NULL DEFAULT FALSE,
    "Metadata"  JSONB,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

## Migration Strategy

### Development

- EF Core migrations are created per-service and stored in each service's source tree.
- On startup in Development mode, services call `DbContext.Database.Migrate()` to auto-apply pending migrations.

### Production

- Apply migrations via a dedicated init container or CI/CD job **before** rolling out new service versions.
- Generate idempotent SQL scripts:

```bash
dotnet ef migrations script --idempotent -o migrate.sql -p apps/auth-service
```

### Rollback

- Always test migrations in a staging environment first.
- For breaking schema changes, use an **expand-contract** pattern:
  1. Add new column/schema (backward compatible).
  2. Deploy updated service that writes to both old and new columns.
  3. Backfill data.
  4. Deploy service that reads only from new columns.
  5. Drop old columns.

## Initialization Scripts

On first Docker Compose startup, PostgreSQL runs scripts in order:

| Order | Script                              | Action                               |
|-------|-------------------------------------|--------------------------------------|
| 1     | `init-multiple-databases.sh`        | Creates `deskmatch_auth`, `deskmatch_core`, `deskmatch_notification` |
| 2     | `init-auth.sql`                     | Creates auth tables + seed admin user |
| 3     | `init-core.sql`                     | Creates core tables + seed offices   |
| 4     | `init-notification.sql`             | Creates notification tables + seed email templates |

Each script is idempotent (`CREATE IF NOT EXISTS`, `ON CONFLICT DO NOTHING`).

## Backup and Restore

### Backup all databases

```bash
docker exec deskmatch-postgres pg_dumpall -U deskmatch > backup.sql
```

### Backup a single database

```bash
docker exec deskmatch-postgres pg_dump -U deskmatch deskmatch_core > core_backup.sql
```

### Restore

```bash
docker exec -i deskmatch-postgres psql -U deskmatch < backup.sql
```

### Automated Backups (recommended for production)

Use a sidecar container with `pg_dump` scheduled via cron, or use a managed PostgreSQL service (AWS RDS, Azure PostgreSQL, GCP Cloud SQL) with automated snapshot backups.

### Point-in-Time Recovery

Enable WAL archiving for PITR:

```ini
# postgresql.conf
wal_level = replica
archive_mode = on
archive_command = '...'
```

## Connection Strings

### Docker Compose

```
Host=postgres;Port=5432;Database=deskmatch_core;Username=deskmatch;Password=deskmatch123!
```

### Kubernetes

```
Host=postgres.deskmatch.svc.cluster.local;Port=5432;Database=deskmatch_core;Username=deskmatch;Password={from-secret}
```