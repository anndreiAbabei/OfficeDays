# HTTP Client requests

These files target JetBrains Rider's built-in HTTP Client.

1. Open a `.http` file and select an environment from `http-client.env.json` from Rider's **Run with** menu.
2. Provide credentials using either of these approaches:
   - Copy `http-client.private.env.json.example` to `http-client.private.env.json` and replace its placeholders. The private filename is ignored by Git.
   - Start Rider with `OFFICEDAYS_API_TOKEN`, `OFFICEDAYS_USERNAME`, and `OFFICEDAYS_PASSWORD` available in its process environment. The request files read them with `{{$env.NAME}}`.
3. Run individual requests using the gutter icons.

`authentication.http` demonstrates registration with an optional email and attendance requirement, cookie authentication, antiforgery handling, and profile updates with response assertions. Requirements use `requirementMode` and `requirementValue`: Percentage accepts 0–100 and DaysPerWeek accepts 0–5. Registration defaults to Percentage with value 50 when both are omitted. Profile updates preserve the requirement; use the separate requirement examples to list, add, edit, and delete effective-month changes. The initial requirement covers all earlier history and cannot be deleted or moved. Status calculations use the percentage effective for the requested month and round required office days up to a whole day. `attendance.http`, `vacations.http`, `status.http`, and `tokens.http` exercise their respective APIs with a bearer token. Attendance requests demonstrate both the optional `isManual` POST body and manual date-based PUT; duplicate dates retain their original source. `romania-2026.http` and `romania-2027.http` configure Romania's complete holiday calendars for their respective years.

`health.http` checks API liveness and database readiness and retrieves the deployed application version. These requests use the selected environment's `baseUrl` and do not require authentication.

The Romania dates follow Article 139 of the Romanian Labour Code and the Orthodox calendar for each year. Because both Children's Day and Orthodox Whit Monday fall on 1 June 2026, that date has one combined entry to satisfy the API's unique-date constraint.

The 2027 calendar contains 17 distinct holiday dates, including Orthodox Easter on 2–3 May and Pentecost on 20–21 June. Dates were checked against [Romania’s 2027 holiday calendar](https://www.timeanddate.com/holidays/romania/2027).

`reports.http` generates the signed-in user’s report for a full year or month range. Each response is a fresh calculation; the Reports page exports its displayed response to CSV locally, without requesting another report.


### Rate limits

Every `/api` request may return `429 application/problem+json` with `Retry-After` (seconds) and `retryAfterSeconds`. Wait at least this duration before retrying, including in iOS Shortcuts; avoid immediate retry loops. All tokens share their user's total allowance and each token also has its own limit. Attendance retries remain duplicate-safe. Oversized request bodies return 413. See the README's request-limit table for defaults and deployment constraints.

Account deletion is documented in [account-deletion.http](account-deletion.http). Use only a disposable account when trying this irreversible operation. It requires a browser cookie, CSRF token, and current password; an API token is insufficient. See the root README for backup retention and restore requirements.

### Requirement modes

Registration and requirement POST/PUT accept `"requirementMode": "Percentage", "requirementValue": 60` or `"requirementMode": "DaysPerWeek", "requirementValue": 3`. Values must be integers: 0–100 for percentage, 0–5 for days per week. Supply both fields together. Requirement POST/PUT requires both fields; percentage-only payloads are no longer supported. Registration still defaults to 50% when no requirement is supplied.

Account, requirement-history, report-month and personal-export requirement responses include `requirementMode` and `requirementValue`, retaining the original choice. There is no separate equivalent-percentage field. CSV reports include requirement mode and value instead of a required-office-percentage column.

Weekly requirements are monthly averages: `ceil(eligible working days × days per week / 5)`. Holidays and vacations reduce eligible days, and rounding occurs once for the month. Attendance can be distributed freely among eligible dates. Effective-month history and baseline protections apply equally to both modes. Migrating an existing database preserves all percentage history. Downgrading converts weekly requirements to equivalent percentages, so their original display format cannot survive a downgrade.
