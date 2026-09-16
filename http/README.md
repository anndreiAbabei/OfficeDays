# HTTP Client requests

These files target JetBrains Rider's built-in HTTP Client.

1. Open a `.http` file and select `local` or `docker` from Rider's **Run with** menu.
2. Provide credentials using either of these approaches:
   - Copy `http-client.private.env.json.example` to `http-client.private.env.json` and replace its placeholders. The private filename is ignored by Git.
   - Start Rider with `OFFICEDAYS_API_TOKEN`, `OFFICEDAYS_USERNAME`, and `OFFICEDAYS_PASSWORD` available in its process environment. The request files read them with `{{$env.NAME}}`.
3. Run individual requests using the gutter icons.

`authentication.http` demonstrates cookie authentication and antiforgery handling. `api.http` exercises the main user APIs with a bearer token. `romania-2026.http` configures Romania's complete 2026 holiday calendar.

The Romania dates follow Article 139 of the Romanian Labour Code and the 2026 Orthodox calendar. Because both Children's Day and Orthodox Whit Monday fall on 1 June 2026, that date has one combined entry to satisfy the API's unique-date constraint.
