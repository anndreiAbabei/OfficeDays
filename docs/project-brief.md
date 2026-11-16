# OfficeDays project brief

OfficeDays is a self-hosted tracker for people with hybrid working arrangements. It answers three questions: **How many office days do I owe this month? How many have I actually attended? Can I still meet the target with the days available?** It started with an iPhone shortcut that could record a visit on arrival at the office. That origin explains two design decisions that still matter: recording today takes one authenticated request with an empty body, and retrying that request is safe.

## What a user can do

| Area | Behaviour |
| --- | --- |
| Dashboard | Record today, add a missed visit, remove an incorrect entry, and inspect how the monthly target was calculated. The breakdown includes remaining work-from-home allowance, available dates, and any unavoidable shortfall. |
| Calendar | Plan eligible dates from today onward, see attended and missed planned days, and enter vacation ranges. A preferred team weekday is a visual reminder only. |
| Profile | Choose timezone, holiday jurisdiction, appearance (System, Dark, Light), and a monthly attendance requirement. Requirements have effective dates, so a change can apply to past or future months. |
| Reports and exports | Review a year or a range of months within a year and export the report as CSV. A separate JSON download contains personal data and requirement history. The account export contract is documented in [personal-data-export.md](personal-data-export.md). |
| Accounts and administration | Users register and manage their own data and personal API tokens. Administrators maintain shared jurisdictions and dated bank holidays. Account deletion requires the current password and protects the last administrator. |
| Guest and offline use | A guest can track attendance, plans, vacations, preferences, and reports on one browser without registering. The installable web app can reopen offline once it signals readiness. |

## How the numbers work

1. Start with the weekdays in the selected calendar month. Exclude dates covered by the user's jurisdiction holidays or vacations. A vacation overlapping a holiday removes one weekday only.
2. Choose the requirement effective on the first of that month. The default is 50%. Users can specify a whole percentage (0–100) or days per five-day week (0–5). Round up **once per month**. With 22 eligible days, three days per week means `ceil(22 × 3 / 5) = 14` office days. Users can meet that target on any eligible dates.
3. Count distinct eligible dates with recorded attendance. A plan represents intent. It earns no attendance credit, even after the planned day passes.
4. For feasibility, count eligible dates from the user's today to month-end that have no recorded attendance. If 14 days are required, 10 have been recorded, and only three eligible dates remain, four days are outstanding and one is already unavoidable. An upcoming plan occupies one of those three available dates.
5. Reports begin with the registration month. The current month is in progress. Future months have targets but no actual attendance result. Only completed months contribute to the count of months that met their target.

Use date-only values for calendar days, UTC instants for timestamps, and the user's configured timezone to decide what day it is. Editing historical holidays, vacations, attendance, or an effective requirement can change the corresponding report.

## Architecture and where to look

OfficeDays ships as one ASP.NET Core application backed by SQLite. The browser UI is a Svelte 5 / SvelteKit 2 static SPA written in TypeScript. Vite proxies development API calls to the backend. For deployment, the frontend builds into `src/OfficeDays.Web/build` and the backend build copies those assets into `wwwroot`.

| Starting point | What it owns |
| --- | --- |
| [Program.cs](../src/OfficeDays/Program.cs) and [ServiceRegistration.cs](../src/OfficeDays/Configuration/ServiceRegistration.cs) | Startup, dependency injection, middleware order, authentication, request limits, structured logging, endpoint discovery, and database initialization. |
| [Features](../src/OfficeDays/Features) and [RequestExecutor.cs](../src/OfficeDays/Infrastructure/RequestExecutor.cs) | Vertical slices under each area. An endpoint binds a feature-local request and delegates to the executor, which runs validation and dispatches to the scoped handler. The handler coordinates persistence and maps results to that feature's response. |
| [Domain](../src/OfficeDays/Domain) and [Data](../src/OfficeDays/Data) | Persisted users, preferences, effective requirements, attendance, plans, vacations, tokens, and shared calendars. EF Core migrations and SQLite constraints live here. |
| [Services](../src/OfficeDays/Services) | Shared account-side calculations. Start with `AttendanceCalculator` for monthly totals, `PlanningCalculator` for per-date state, `OfficeRequirementReader` for the applicable target, and `ReportCalculator` for historical aggregation. |
| [Frontend routes](../src/OfficeDays.Web/src/routes) and [feature UI](../src/OfficeDays.Web/src/lib/features) | Routes compose screens. Feature components handle the dashboard, calendar, reports, profile, tokens, and administration. [api.ts](../src/OfficeDays.Web/src/lib/api.ts) handles credentials, CSRF, structured API errors, and expired sessions. |
| [Guest implementation](../src/OfficeDays.Web/src/lib/features/guest) | Guest storage, holiday downloads, calculations, exports, and UI. When a rule changes, check both the C# implementation and guest `calculations.ts`. |

