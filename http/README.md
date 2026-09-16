# HTTP Client requests

These files target JetBrains Rider's built-in HTTP Client.

1. Open a `.http` file and select an environment from `http-client.env.json` from Rider's **Run with** menu.
2. Provide credentials using either of these approaches:
   - Copy `http-client.private.env.json.example` to `http-client.private.env.json` and replace its placeholders. The private filename is ignored by Git.
   - Start Rider with `OFFICEDAYS_API_TOKEN`, `OFFICEDAYS_USERNAME`, and `OFFICEDAYS_PASSWORD` available in its process environment. The request files read them with `{{$env.NAME}}`.
3. Run individual requests using the gutter icons.

`authentication.http` demonstrates cookie authentication and antiforgery handling. `attendance.http`, `vacations.http`, `status.http`, and `tokens.http` exercise their respective APIs with a bearer token. Attendance requests demonstrate both the optional `isManual` POST body and manual date-based PUT; duplicate dates retain their original source. `romania-2026.http` configures Romania's complete 2026 holiday calendar.

`health.http` checks API liveness and database readiness and retrieves the deployed application version. These requests use the selected environment's `baseUrl` and do not require authentication.

The Romania dates follow Article 139 of the Romanian Labour Code and the 2026 Orthodox calendar. Because both Children's Day and Orthodox Whit Monday fall on 1 June 2026, that date has one combined entry to satisfy the API's unique-date constraint.
