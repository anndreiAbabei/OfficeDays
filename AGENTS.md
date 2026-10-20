# Guidance for AI agents working on OfficeDays

## Scope and working approach

These instructions apply throughout this repository. Apply them to new and changed code; older code is not always an example of the desired standard. Do not perform unrelated migrations or broad style cleanups just to make existing code comply.

- Inspect the affected feature, its tests, and callers before editing. Preserve unrelated working-tree changes.
- Keep changes focused and preserve existing API behavior unless the task calls for a contract change. Update frontend consumers, tests, and `http/` examples together when contracts change.
- Fix the underlying problem and add regression coverage. Do not weaken validation, authorization, warnings, or tests to make a change pass.
- Prefer the existing stack and infrastructure over new packages, frameworks, or abstraction layers.
- Explain the resulting behavior, validation performed, and any checks that could not be run. Never claim a check passed unless it was run successfully.

## Project map and architecture

OfficeDays is a self-hosted office-attendance tracker with planning, vacations, holiday calendars, monthly attendance requirements, reports, and personal API tokens for integrations such as iOS Shortcuts.

| Location | Responsibility |
| --- | --- |
| `src/OfficeDays/` | ASP.NET Core Minimal API, targeting .NET 10 / C# 14 |
| `src/OfficeDays/Features/<Area>/<UseCase>/` | Vertical slices: endpoint, request/response contracts, handler, validation, mapping, logging |
| `src/OfficeDays/Domain/` | Shared persisted domain entities |
| `src/OfficeDays/Data/` | EF Core SQLite context, initialization, and migrations |
| `src/OfficeDays/Services/` | Shared business calculations, calendar readers, date and requirement services |
| `src/OfficeDays/Infrastructure/` | Request execution, feature discovery, and endpoint registration |
| `src/OfficeDays/Security/` | Current user, cookie/API-token authentication, and antiforgery protection |
| `src/OfficeDays.Web/` | Svelte 5 / SvelteKit 2, TypeScript, Vite, static SPA |
| `tests/OfficeDays.UnitTests/` | xUnit unit tests; NSubstitute and local testing helpers |
| `tests/OfficeDays.IntegrationTests/` | xUnit HTTP integration tests with `WebApplicationFactory<Program>` and real SQLite |
| `src/OfficeDays.Web/e2e/` | Playwright browser tests |
| `http/` | HTTP request examples and API usage documentation |

Use **Vertical Slice architecture**. Keep behavior together by use case, rather than adding application-wide controller, DTO, validator, or mapping layers. Shared domain entities and genuine cross-feature business services belong in their existing shared locations; feature contracts do not.

The frontend builds to `src/OfficeDays.Web/build`. The backend build copies that output into `wwwroot` when present, and ASP.NET Core serves the SPA. Edit frontend source, never generated `build`, `wwwroot`, `.svelte-kit`, `bin`, or `obj` output. The Vite development server proxies `/api` to the backend, defaulting to `http://localhost:5200`.

## Implementing a backend feature

Follow the existing slice layout:

```text
Features/<Area>/<UseCase>/
  Contracts/
    <UseCase>Request.cs
    <UseCase>RequestBody.cs    # when a separate body is needed
    <UseCase>Response.cs       # when a response body is returned
  <UseCase>Endpoint.cs
  <UseCase>Handler.cs
  <UseCase>RequestValidator.cs
  <UseCase>Mapping.cs
  <UseCase>HandlerLogging.cs
```

- Every feature owns its models/contracts, validators, mapping, and logging. Never reuse another feature's records, DTOs, validators, or mapping implementation, even when their shapes match. Do not create empty placeholder files for a feature with no validation, conversion, or response body to implement.
- Requests implement `IRequest`. Handlers implement `IRequestHandler<TRequest>` with the existing `ValueTask<IResult> Handle(..., CancellationToken)` contract.
- Endpoints map routes to `IRequestExecutor.Execute<TRequest>`. Keep endpoints thin; put orchestration and persistence in handlers and input rules in FluentValidation validators.
- Bind route, query, and body values explicitly using the existing request contract conventions.
- Map entities and contracts through feature-local extension methods in `<UseCase>Mapping`. Keep mappings deterministic and free of database access or other side effects. Do not introduce AutoMapper or expose EF entities directly as API responses.
- Each endpoint implements exactly one feature endpoint marker interface, such as `IPlanningEndpoint`. Area groups implement `IEndpointGroup`; `IRootEndpointGroup` is for routes outside `/api`.
- `AddFeatures` discovers exported types. Each request must have exactly one handler, and each discovered endpoint must be mapped exactly once. Keep discovered implementations public and concrete; do not add duplicate manual registrations.
- Endpoints and groups are singletons, handlers are scoped. Never capture a scoped `AppDbContext` or current-user service in a singleton. Resolve request-scoped dependencies through handlers and endpoint execution.
- Reuse `RequestExecutor` validation and logging behavior rather than bypassing it. Register genuine shared services in `Configuration/ServiceRegistration.cs` with appropriate lifetimes.

