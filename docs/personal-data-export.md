# Personal data export, version 1

Download from **Profile → Download my data (JSON)** or authenticated `GET /api/users/me/export` (cookie or bearer authentication). The response is a UTF-8 JSON attachment with `Cache-Control: no-store`. It is read-only and needs no CSRF token. Authentication failures use the normal API responses; a deleted account returns 404. The UI displays failures and permits retry.

The machine-readable contract is [personal-data-export-v1.schema.json](personal-data-export-v1.schema.json), JSON Schema draft 2020-12. All fields are required; nullable fields are emitted as `null`, and empty collections as `[]`. Dates use `YYYY-MM-DD`; timestamps use ISO 8601 with an offset. No date-range or pagination limit is applied. All database reads share one transaction snapshot.

| Field | Meaning |
| --- | --- |
| `format` | Always `officedays-personal-data`. |
| `formatVersion` | Integer `1`, independent of app release numbers. |
| `exportedAt` | UTC instant at which the snapshot was exported. Use this as report evaluation time to reproduce the snapshot's report states. |
| `application` | App name and complete assembly informational version, including build metadata when supplied by the build. Local builds have a `+local` suffix and do not uniquely identify uncommitted code. |
| `reportContext` | Calculation contract version (`1`), user-local evaluation date, and user-local registration date. Dates are saved explicitly to detect differences in timezone rules on another instance. |
| `profile` | Original user ID, username, normalized username, optional email, timezone ID, holiday jurisdiction ID, original registration timestamp, and administrator flag (informational only). |
| `preferences` | Team day as lowercase weekday name or `null`. |
| `requirementHistory` | Every requirement, including baseline and future changes: ID, owner ID, start date, percentage, creation timestamp. Baseline date `0001-01-01` applies to all history. |
| `attendance` | Every recorded day: ID, owner ID, date, manual-entry flag, creation timestamp. |
| `plans` | Every plan, including past plans: ID, owner ID, date, creation timestamp. |
| `vacations` | Every vacation: ID, owner ID, inclusive from/to dates, creation timestamp. |
| `holidayCalendar` | Referenced shared calendar's ID, code and name, plus **all stored holiday dates**, names, IDs and jurisdiction references, across all years. This is a snapshot of shared reference data, not ownership of the calendar. |

Records retain their original identifiers and relationships. Attendance/plans/holidays are sorted by date, requirements by start date, and vacations by start, end, then ID. The export is scoped to the authenticated user even for administrators; there is no user selector. Other users and unrelated calendars are never included.

## Reproducing reports

Calculation contract 1 uses Monday–Friday working days, excluding calendar holidays and inclusive vacation ranges. The requirement active on the first day of each month determines the target; required office days are rounded up. Only attendance on eligible days and on or before the evaluation date counts. Reports begin with the user's local registration month. Future months are not started; the evaluation month is in progress. Earlier months compare attendance with the target. Attendance percentage is rounded to one decimal using .NET decimal midpoint-to-even rounding; zero eligible days produce null. Summary attendance includes started months; target attainment counts completed months. Plans and team-day preferences do not count as attendance.

The export contains inputs for any supported report range, not a saved selection of report filters. Use the desired year/month range and `exportedAt` when reconstructing a report. Evaluating on today's date instead can intentionally change report states. Preserve `profile.createdAt`; using the destination account's signup date would lose earlier report months. Use the calendar snapshot instead of downloading a potentially changed calendar. Empty calendar years mean no stored holidays for those years, not that a national calendar has been verified as complete.

Only the current stored state can be exported: deleted records, previous calendar selections/edits, and overwritten settings are not retained by the application. The app currently has no notes model, so version 1 has no notes field. Passwords, password hashes, API tokens (including their metadata and hashes), sessions, cookies, signing keys, and other security secrets are excluded. Browser-only transient UI state is not account data. Keep the file private because it contains personal information.

## Future import compatibility

This feature does not implement import or promise compatibility with an importer that does not yet exist. A future importer must inspect `format`, `formatVersion`, `application.version`, and `reportContext.calculationVersion` before accepting the file. Reject unsupported versions or migrate explicitly; do not silently assume newer data has the same meaning. Any change to the exported contract requires a new format version and schema. Changes to report semantics require a new calculation version.

Remap all original record IDs and owner references to the destination account; do not trust source IDs as destination identities. Calendar IDs are instance-local: match using the code **and snapshot contents**, or create an isolated calendar to preserve calculations. Preserve original registration and record timestamps. Recompute normalized usernames under destination rules. Never grant administrator privileges from `profile.isAdmin`; authentication and authorization are established separately. Recreate credentials separately. Validate all values and relationships, handle duplicate imports, and surface timezone/calculation differences before restoring. These rules also apply to a future guest-to-account migration.
