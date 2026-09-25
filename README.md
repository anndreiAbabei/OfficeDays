# Office Days Tracker

Office Days Tracker is a small, self-hosted application for recording office attendance and checking progress against a personal office attendance requirement, **50% of eligible working days by default**.

For each calendar month, the application subtracts weekends, centrally configured bank holidays, and the user's vacation dates. It then calculates:

```text
required office days   = ceil(eligible working days × required office percentage / 100)
maximum WFH days       = eligible working days - required office days
remaining office days  = max(0, required office days - eligible office attendance)
```

Users can set a whole percentage from 0–100 during registration or in Profile. Existing accounts default to 50%. Changing the percentage recalculates all viewed months using the current setting. The API accepts `requiredOfficePercentage` on registration and profile updates, and returns it with user details. Omitting it during registration defaults to 50; omitting it during a profile update preserves the saved value.

With 21 eligible days at 50%, this means at most 10 WFH days and therefore at least 11 office days.

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
- A holiday jurisdiction for each user, such as `RO`, `GB-ENG`, or `GB-NIR`

## Run locally

The first start requires bootstrap administrator environment variables. They are intentionally absent from `appsettings.json`. Passwords must contain at least 12 characters.

```bash
BootstrapAdmin__Password='use-a-long-unique-password' \
BootstrapAdmin__TimeZoneId='Europe/Bucharest' \
BootstrapAdmin__CountryCode='RO' \
BootstrapAdmin__CountryName='Romania' \
Security__RequireHttpsForBearerTokens=false \
dotnet run --project src/OfficeDays
```

Open the URL printed by ASP.NET Core and sign in as `Admin`. The HTTPS bearer-token check is disabled above only for local development. EF Core migrations run automatically on startup and create `officedays.db` in the working directory.

If the database already contains any user, bootstrap configuration is ignored. It never resets or modifies the existing `Admin` account. On an empty database, the configured jurisdiction is created if necessary. If any required bootstrap setting is missing, startup fails with a clear error.

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

Docker Compose uses two host bind mounts:

```text
./data                                      SQLite database and data-protection keys
./src/OfficeDays/appsettings.Production.json non-secret production settings (read-only)
```

Create the writable data directory and copy the environment template:

```bash
mkdir -p data
cp .env.example .env
```

Edit `.env` before the first start:

- Replace `OFFICEDAYS_ADMIN_PASSWORD` with a strong password of at least 12 characters.
- Leave `OFFICEDAYS_DATA_PATH=./data` for a local checkout, or set it to a stable absolute host path when a deployment manager such as Komodo controls the checkout.
- On Linux, set `OFFICEDAYS_UID` and `OFFICEDAYS_GID` to the output of `id -u` and `id -g`. This lets the non-root container process write to `./data`.
- Keep `OFFICEDAYS_REQUIRE_HTTPS_FOR_BEARER=true` when using an HTTPS reverse proxy.

Build and start the application:

```bash
docker compose up --build -d
docker compose logs -f office-days
```

The service listens on port `8080`. Database migrations and initial Admin creation happen automatically. The bind-mounted directory will contain:

```text
data/officedays.db   SQLite database
data/keys/           ASP.NET Core data-protection keys used by login cookies
```

Rebuilding or replacing the container does not delete these files. Back up the entire `data` directory; retaining both the database and key ring prevents existing browser sessions from becoming invalid after restoration.

After the Admin account has been created successfully, `OFFICEDAYS_ADMIN_PASSWORD` may be removed from `.env`. Bootstrap values are ignored whenever the database already contains a user.

To stop or update the application:

```bash
docker compose down

git pull
docker compose up --build -d
```

`src/OfficeDays/appsettings.Production.json` sits beside the other ASP.NET Core settings files and is mounted read-only. It is suitable for cookie lifetime, logging, and security switches, but passwords and other secrets should remain in `.env`. Environment variables override values from the JSON file.

### Deploy with Komodo

Create Office Days as a **Stack** rather than a standalone Deployment so Komodo can use the repository's `compose.yaml` and build the Dockerfile:

1. Push this repository to a Git server that the Komodo host can access.
2. On the Docker host, create a persistent directory such as `/opt/appdata/office-days`, owned by the UID/GID that the container will use.
3. In Komodo, open **Stacks**, select **New Stack**, give it a name such as `office-days`, and select the target server.
4. Choose the Git repository source, select the repository and branch, and set the Compose file path to `compose.yaml`.
5. In Komodo's **Settings → Variables**, create a variable named `OFFICE_DAYS_BOOTSTRAP_PASSWORD`, enable its **Secret** option, and give it the initial Admin password.
6. Add the following values to the Stack's **Environment** field. The double-bracket expression asks Komodo to substitute the secret without committing it to Git:

```dotenv
OFFICEDAYS_ADMIN_PASSWORD=[[OFFICE_DAYS_BOOTSTRAP_PASSWORD]]
OFFICEDAYS_ADMIN_TIMEZONE=Europe/Bucharest
OFFICEDAYS_ADMIN_COUNTRY_CODE=RO
OFFICEDAYS_ADMIN_COUNTRY_NAME=Romania
OFFICEDAYS_DATA_PATH=/opt/appdata/office-days
OFFICEDAYS_UID=1000
OFFICEDAYS_GID=1000
OFFICEDAYS_REQUIRE_HTTPS_FOR_BEARER=true
```

7. Save the Stack and deploy it. Port `5802` on the Docker host will serve the application.

The absolute data path is important in Komodo: it keeps the SQLite database and login-cookie keys outside Komodo's Git checkout. After the first successful startup creates `Admin`, the bootstrap password can be removed from the Stack environment. Put an HTTPS reverse proxy in front of port `5802` before using bearer tokens outside a trusted network.

### HTTPS deployment

Put the application behind an HTTPS reverse proxy before exposing it outside a trusted network. API bearer tokens are credentials and must never be transmitted over public plain HTTP.

By default, the application rejects any bearer-authenticated request for which ASP.NET Core does not see HTTPS. It is normal for TLS to terminate at the reverse proxy and for the proxy-to-application connection to use HTTP. The proxy must preserve the original public scheme so the application sees the request as secure:

```text
Client --HTTPS--> Nginx --HTTP + X-Forwarded-Proto: https--> Office Days
```

For Nginx or OpenResty, the proxy location should include:

```nginx
proxy_set_header Host $host;
proxy_set_header X-Forwarded-Proto $scheme;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Real-IP $remote_addr;
```

ASP.NET Core trusts forwarded headers from loopback proxies by default. When Nginx and Office Days run in different containers, Nginx normally reaches the application from a Docker-network address rather than loopback. The Compose configuration therefore enables forwarded-header processing inside the container with:

```yaml
ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"
```

Komodo's Stack Environment is supplied to Docker Compose as an environment file. Values placed there are available for Compose interpolation but are not automatically passed into the application container unless `compose.yaml` includes them under the service's `environment` section. After changing container environment configuration, use **Redeploy** rather than only restarting the existing container.

`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` accepts forwarded headers from any connecting proxy. Use it only when direct access to the application's HTTP port is restricted to trusted systems. If Nginx runs directly on the Docker host, the published port can be restricted to loopback:

```yaml
ports:
  - "127.0.0.1:5802:5802"
```

If Nginx runs in a container, prefer attaching Nginx and Office Days to a shared Docker network and avoiding a publicly reachable application port. A stricter alternative is to configure the exact proxy address or Docker subnet as a trusted ASP.NET Core proxy/network.

If an HTTPS request with a bearer token returns `400 HTTPS required`, verify that Nginx sends `X-Forwarded-Proto` and that forwarded-header processing is enabled inside the Office Days container.

For an explicitly trusted internal-only HTTP network, the check can be disabled in `.env` with:

```text
OFFICEDAYS_REQUIRE_HTTPS_FOR_BEARER=false
```

Do not disable this for an internet-facing deployment. The browser session and antiforgery cookies are `HttpOnly`/`SameSite=Strict`; they automatically receive the `Secure` flag when the request is HTTPS.