## C# standards and naming

- Prefer modern .NET 10 / C# 14 features when they improve clarity, including collection expressions, pattern matching, and extension members where appropriate. Use primary constructors only as described below. Do not change the language/framework version incidentally.
- Nullable reference types stay enabled. Model absence explicitly; avoid `null!`, unchecked casts, and warning suppression unless required and explained.
- Use `sealed class` or `sealed record` by default. Leave a type extensible only for a concrete reason, such as a framework integration requirement. Use immutable records and `init` properties for contracts where appropriate; EF entities may require mutable state.
- Use file-scoped namespaces matching the folder structure and PascalCase filenames matching the main type. Use PascalCase for types, methods, and public properties; camelCase for parameters and locals; `_camelCase` for private instance fields; and `I` prefixes for interfaces.
- Use descriptive `<UseCase>Endpoint`, `<UseCase>Handler`, `<UseCase>RequestValidator`, `<UseCase>Mapping`, and `<UseCase>HandlerLogging` names. Preserve existing interface method names such as `Handle`; use an `Async` suffix for new asynchronous methods where no existing contract dictates the name.
- Match nearby formatting; C# generally uses four-space indentation and braces on separate lines. Avoid unrelated reformatting.
- Prefer constructor injection, small methods, guard clauses, and explicit dependencies. Do not introduce service location into business logic.
- Prefer explicit constructors. A primary constructor (the record-like constructor syntax on a class) is acceptable when the class and initialization logic are simple. In either style, assign injected services to `private readonly` fields named `_camelCase` and use those fields in instance methods. Do not directly capture primary-constructor service parameters in methods or use them as implicit backing fields.
- Use async/await for I/O and pass cancellation tokens through validation, EF queries, and saves. Avoid `.Result`, `.Wait()`, `async void`, and `Task.Run` around database operations.
- Treat warnings as errors, as configured in `Directory.Build.props`. Do not relax this setting.
- Prefer to be explicit when creating an object (use `private Test _test = new Test();`; do not omit the type `private Test _test = new();`)

For example, a simple endpoint may use a primary constructor with an explicit readonly field:

```csharp
public sealed class DeleteJurisdictionEndpoint(IRequestExecutor executor) : IHolidayJurisdictionsEndpoint
{
    private readonly IRequestExecutor _executor = executor;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{code}", _executor.Execute<DeleteJurisdictionRequest>)
                 .RequireAuthorization("AdminOnly")
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
```

## HTTP contracts

- Use RESTful resource-based URLs under `/api`, with HTTP verbs expressing the operation: GET reads, POST creates, PUT replaces or idempotently sets a resource, PATCH partially updates, and DELETE removes. Prefer plural resource names and route identifiers; do not invent action URLs for ordinary CRUD.
- Preserve existing specialized authentication and operational routes. Resource design is not a reason to silently rename published routes.
- GET must not mutate state. Preserve idempotency for repeated PUT/DELETE operations according to the existing resource contract, and preserve duplicate-safe attendance recording used by Shortcuts.
- Use standard HTTP status codes consistently:

| Status | Meaning |
| --- | --- |
| `200 OK` | Successful request returning a body |
| `201 Created` | Newly created resource; include a meaningful Location when addressable |
| `204 No Content` | Successful operation with no response body |
| `400 Bad Request` | Invalid input, binding, or validation; use validation problem details for field errors |
| `401 Unauthorized` | Missing or invalid authentication |
| `403 Forbidden` | Authenticated caller lacks permission |
| `404 Not Found` | Resource absent or not visible to this caller |
| `409 Conflict` | Resource-state conflict, such as a conflicting unique resource or blocked deletion |
| `500 Internal Server Error` | Unexpected server failure, handled through the exception/ProblemDetails pipeline |

- Use `Results.Problem` and `Results.ValidationProblem` for structured errors as appropriate. Do not return `200` with an error payload, leak exception internals, or convert every exception to `400`.
- Preserve documented response shapes, date formats, and current status semantics unless deliberately changing the contract and its consumers.

## Logging and error handling

