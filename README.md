# Office Days Tracker

Office Days Tracker is a small, self-hosted application for recording office attendance and checking compliance with a hybrid-working rule: **no more than 50% of eligible working days may be worked from home**.

For each calendar month, the application subtracts weekends, centrally configured bank holidays, and the user's vacation dates. It then calculates:

```text
maximum WFH days       = floor(eligible working days × 0.5)
required office days   = eligible working days - maximum WFH days
remaining office days  = max(0, required office days - eligible office attendance)
```

With 21 eligible days, this means at most 10 WFH days and therefore at least 11 office days.

## Architecture

The repository intentionally has a small structure:

```text
src/OfficeDays                    ASP.NET Core application, API, UI, domain logic, and EF Core
src/OfficeDays/Features           vertical feature slices with local contracts, validation, and endpoint mappings
tests/OfficeDays.UnitTests        isolated attendance-calculation tests
tests/OfficeDays.IntegrationTests real HTTP pipeline tests with an isolated SQLite database
```

The application uses .NET 10, ASP.NET Core minimal APIs, EF Core, and SQLite. The UI is one server-hosted page built with plain JavaScript and CSS. `AttendanceCalculator` is independent of HTTP and persistence, and accepts a general date range so future period types do not require rewriting the rule.

Stored dates such as attendance, vacation, and holidays use `DateOnly`. Audit timestamps use UTC. Calculated values are never persisted.

## Requirements

- .NET 10 SDK for local development
- Docker with Compose for container deployment
- An IANA timezone name for each user, such as `Europe/Bucharest`

## Run locally

The first start requires bootstrap administrator environment variables. They are intentionally absent from `appsettings.json`. Passwords must contain at least 12 characters.

```bash
BootstrapAdmin__Password='use-a-long-unique-password' \
BootstrapAdmin__TimeZoneId='Europe/Bucharest' \
Security__RequireHttpsForBearerTokens=false \
dotnet run --project src/OfficeDays
```

Open the URL printed by ASP.NET Core and sign in as `Admin`. The HTTPS bearer-token check is disabled above only for local development. EF Core migrations run automatically on startup and create `officedays.db` in the working directory.

If the database already contains any user, bootstrap configuration is ignored. It never resets or modifies the existing `Admin` account. If the database is empty and either required setting is missing, startup fails with a clear error.

Cookie session lifetime is configured in `src/OfficeDays/appsettings.json` using a standard .NET `TimeSpan` value:

```json
{
  "Authentication": {
    "CookieExpireTimeSpan": "12:00:00"
  }
}
```

This setting must be a positive duration. The authentication cookie is persistent across browser restarts and uses sliding expiration.

## Run with Docker Compose

Copy the example environment file and replace the password:

```bash
cp .env.example .env
docker compose up --build -d
```

The service listens on `http://localhost:8080`. SQLite data and ASP.NET Core data-protection keys are persisted in the `office-days-data` named volume at `/app/data`. Rebuilding or replacing the container does not delete them.

The container runs as the image's non-root `app` user. Database migrations and initial Admin creation happen automatically.

### HTTPS deployment

Put the application behind an HTTPS reverse proxy before exposing it outside a trusted network. API bearer tokens are credentials and must never be transmitted over public plain HTTP.

By default, the application rejects any bearer-authenticated request for which ASP.NET Core does not see HTTPS. A TLS-terminating proxy must forward the original scheme (normally `X-Forwarded-Proto: https`) from a trusted proxy. For an explicitly trusted internal-only HTTP network, the check can be disabled with:

```text
Security__RequireHttpsForBearerTokens=false
```

Do not disable this for an internet-facing deployment. The browser session and antiforgery cookies are `HttpOnly`/`SameSite=Strict`; they automatically receive the `Secure` flag when the request is HTTPS.

## Create a normal user

`POST /api/users` is the self-registration endpoint. It is intentionally anonymous and always creates a normal user. Its DTO has no `isAdmin` field; extra JSON such as `"isAdmin": true` cannot grant administration.

```bash
curl -X POST https://office.example.com/api/users \
  -H 'Content-Type: application/json' \
  -d '{
    "username": "andrew",
    "password": "a-long-unique-password",
    "timeZoneId": "Europe/Bucharest"
  }'
```

Usernames are unique without regard to case. The per-user timezone determines the calendar date recorded by `POST /api/attendance`, regardless of the container's timezone.

## Web UI

After login, everything is on one dashboard:

- month/year navigation and compliance status
- attendance history with a minimal past-day add/remove fallback
- vacation range add/remove
- API token creation and revocation
- an admin-only bank holiday editor

Manual attendance uses idempotent `PUT /api/attendance/{yyyy-MM-dd}` and does not allow future dates. Shortcut attendance uses the payload-free `POST /api/attendance`.