## Create a normal user

Email is optional at registration: omit `email` or pass `null`. User responses (registration, login, and `/api/auth/me`) include the nullable `email` field. The Profile section lets signed-in users save or clear their email.

`PUT /api/users/me` updates the authenticated user's email and returns the updated user. Send `{"email":"person@example.com"}` to set it, or `{"email":null}` to clear it. Empty/whitespace values also clear it; surrounding whitespace is trimmed. Supplied addresses must be valid and at most 254 characters. Cookie requests require the usual `X-CSRF-TOKEN`; bearer authentication is also supported. Email verification and password recovery are not implemented yet.

From the sign-in page, choose **Create an account** (or open `/#register`). Enter a username, a password of at least 12 characters, timezone, and holiday calendar. Email is optional. After registration, sign in with the new account.

`POST /api/users` is the self-registration endpoint. It is intentionally anonymous and always creates a normal user. Its DTO has no `isAdmin` field; extra JSON such as `"isAdmin": true` cannot grant administration.

```bash
curl -X POST https://office.example.com/api/users \
  -H 'Content-Type: application/json' \
  -d '{
    "username": "andrew",
    "password": "a-long-unique-password",
    "timeZoneId": "Europe/Bucharest",
    "countryCode": "RO"
  }'
```

Usernames are unique without regard to case. The country code must identify an existing holiday jurisdiction. The per-user timezone determines the calendar date recorded by `POST /api/attendance`, regardless of the container's timezone. The user's holiday jurisdiction determines which bank holidays are excluded from status calculations.

## Web UI

After login, everything is on one dashboard:

- month/year navigation and compliance status
- attendance history with a minimal past-day add/remove fallback
- vacation range add/remove
- API token creation and revocation
- admin-only holiday-jurisdiction management
- an admin-only bank holiday editor

Manual attendance uses idempotent `PUT /api/attendance/{yyyy-MM-dd}` and does not allow future dates. Shortcut attendance uses `POST /api/attendance` with no body by default. It also accepts an optional JSON body `{ "isManual": false }`; omitted `isManual` defaults to false. Send `{ "isManual": true }` to record today manually. PUT always creates manual entries. Both endpoints preserve the original source when the date already exists.

Attendance responses include `isManual`. In the Office days list, automatic entries have green checkmarks and manual entries have yellow checkmarks, with source tooltips and accessible labels. The UI uses PUT, so all entries it creates are manual. The `AddAttendanceIsManual` migration marks all existing attendance as manual without changing their dates or timestamps.

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

## Holiday jurisdictions

Holiday jurisdictions represent either a country-wide calendar (`RO`, `US`) or a regional calendar (`GB-ENG`, `GB-NIR`). Codes are normalized to uppercase. The common `UK` prefix is accepted and normalized to the ISO `GB` prefix.

Jurisdiction reads are public so registration clients can discover valid country codes. Creating, renaming, and deleting jurisdictions requires an administrator:

```text
GET    /api/holiday-jurisdictions
POST   /api/holiday-jurisdictions          { "code": "GB-NIR", "name": "Northern Ireland" }
PUT    /api/holiday-jurisdictions/{code}   { "name": "Northern Ireland" }
DELETE /api/holiday-jurisdictions/{code}
```

Codes are immutable and unique. A jurisdiction referenced by a user or bank holiday returns `409 Conflict` when deletion is attempted.

## Bank holidays

Sign in as `Admin`, choose a jurisdiction and year, edit the bank holiday list, and save. Holiday reads are public; only administrators can replace them.

The API has full replacement semantics:

```http
GET /api/bank-holidays/RO/2026
PUT /api/bank-holidays/RO/2026
```

Example request using an Admin-owned API token:

```bash
curl -X PUT https://office.example.com/api/bank-holidays/RO/2026 \
  -H 'Authorization: Bearer ADMIN_API_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '[
    { "date": "2026-01-01", "name": "New Year’s Day" },
    { "date": "2026-12-25", "name": "Christmas" }
  ]'
```