- Use source-generated logging with `[LoggerMessage]` in feature-local `public static partial` logging classes and typed `ILogger<THandler>` extension methods.
- Follow the existing event-ID ranges. Inspect neighboring logging classes before assigning IDs; avoid introducing collisions or renumbering existing events.
- Log non-happy paths such as a missing user, forbidden operation, invalid business state, or blocked deletion. Use Warning for actionable rejected operations and Error for unexpected failures. Expected idempotent duplicates or successfully resolved concurrency races may use Debug, following existing conventions.
- `RequestExecutor` already logs request execution and validation failures. Avoid duplicate logs for the same failure at multiple layers.
- Use structured placeholders, stable templates, and relevant safe identifiers. Pass exceptions through the exception parameter rather than interpolating their text.
- Never log passwords, raw API tokens, authorization headers, cookies, antiforgery tokens, or whole request bodies. Avoid unnecessary personal data.
- Preserve correlation-ID middleware and logging scopes. Do not swallow exceptions or treat cancellation as an unexpected application failure.

## Persistence, security, and domain invariants

- Use EF Core with SQLite through the existing context. Prefer async queries, projections, and `AsNoTracking` for read-only entity queries; avoid unnecessary full-table materialization and N+1 queries.
- Keep database constraints and validation aligned. Add migrations and update the model snapshot for schema changes; do not rewrite applied migrations or replace migration-based initialization with `EnsureCreated`.
- Preserve uniqueness of user/date attendance and plans, jurisdiction/date holidays, and user/effective-month requirements. Handle known unique-constraint races narrowly with the existing `IsUniqueViolation` helper; do not swallow unrelated database errors.
- Use transactions when multiple writes must succeed together. Never use a production database for tests or destructive local experiments.
- Obtain ownership from `ICurrentUser`, not caller-supplied user IDs. Scope reads and mutations to the authenticated user and verify cross-user access cannot disclose or change data.
- Apply authorization explicitly where needed; do not assume every area group requires authentication. Preserve `AdminOnly` restrictions for administrative operations.
- Cookie-authenticated mutations require `CookieAntiforgeryFilter` or the established equivalent. Preserve the bearer-token authentication path, its antiforgery distinction, and HTTPS protection. Do not weaken these protections to simplify tests.
- Preserve hashing and one-time disclosure of API tokens, password hashing, cookie settings, and persistent data-protection keys. Keep secrets out of source, logs, and fixtures intended for real deployments.
- Use `DateOnly` for calendar dates, UTC `DateTimeOffset` for instants, injected `TimeProvider` for time, and `IUserDateService` for the user's today. Do not use the server's or browser's timezone as a substitute for the user's configured timezone.
- Eligible working days exclude weekends, the user's jurisdiction holidays, and vacations. Required office days round up from the applicable percentage; only eligible recorded attendance counts.
- Plans are distinct from attendance. Planning must not record a visit; past plans cannot be changed. Team-day preferences are visual reminders, not attendance or planning actions.
- Requirement percentages are integers from 0 through 100, effective from the first day of a month. Historical reports must use the requirement applicable to that month, not just today's percentage.
- Preserve report behavior around registration month, incomplete current months, and future months. Add boundary coverage when changing calendar calculations, including month/year transitions, leap days, and timezone boundaries.

## Backend unit testing

- Use xUnit and NSubstitute, with test folders/namespaces mirroring the production feature or service. Name classes `<TypeUnderTest>Tests` or the existing more specific method-focused equivalent.
- Name test methods `Should_{ExpectedResult}_When{UseCase}`, for example `Should_ReturnFailure_WhenNameIsNull`. Follow this for new and changed tests even where older tests use shorter names.
- Use explicit Arrange, Act, Assert sections. Keep each test focused on one behavior; use theories for meaningful input variations.
- Store expected values in descriptive variables such as `expectedDate` or `expectedStatusCode` and use `Assert.Equal(expectedValue, actualValue)`. Do not inline expected literals in equality assertions. `Assert.True`/`Assert.False` are fine when the expected boolean is not reused elsewhere. If possible those variables can be const variables (eg. `const string expectedName = "Jack"; var expectedDate = DateTime.Now;`)
- When comparing reference types (non-strings) use `Assert.Same` if it makes more sense 
- Include tests for DTOs/models/records: constructor or initializer values, getters/setters or init properties, defaults, nullability behavior, and relevant equality semantics. Do not test compiler-generated machinery beyond the public contract.
- Unit-test validators, mapping extensions, source-generated logging methods, endpoints, and shared calculations. Handler behavior may be exercised through integration tests instead of duplicating those tests with mocked persistence.
- Mapper tests assert every mapped field and relevant null/optional cases. Logging tests verify level, message and relevant structured values/event IDs using `Testing/FakeLogger`; extend the helper when needed.
- Endpoint tests verify route, verb, executor/request wiring, and security metadata or filters. Reuse `Assert.Endpoint<TRequest>` from `Testing/CustomAssertions.cs` for its supported checks and add assertions for metadata it does not cover.
- Use fixed dates and injected clocks. Avoid sleeps, real network access, and assumptions about execution order or wall-clock time.

## Backend integration testing

