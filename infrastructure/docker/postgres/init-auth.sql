CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'userrole') THEN
        CREATE TYPE userrole AS ENUM ('Admin', 'Manager', 'User');
    END IF;
END;
$$;

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Email" VARCHAR(256) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "FirstName" VARCHAR(128) NOT NULL,
    "LastName" VARCHAR(128) NOT NULL,
    "Role" userrole NOT NULL DEFAULT 'User',
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "RefreshToken" TEXT,
    "RefreshTokenExpiryTime" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Token" TEXT NOT NULL,
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "RevokedAt" TIMESTAMPTZ
);

INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role")
VALUES (
    'admin@deskmatch.com',
    '$2a$11$K7Q5pY8zG3xLmN4vR6sT9uW1yB2cD3eF4gH5iJ6kL7mN8oP9qR0s',
    'Admin',
    'DeskMatch',
    'Admin'
)
ON CONFLICT ("Email") DO NOTHING;