## API tokens

On the dashboard, enter a friendly name under **API tokens** and choose **Create token**. Copy the resulting token immediately: it is displayed once and cannot be recovered. The database stores only a SHA-256 hash of a cryptographically random 256-bit token.

The token list shows creation, last-use, and revocation information. **Revoke** retains the audit record while immediately preventing further authentication. Users can only list, create, or revoke their own tokens.

Relevant endpoints are:

```text
GET    /api/tokens
POST   /api/tokens              { "name": "Andrew's iPhone" }
DELETE /api/tokens/{id}         revokes; does not physically delete
```

Browser mutations use ASP.NET Core antiforgery protection. Bearer clients do not need an antiforgery token.

## Bank holidays

Sign in as `Admin`, choose a year, edit the bank holiday list, and save. Normal users may view holidays but cannot change them.

The API has full replacement semantics:

```http
GET /api/bank-holidays/2026
PUT /api/bank-holidays/2026
```

Example request using an Admin-owned API token:

```bash
curl -X PUT https://office.example.com/api/bank-holidays/2026 \
  -H 'Authorization: Bearer ADMIN_API_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '[
    { "date": "2026-01-01", "name": "New Year’s Day" },
    { "date": "2026-12-25", "name": "Christmas" }
  ]'
```

After the request, those are exactly the holidays stored for 2026. Omitted dates are removed, dates must belong to the URL year, and duplicate dates are rejected.

## Vacation

Vacation ranges can be maintained on the dashboard or through:

```text
GET    /api/vacations
POST   /api/vacations           { "from": "2026-09-21", "to": "2026-09-25" }
DELETE /api/vacations/{id}
```

Users can access only their own ranges. Overlapping ranges, weekends, and bank-holiday overlap are deduplicated by the calculation, so eligible days are never subtracted twice.

## iOS Shortcut setup

1. Create a token named after the device and copy it.
2. In Shortcuts, create a personal automation triggered by **Arrive** and select the office location.
3. Add **Get Contents of URL**.
4. Set the URL to `https://office.example.com/api/attendance`.
5. Set the method to `POST`.
6. Add the header `Authorization` with value `Bearer YOUR_TOKEN`.
7. Do not add a request body, date, username, or user ID.

Equivalent request:

```bash
curl -X POST https://office.example.com/api/attendance \
  -H 'Authorization: Bearer odt_your_token_here'
```

The token identifies the user. The server converts the current instant to that user's configured timezone and records the resulting local calendar date. `(UserId, Date)` has a unique database index, so repeated automation triggers still produce one record.

## Status and attendance API

```text
POST   /api/attendance                     record today in the user's timezone
PUT    /api/attendance/{yyyy-MM-dd}        manually record today or a past day
GET    /api/attendance?year=2026&month=9
DELETE /api/attendance/{yyyy-MM-dd}
GET    /api/status                         current month in the user's timezone
GET    /api/status?year=2026&month=9       selected month
```

Status includes eligible working days, maximum WFH days, required office days, counted eligible office days, remaining days, and progress percentage.

## Authentication and security model

- Web access uses ASP.NET Core encrypted cookie authentication, not a hand-built session mechanism.
- Passwords are hashed and verified by ASP.NET Core's `PasswordHasher<TUser>`.
- `IsAdmin` is a single boolean and claim; there is no generic role system.
- Long-lived API tokens are random opaque secrets, not JWTs. Only their hashes are stored.
- Revocation is checked on every bearer authentication and `LastUsedAt` is updated after successful use.
- User-owned queries always scope records to the authenticated user ID.
- Cookie-authenticated modifying operations require ASP.NET Core antiforgery tokens.
- Production exception handling returns Problem Details without stack traces.
- Application events use structured ASP.NET Core logging. Passwords and raw API tokens are never logged.

## Tests

Run everything with:

```bash
dotnet test OfficeDays.sln
```

The unit suite covers odd/even rule behavior, weekdays and weekends, holidays, all vacation overlap cases, remaining-day clamping, ineligible attendance, leap years, and period boundaries.

The integration suite uses `WebApplicationFactory`, the real middleware/endpoints, and a fresh migrated SQLite file per test. It covers registration/login, admin authorization and holiday replacement, token creation/use/revocation and ownership isolation, idempotent bearer attendance and removal, vacation ownership/lifecycle, and persisted status calculations.

## Database migrations

Migrations are committed under `src/OfficeDays/Data/Migrations` and applied at startup. To create a migration after changing the model:

```bash
dotnet ef migrations add NameOfChange \
  --project src/OfficeDays \
  --startup-project src/OfficeDays \
  --output-dir Data/Migrations
```