- Use `Support/OfficeDaysFactory` and `ApiTestHelpers`. The factory starts the real pipeline in `Testing`, uses a unique temporary SQLite database, and replaces `TimeProvider` with a fixed clock.
- Exercise handlers through HTTP, including binding, validation, authentication, authorization, antiforgery, persistence, and response contracts. Do not replace relational coverage with EF's in-memory provider.
- For affected features, cover success, invalid input, missing resources, unauthenticated access, insufficient permissions, and cross-user isolation. Include duplicate/idempotent requests and relevant persistence conflicts.
- Test cookie mutations with and without valid CSRF tokens, and bearer authentication for endpoints that support it. Cover revoked/invalid tokens and HTTPS enforcement when changing security behavior.
- Assert status, relevant body/headers, and persisted effects, including the absence of changes after rejected requests. Use fresh contexts/scopes where necessary to avoid asserting only tracked state.
- Keep test data isolated and clean up resources. Do not depend on shared developer data, test execution order, or an externally running app.
- Add migration coverage for schema changes and feature registration/mapping coverage when changing discovery. Maintain the existing infrastructure and security regression suites.

## TypeScript and Svelte

- Use `type` aliases for TypeScript type declarations, not `interface`. Use `import type` for type-only dependencies.
- Avoid `any`. An exceptional use must have a nearby comment explaining why a safer type cannot express it. Prefer `unknown` with narrowing for untrusted values and errors.
- Explicitly declare function and method return types, including arrow-function helpers and async `Promise<T>` results. Constructors do not have return annotations.
- Prefer async/await to promise chains, and handle rejected operations and loading cleanup. Avoid floating promises and unsafe non-null assertions.
- Use PascalCase for Svelte components and type aliases, camelCase for functions and variables, and the established SvelteKit route filenames. Match nearby formatting.
- Keep route files thin; place feature UI in `src/lib/features`, reusable UI in `src/lib/components`, and shared transport/date utilities in `src/lib`.
- Keep new feature API types local to their feature. The existing `src/lib/types.ts` is a legacy shared contract location; do not use it to introduce new coupling between feature contracts. Reusable UI primitives can remain shared.
- Route API calls through `src/lib/api.ts` (`request`/`mutate`) to preserve credentials, CSRF, ProblemDetails handling, and expired-session behavior. Do not duplicate fetch/authentication logic in components.
- Preserve the static SPA architecture; do not introduce server-only SvelteKit dependencies without an explicit architectural need.
- Use existing date helpers and date-only strings for calendar values. Avoid parsing a date in a way that shifts its displayed day across timezones.
- Preserve the theme and responsive behavior. Use semantic HTML, accessible names and labels, keyboard interaction, visible focus, and clear loading/empty/error states. Do not convey calendar state by color alone.

## Frontend testing

- Use Vitest with Testing Library for utilities and components. Tests live alongside code as `*.test.ts`; browser setup is in `src/test-setup.ts`.
- Use behavior-focused test names with the same `Should_..._When...` convention for new tests. Apply Arrange/Act/Assert and named expected values where relevant.
- Query components by accessible role/name where possible. Assert visible behavior rather than internal implementation; cover pending, successful, failed, and empty states for affected interactions.
- Use Playwright `e2e/*.spec.ts` for full browser workflows. Preserve configured desktop Chromium, iPhone WebKit, and Android Chromium coverage for responsive changes.
- The Playwright configuration starts a backend with isolated database/key paths. Build the frontend first so the backend serves current assets; do not point tests at a personal or production instance.
- Use stable locators and awaited assertions rather than arbitrary delays. Include regression coverage for authentication, calendar interactions, requirements, and reports when those flows change.

## Validation commands and completion

Use .NET 10 and Node.js 24 (the CI/Docker baseline). Install frontend dependencies with the committed npm lockfile using `npm ci`.

From `src/OfficeDays.Web`:

```sh
npm ci
npm run check
npm test
npm run build
```

Then from the repository root, matching the backend CI sequence:

```sh
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

For browser workflow changes, after the frontend build, run from `src/OfficeDays.Web`:

```sh
npx playwright install
npm run test:e2e
```

Browser installation may require additional platform dependencies. Report missing prerequisites rather than silently skipping browser coverage. Targeted development checks are useful, but run the affected suites and applicable build/type checks before delivery. Documentation-only changes need content/path review rather than application test execution.

The current GitHub workflow builds the frontend and runs .NET build/tests; it does **not** currently run Svelte type checks, Vitest, or Playwright. Run those explicitly when relevant rather than treating a passing CI build as frontend test coverage.

Before finishing, review the diff for unrelated edits, secrets, generated files, contract mismatches, and missing tests. Update `README.md` for user-facing setup or behavior changes and `http/` examples for changed API usage.
