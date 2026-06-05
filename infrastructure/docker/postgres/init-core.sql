CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE SCHEMA IF NOT EXISTS core;

CREATE TABLE IF NOT EXISTS core."Companies" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT,
    "LogoUrl" VARCHAR(512),
    "WebsiteUrl" VARCHAR(512),
    "OwnerId" UUID,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS core."Workspaces" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "CompanyId" UUID NOT NULL REFERENCES core."Companies"("Id") ON DELETE CASCADE,
    "Name" VARCHAR(256) NOT NULL,
    "Description" TEXT,
    "Address" VARCHAR(512),
    "City" VARCHAR(128),
    "Country" VARCHAR(128),
    "Latitude" DOUBLE PRECISION,
    "Longitude" DOUBLE PRECISION,
    "Capacity" INTEGER NOT NULL DEFAULT 1,
    "PricePerHour" NUMERIC(10,2) NOT NULL DEFAULT 0,
    "PricePerDay" NUMERIC(10,2),
    "PricePerMonth" NUMERIC(10,2),
    "Amenities" TEXT[],
    "Images" TEXT[],
    "Rating" NUMERIC(3,2),
    "ReviewCount" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS core."Reservations" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "WorkspaceId" UUID NOT NULL REFERENCES core."Workspaces"("Id") ON DELETE CASCADE,
    "UserId" UUID NOT NULL,
    "StartTime" TIMESTAMPTZ NOT NULL,
    "EndTime" TIMESTAMPTZ NOT NULL,
    "Status" VARCHAR(32) NOT NULL DEFAULT 'Pending',
    "TotalPrice" NUMERIC(10,2),
    "Notes" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS core."Reviews" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "WorkspaceId" UUID NOT NULL REFERENCES core."Workspaces"("Id") ON DELETE CASCADE,
    "UserId" UUID NOT NULL,
    "Rating" INTEGER NOT NULL CHECK ("Rating" >= 1 AND "Rating" <= 5),
    "Comment" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

INSERT INTO core."Companies" ("Id", "Name", "Description", "WebsiteUrl")
VALUES (
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'DeskMatch Co.',
    'Official DeskMatch company for workspace management.',
    'https://deskmatch.com'
)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO core."Workspaces" ("Id", "CompanyId", "Name", "Description", "Address", "City", "Country", "Latitude", "Longitude", "Capacity", "PricePerHour", "Amenities")
VALUES
(
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'Downtown Hub',
    'Modern workspace in the heart of the city with high-speed internet and meeting rooms.',
    '123 Main Street',
    'New York',
    'United States',
    40.7128,
    -74.0060,
    50,
    25.00,
    ARRAY['WiFi', 'Meeting Rooms', 'Coffee', 'Printing', 'Parking']
),
(
    'c3d4e5f6-a7b8-9012-cdef-123456789012',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'Tech Campus',
    'Spacious campus designed for tech teams with dedicated desks and event space.',
    '456 Innovation Drive',
    'San Francisco',
    'United States',
    37.7749,
    -122.4194,
    120,
    35.00,
    ARRAY['WiFi', 'Meeting Rooms', 'Coffee', 'Event Space', 'Gym', 'Cafeteria']
),
(
    'd4e5f6a7-b8c9-0123-defa-234567890123',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'Creative Loft',
    'Inspiring loft space for freelancers and small teams with natural lighting.',
    '789 Arts Avenue',
    'London',
    'United Kingdom',
    51.5074,
    -0.1278,
    30,
    20.00,
    ARRAY['WiFi', 'Coffee', 'Lounge', 'Rooftop', 'Bike Storage']
)
ON CONFLICT ("Id") DO NOTHING;