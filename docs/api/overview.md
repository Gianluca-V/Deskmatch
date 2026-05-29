# API Documentation

## Authentication

DeskMatch uses **JWT Bearer Authentication** with symmetric HMAC SHA-256 signing.

### JWT Flow

```
1. POST /api/auth/register     → Create user account
2. POST /api/auth/login        → { accessToken, refreshToken }
3. Authorization: Bearer {accessToken} on all subsequent requests
4. POST /api/auth/refresh      → New { accessToken, refreshToken } (when access token expires)
5. POST /api/auth/revoke       → Invalidate refresh token (logout)
```

### Token Claims

```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "role": "User",
  "iat": 1704067200,
  "exp": 1704068100,
  "iss": "DeskMatch",
  "aud": "DeskMatch"
}
```

## Core Endpoints

### Auth Service (`/api/auth`)

| Method | Path                | Auth | Request Body                         | Response                          |
|--------|---------------------|------|--------------------------------------|-----------------------------------|
| POST   | `/register`         | No   | `{ email, password, firstName, lastName }` | `{ id, email, firstName, lastName, role }` |
| POST   | `/login`            | No   | `{ email, password }`               | `{ accessToken, refreshToken, expiresIn }` |
| POST   | `/refresh`          | No   | `{ refreshToken }`                   | `{ accessToken, refreshToken, expiresIn }` |
| POST   | `/revoke`           | Yes  | `{ refreshToken }`                   | `{ message }`                     |
| GET    | `/me`               | Yes  | —                                    | `{ id, email, firstName, lastName, role }` |
| GET    | `/users`            | Yes  | — (Admin only)                       | `[{ id, email, firstName, lastName, role, isActive }]` |

#### Register Request

```json
{
  "email": "john@example.com",
  "password": "SecureP@ss1",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Login Response

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
  "expiresIn": 900
}
```

### Core Service (`/api/offices`, `/api/companies`, `/api/reservations`, `/api/reviews`)

#### Offices

| Method | Path                     | Auth | Description                     |
|--------|--------------------------|------|---------------------------------|
| GET    | `/api/offices`           | No   | List offices (paginated)        |
| GET    | `/api/offices/{id}`      | No   | Get office by ID                |
| POST   | `/api/offices`           | Yes  | Create office (Manager/Admin)   |
| PUT    | `/api/offices/{id}`      | Yes  | Update office                   |
| DELETE | `/api/offices/{id}`      | Yes  | Soft-delete office (Admin)      |

#### List Offices (Query Parameters)

| Param       | Type    | Default | Description                     |
|-------------|---------|---------|---------------------------------|
| `page`      | int     | 1       | Page number                     |
| `pageSize`  | int     | 20      | Items per page (max 100)        |
| `city`      | string  | —       | Filter by city                  |
| `country`   | string  | —       | Filter by country               |
| `minPrice`  | decimal | —       | Minimum price per hour          |
| `maxPrice`  | decimal | —       | Maximum price per hour          |
| `minCapacity`| int    | —       | Minimum capacity                |
| `amenities` | string  | —       | Comma-separated (WiFi,Coffee)   |

#### Get Office Response

```json
{
  "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "name": "Downtown Hub",
  "description": "Modern workspace in the heart of the city...",
  "address": "123 Main Street",
  "city": "New York",
  "country": "United States",
  "latitude": 40.7128,
  "longitude": -74.006,
  "capacity": 50,
  "pricePerHour": 25.00,
  "pricePerDay": 150.00,
  "pricePerMonth": 2500.00,
  "amenities": ["WiFi", "Meeting Rooms", "Coffee", "Printing", "Parking"],
  "images": ["https://...", "https://..."],
  "rating": 4.5,
  "reviewCount": 12,
  "company": {
    "id": "a1b2c3d4-...",
    "name": "DeskMatch Co."
  }
}
```

#### Create Office Request

```json
{
  "companyId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "New Office",
  "description": "A great place to work",
  "address": "456 Work Street",
  "city": "New York",
  "country": "United States",
  "latitude": 40.7128,
  "longitude": -74.006,
  "capacity": 30,
  "pricePerHour": 20.00,
  "amenities": ["WiFi", "Coffee", "Parking"]
}
```

#### Companies

| Method | Path                     | Auth | Description                     |
|--------|--------------------------|------|---------------------------------|
| GET    | `/api/companies`         | No   | List all companies              |
| GET    | `/api/companies/{id}`    | No   | Get company by ID               |
| POST   | `/api/companies`         | Yes  | Create company (Admin)          |
| PUT    | `/api/companies/{id}`    | Yes  | Update company (Admin)          |

#### Reservations

| Method | Path                              | Auth | Description                        |
|--------|-----------------------------------|------|------------------------------------|
| GET    | `/api/offices/{id}/reservations`  | No   | List reservations for office       |
| GET    | `/api/reservations/{id}`          | Yes  | Get reservation by ID              |
| POST   | `/api/reservations`               | Yes  | Create reservation                 |
| PUT    | `/api/reservations/{id}`          | Yes  | Update reservation                 |
| DELETE | `/api/reservations/{id}`          | Yes  | Cancel reservation                 |

#### Create Reservation Request

