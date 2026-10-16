# HTTP Client requests

These files target JetBrains Rider's built-in HTTP Client.

1. Open a `.http` file and select an environment from `http-client.env.json` from Rider's **Run with** menu.
2. Provide credentials using either of these approaches:
   - Copy `http-client.private.env.json.example` to `http-client.private.env.json` and replace its placeholders. The private filename is ignored by Git.
   - Start Rider with `OFFICEDAYS_API_TOKEN`, `OFFICEDAYS_USERNAME`, and `OFFICEDAYS_PASSWORD` available in its process environment. The request files read them with `{{$env.NAME}}`.
3. Run individual requests using the gutter icons.

`authentication.http` demonstrates registration with an optional email and office percentage, cookie authentication, antiforgery handling, and profile updates with response assertions. The office percentage accepts whole numbers from 0–100 and defaults to 50 when omitted during registration. Profile updates preserve the requirement; use the separate requirement examples to list, add, edit, and delete effective-month changes. The initial requirement covers all earlier history and cannot be deleted or moved. Status calculations use the percentage effective for the requested month and round required office days up to a whole day. `attendance.http`, `vacations.http`, `status.http`, and `tokens.http` exercise their respective APIs with a bearer token. Attendance requests demonstrate both the optional `isManual` POST body and manual date-based PUT; duplicate dates retain their original source. `romania-2026.http` and `romania-2027.http` configure Romania's complete holiday calendars for their respective years.

`health.http` checks API liveness and database readiness and retrieves the deployed application version. These requests use the selected environment's `baseUrl` and do not require authentication.

The Romania dates follow Article 139 of the Romanian Labour Code and the Orthodox calendar for each year. Because both Children's Day and Orthodox Whit Monday fall on 1 June 2026, that date has one combined entry to satisfy the API's unique-date constraint.

The 2027 calendar contains 17 distinct holiday dates, including Orthodox Easter on 2–3 May and Pentecost on 20–21 June. Dates were checked against [Romania’s 2027 holiday calendar](https://www.timeanddate.com/holidays/romania/2027).

`reports.http` generates the signed-in user’s report for a full year or month range. Each response is a fresh calculation; the Reports page exports its displayed response to CSV locally, without requesting another report.