After the request, those are exactly the holidays stored for Romania in 2026. Omitted dates are removed, dates must belong to the URL year, and duplicate dates are rejected. Different jurisdictions may configure the same date independently.

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
- Status calculations select bank holidays through the user's required holiday-jurisdiction foreign key.
- Cookie-authenticated modifying operations require ASP.NET Core antiforgery tokens.
- Production exception handling returns Problem Details without stack traces.
- Application events use structured ASP.NET Core logging. Passwords and raw API tokens are never logged.

## Tests

Run everything with:

```bash
dotnet test OfficeDays.sln
```

The unit suite covers odd/even rule behavior, weekdays and weekends, holidays, all vacation overlap cases, remaining-day clamping, ineligible attendance, leap years, and period boundaries.

The integration suite uses `WebApplicationFactory`, the real middleware/endpoints, and a fresh migrated SQLite file per test. It covers registration/login, admin jurisdiction management, country-scoped holiday replacement, token creation/use/revocation and ownership isolation, idempotent bearer attendance and removal, vacation ownership/lifecycle, and persisted status calculations.

Rider-compatible manual API requests are available under `http/`. Select the `local` or `docker` HTTP Client environment and provide secrets through the ignored `http/http-client.private.env.json` file or the documented operating-system variables.

## Database migrations

Migrations are committed under `src/OfficeDays/Data/Migrations` and applied at startup. To create a migration after changing the model:

```bash
dotnet ef migrations add NameOfChange \
  --project src/OfficeDays \
  --startup-project src/OfficeDays \
  --output-dir Data/Migrations
```

The `AddHolidayJurisdictions` migration assigns existing users and existing bank holidays to the seeded `RO` jurisdiction. This preserves the data from installations created before jurisdiction support was introduced.

## Application version and health

`GET /api/version` returns the running application's embedded version, for example
`{"version":"1.0.0+build.20260916.143025"}`. The footer fetches this endpoint on page
load, including before login. The response disables caching.

The release version is maintained in `src/OfficeDays/OfficeDays.csproj` (`Version`).
Docker publish automatically appends a UTC build timestamp. Komodo needs no extra
configuration: pulling master and building via Compose embeds the stamp in the image.
This identifies a build, not a push counter: a cached image retains its version,
a fresh rebuild gets a new stamp, and a rollback shows the older image's version.
Local .NET builds display `1.0.0+local`. Change `Version` manually only when choosing
a new release number. Refresh the page after deployment to see the running version.

Public monitoring endpoints:

- `GET /health/live`: HTTP 200 when the HTTP application is running, without querying SQLite.
- `GET /health/ready`: queries the application's Users table; HTTP 200 when healthy,
  HTTP 503 on database failure. JSON includes overall status and database status,
  without exception details. Responses are not cached.

Startup database initialization must finish before the server accepts requests.
Readiness verifies a read query, not write access or every table in the schema.
SQLite `.db-wal`, `.db-shm`, and `.db-journal` files are runtime database sidecars;
they are excluded from Git and Docker build contexts. Do not delete them while the
application is running, since the WAL can contain uncheckpointed data.

## UI asset caching

The application serves JavaScript and CSS at content-addressed URLs such as
`/assets/<sha256>/app.js`. Each file's URL changes automatically when its contents
change, for both local runs and Docker deployments. Files are loaded at application
startup; restart the app after editing UI assets during local development.

The HTML response (including `/index.html` and client-side fallback routes) sends
`Cache-Control: no-store` so it references the current asset URLs. Fingerprinted
assets can be cached for a year because each URL identifies fixed content. Legacy
`/app.js` and `/styles.css` routes also send `no-store`.

Keep Komodo's **Build Images → Pre Build Images** enabled so deployments rebuild
the application from the selected Git branch. Save and deploy after merging changes.
Reverse proxies should respect the application's HTML cache headers; if a proxy
already cached old HTML or overrides `no-store`, purge that HTML cache once and
remove the override. Old cached unversioned CSS/JS are bypassed by the new HTML.