```json
{
  "officeId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "startTime": "2024-06-15T09:00:00Z",
  "endTime": "2024-06-15T17:00:00Z",
  "notes": "Need a quiet desk near a window"
}
```

#### Reservation Response

```json
{
  "id": "r1r2r3r4-...",
  "officeId": "b2c3d4e5-...",
  "userId": "u1u2u3u4-...",
  "officeName": "Downtown Hub",
  "startTime": "2024-06-15T09:00:00Z",
  "endTime": "2024-06-15T17:00:00Z",
  "status": "Confirmed",
  "totalPrice": 200.00,
  "notes": "Need a quiet desk near a window",
  "createdAt": "2024-06-14T10:30:00Z"
}
```

#### Reviews

| Method | Path                            | Auth | Description                      |
|--------|--------------------------------|------|----------------------------------|
| GET    | `/api/offices/{id}/reviews`     | No   | List reviews for an office       |
| POST   | `/api/reviews`                  | Yes  | Create a review                  |
| PUT    | `/api/reviews/{id}`             | Yes  | Update own review                |
| DELETE | `/api/reviews/{id}`             | Yes  | Delete own review                |

#### Create Review Request

```json
{
  "officeId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "rating": 5,
  "comment": "Amazing workspace! Great coffee and fast WiFi."
}
```

### Search Service (`/api/search`)

| Method | Path                 | Auth | Description                                 |
|--------|----------------------|------|---------------------------------------------|
| GET    | `/api/search/offices` | No   | Full-text search with filters               |
| GET    | `/api/search/nearby`  | No   | Geo-search by coordinates + radius           |
| GET    | `/api/search/suggest` | No   | Autocomplete suggestions                     |

#### Search Query Parameters

| Param      | Type   | Description                                      |
|------------|--------|--------------------------------------------------|
| `q`        | string | Full-text search query                            |
| `city`     | string | Filter by city                                   |
| `country`  | string | Filter by country                                |
| `minPrice` | decimal| Minimum price per hour                            |
| `maxPrice` | decimal| Maximum price per hour                            |
| `amenities`| string | Comma-separated (WiFi,Coffee)                    |
| `minRating`| int    | Minimum rating (1-5)                              |
| `sortBy`   | string | `relevance`, `rating`, `price`, `distance`       |
| `page`     | int    | Page number                                       |
| `pageSize` | int    | Items per page                                    |

#### Nearby Query Parameters

| Param      | Type   | Description                                      |
|------------|--------|--------------------------------------------------|
| `lat`      | double | Latitude for geo-search center                   |
| `lon`      | double | Longitude for geo-search center                  |
| `radiusKm` | double | Search radius in kilometers (default: 10)         |

#### Search Response

```json
{
  "items": [
    {
      "id": "b2c3d4e5-...",
      "name": "Downtown Hub",
      "city": "New York",
      "country": "United States",
      "pricePerHour": 25.00,
      "capacity": 50,
      "rating": 4.5,
      "reviewCount": 12,
      "amenities": ["WiFi", "Meeting Rooms", "Coffee"],
      "distanceKm": 2.3,
      "score": 9.8
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

### Notification Service (`/api/notifications`)

| Method | Path                           | Auth | Description                         |
|--------|--------------------------------|------|-------------------------------------|
| GET    | `/api/notifications`           | Yes  | List user's notifications            |
| PUT    | `/api/notifications/{id}/read` | Yes  | Mark notification as read            |
| POST   | `/api/notifications/send`      | Yes  | Send ad-hoc email (Admin only)       |

#### Send Notification Request (Admin)

```json
{
  "userId": "u1u2u3u4-...",
  "type": "Custom",
  "title": "Maintenance Notice",
  "message": "The Downtown Hub will be closed for maintenance on July 1st."
}
```

## Error Format

All services return errors using **RFC 7807 ProblemDetails**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Email": ["Email is required.", "Email is not in a valid format."],
    "Password": ["Password must be at least 8 characters."]
  },
  "traceId": "00-abc123def456-789012-1"
}
```

### Common Status Codes

| Status | Title               | Typical Cause                          |
|--------|---------------------|----------------------------------------|
| 400    | Bad Request         | Validation errors                      |
| 401    | Unauthorized        | Missing or invalid JWT                 |
| 403    | Forbidden           | Insufficient role (e.g., User trying Admin action) |
| 404    | Not Found           | Resource ID doesn't exist              |
| 409    | Conflict            | Duplicate resource, overlapping reservation |
| 429    | Too Many Requests   | Rate limit exceeded                    |
| 500    | Internal Server Error | Unhandled exception                 |
| 503    | Service Unavailable | Database/Redis/OpenSearch down         |

## Swagger

Swagger UI is available at:
- Gateway (aggregated): `http://localhost:5000/swagger`
- Individual services: `http://localhost:5xxx/swagger`

## Rate Limiting

| Endpoint           | Limit            | Window    |
|--------------------|------------------|-----------|
| `/api/auth/login`  | 5 requests       | 1 minute  |
| `/api/auth/register`| 3 requests     | 1 minute  |
| All others         | 100 requests     | 1 minute  |

(Enforced at the API Gateway level.)

## Pagination

All list endpoints return paginated results with the format:

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```