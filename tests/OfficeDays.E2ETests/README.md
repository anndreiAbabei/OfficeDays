# OfficeDays C# end-to-end tests

This is an independent xUnit/Playwright suite. The existing TypeScript suite is unchanged.
Tests exercise the built SPA and real API over HTTP, with a new SQLite database for each run.

## Prepare and run

Use .NET 10 and Node.js 24. From the repository root:

```sh
npm --prefix src/OfficeDays.Web ci
npm --prefix src/OfficeDays.Web run build
dotnet build
dotnet run --no-build --project tests/OfficeDays.E2ETests -- install chromium
dotnet test --project tests/OfficeDays.E2ETests --no-build
```

On Linux/CI, install browser system dependencies with `install --with-deps chromium` instead.
Rebuild after editing application code. The fixture launches the already-built backend; it does
not run builds or use an existing development server. For Release builds, set
`E2E_CONFIGURATION=Release` and pass `--configuration Release` to the .NET commands.

## Record a scenario

After preparation, from the repository root:

```sh
dotnet run --no-build --project tests/OfficeDays.E2ETests -- record
```

This starts a disposable application and a visible Chromium browser, then opens Playwright
Inspector. No PowerShell installation is needed. Click **Record**, select the **C#** target,
and perform your actions. Add assertions using the visibility/text/value buttons.
The bootstrap credentials are `Admin` / `BrowserTestsOnly!123`, for this disposable instance only.
You can also register users through the UI.

Copy the generated interactions into a test, then click **Resume** to finish the recording session.
The browser and API close and the temporary database and keys are deleted. The copied recording
is not saved automatically. Avoid forcibly killing the launcher: abnormal termination can leave
an API process and temporary files behind.

Replace recorded absolute addresses with relative paths, literal usernames with scenario-specific
test users, and repeated operations with page helpers as needed. Codegen does not generate seed
setup or automatically infer assertions. To test registration, register through the UI; for other
features, use `seeder.CreateUserAsync()` to arrange a unique account.

## Add a test

Use `Features/Authentication/LoginLogoutTests.cs` as the executable example:

- Inherit `E2ETestBase`, forwarding `ApplicationFixture` and `ITestOutputHelper`.
- Apply `[Collection(ApplicationCollection.Name)]` to **every** E2E test class.
- Wrap the entire scenario, including arrangement/assertions, in `RunScenarioAsync`.
- Use the supplied page and seeder. Each invocation creates a new browser context.
- Name tests `Should_ExpectedResult_WhenCondition` and keep Arrange/Act/Assert sections.

The scenario wrapper captures a screenshot and trace on failure before closing the context.
xUnit 2 does not expose the test outcome to fixture teardown, so keeping all scenario code
inside that wrapper is necessary for automatic failure capture. Browser-startup failures may
only have test output, without a screenshot.

## Structure and isolation

- `Infrastructure/ApplicationFixture`: shared API, unique temporary database/key directory,
  OS-assigned port, readiness check, bootstrap Admin and RO jurisdiction, cleanup and API log.
- `Infrastructure/BrowserFixture`: shared headless desktop Chromium; fresh cookies/storage per context.
- `Infrastructure/ApplicationCollection`: one shared application, sequential tests.
- `Infrastructure/E2ETestBase`: per-scenario context and failure artifacts.
- `Support/TestDataSeeder`: prerequisite users created through the real API.
- `Pages`: reusable UI actions; start with `LoginPage` and add helpers when needed.
- `Features`: independent scenarios. Add `Journeys` when adding longer workflows.

Database state is shared within a run. Use unique users and independently owned data; don't depend
on test order. Shared admin changes need reliable restoration or a separate application fixture.
The suite raises authentication/registration rate limits for its shared runner IP; threshold
coverage belongs in HTTP integration tests. It does not bypass authentication, authorization,
antiforgery, or migrations. Calendar tests currently use real application time; introduce an
explicit test clock before adding date-sensitive boundary scenarios.

## Results and CI

Artifacts are under `artifacts/e2e/<run-id>/` (gitignored): API logs and, for failed scenarios,
`failure.png` and `trace.zip`. The test output prints the scenario directory.
Open a trace using the generated Playwright script after building:

```sh
pwsh tests/OfficeDays.E2ETests/bin/Debug/net10.0/playwright.ps1 show-trace /absolute/path/to/trace.zip
```

The `C# E2E` GitHub workflow runs on PRs/pushes to master and supports **Run workflow** manually.
Download `officedays-e2e` from the run's **Artifacts** section. It includes TRX results and
available diagnostics even when tests fail, retained for 14 days. This C# suite does not generate
the TypeScript runner's HTML report. Configure the check as required in GitHub branch protection
if desired. The regular .NET workflow excludes E2E execution because the dedicated job runs it.

`dotnet test` at solution level includes browser tests locally and therefore requires the above
preparation. To run only the existing backend suites, use:

```sh
dotnet test --exclude-module "**/OfficeDays.E2ETests.dll"
```