**Trace one real request first.** An iOS arrival automation sends `POST /api/attendance` with a personal bearer token. The [RecordToday slice](../src/OfficeDays/Features/Attendance/RecordToday) resolves the authenticated user, computes today in their timezone, and writes a row keyed uniquely by user and date. A new entry returns `201 Created` with its location. A retry, including a concurrent duplicate, returns the existing entry with `200 OK`. Follow this slice end to end before adding another endpoint. [HTTP examples](../http/README.md) show the published API from the caller's side.

**Guest mode has a different source of truth.** Account data lives in SQLite. Guest data lives in versioned browser local storage. Online guest setup downloads holidays for the current and next year. Other years need an explicit download. The production app caches public assets for offline reopening. Signing in takes precedence and leaves guest history in the browser, with no automatic upload or merge. Clearing site data can erase guest history. Guest JSON backup and account JSON export are separate formats. Tokens and administration require an account.

## Working on a change

- Read [AGENTS.md](../AGENTS.md) for coding conventions and test expectations. Inspect the nearest slice, its callers, and its tests. Keep contracts, validation, mapping, and source-generated log events local to the use case. Feature discovery registers endpoints and handlers.
- Derive ownership from the authenticated user and scope every personal query accordingly. Cookie mutations use antiforgery protection. Bearer tokens are hashed, shown once at creation, revocable, and require HTTPS by default. Preserve the distinct authentication paths.
- For a calendar change, check the dashboard breakdown, planning, historical reports, CSV, and guest calculations. Useful regression cases include an overlapping holiday and vacation, a month with zero eligible days, a requirement change at a month boundary, and a user whose local date differs from UTC.
- For an API contract change, update its frontend consumer, HTTP examples, and tests together. For a schema change, add an EF migration and update the snapshot. Edit frontend sources rather than generated output.

## Run and verify

Use **.NET SDK 11 and Node.js 24** for this checkout. [Directory.Build.props](../Directory.Build.props) targets `net11.0`, [global.json](../global.json) selects Microsoft.Testing.Platform, and [CI](../.github/workflows/dotnet.yml) installs .NET 11. Some setup prose still lists .NET 10.

From the repository root, install and build the SPA, then start the backend with a fresh development database and a bootstrap password supplied through the environment. The [README](../README.md) lists timezone, jurisdiction, database, and Compose settings. Bootstrap creates the first administrator only when the database has no users.

```sh
(cd src/OfficeDays.Web && npm ci && npm run build)
# Set BootstrapAdmin__Password to a unique password of at least 12 characters.
dotnet run --project src/OfficeDays --launch-profile http
```

Open `http://localhost:5200`. For live frontend work, leave the backend running and run `npm run dev` from `src/OfficeDays.Web`. For the self-hosted deployment, Compose exposes port 5802. Persist the SQLite database and data-protection keys together. The current in-memory request limits assume a single instance. Behind HTTPS termination, configure the trusted immediate proxy so bearer requests see the original scheme. Serilog supports structured console logs and optional OpenTelemetry export.

| Change | Checks |
| --- | --- |
| Backend or API | From the root, run `dotnet restore`, `dotnet build --no-restore`, then `dotnet test --project tests/OfficeDays.UnitTests --no-build` and `dotnet test --project tests/OfficeDays.IntegrationTests --no-build`. Integration tests use the real HTTP pipeline, a fixed clock, and isolated SQLite databases. |
| Frontend or guest behaviour | From `src/OfficeDays.Web`, run `npm run check`, `npm test`, and `npm run build`. Vitest covers utilities and component behaviour. |
| Browser workflows | Build the SPA first. From the web project, run `npx playwright install` and `npm run test:e2e` for TypeScript desktop/mobile coverage. The independent [C# Playwright suite](../tests/OfficeDays.E2ETests/README.md) has its own setup and CI workflow. |
