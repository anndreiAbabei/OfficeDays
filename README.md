# Office Days Tracker

A self-hosted app for keeping track of office days, planning upcoming visits, and seeing how close you are to your monthly attendance target.

## Table of contents

- [Why it exists](#why-it-exists)
- [What you can do](#what-you-can-do)
- [Requirements](#requirements)
- [Run with Docker Compose](#run-with-docker-compose)
- [Run locally without Docker](#run-locally-without-docker)
- [First-time setup](#first-time-setup)
- [Everyday use](#everyday-use)
- [iOS Shortcut setup](#ios-shortcut-setup)
- [Backups and hosting](#backups-and-hosting)
- [License](#license)

## Why it exists

Office Days started with a simple idea: I wanted to record an office day from the **iOS Shortcuts app**. Arrive at the office, let a shortcut log the visit, and have somewhere to check how many days are still needed that month.

That idea grew into a web app with a calendar, vacation tracking, attendance targets, and reports. Shortcuts remains part of the experience: you can record attendance with one request, either by running a shortcut yourself or through an arrival automation.

## What you can do

- **Track attendance:** record today, add a missed office day, or remove an incorrect entry.
- **See monthly progress:** check your required office days, remaining days, and maximum work-from-home days.
- **Plan ahead:** mark upcoming office days on the calendar. Plans stay separate from recorded attendance.
- **Account for time off:** exclude weekends, bank holidays, and vacations from your target.
- **Set your own target:** choose an office attendance percentage, then manage past or future changes from Profile.
- **Highlight your team day:** choose a weekday as a visual reminder in the calendar.
- **Review and export reports:** view monthly attendance across a year or a range of months and download a CSV.
- **Automate with iOS Shortcuts:** use a personal API token to log attendance from your iPhone.
- **Share an installation:** each account has its own attendance, plans, vacation dates, timezone, and holiday calendar.

### How the target works

The default target is **50% of eligible working days**. Eligible days are weekdays, excluding your holiday calendar's bank holidays and your vacation dates. The required number of office days is rounded up.

For example, with **21 eligible working days** and a **50% target**, you need **11 office days**, leaving up to **10 work-from-home days**. Only recorded attendance on eligible days counts toward that target; planning a visit does not mark it as attended.

You can choose any whole percentage from 0 to 100. Later changes take effect from the first day of a chosen month. Editing an earlier requirement recalculates the affected months.

## Requirements

Choose whichever setup suits you:

| Run with | What you need |
| --- | --- |
| Docker Compose | Docker with Compose support. No separate .NET or Node.js installation needed. |
| Local installation | .NET 10 SDK, Node.js 24, and npm. |

Both options use a local SQLite database, so there is no separate database server to install. Start with a copy of this repository and run the commands below from its root directory. The local installation examples use a macOS/Linux shell.

## Run with Docker Compose

### 1. Prepare the settings

```bash
mkdir -p data
cp .env.example .env
```

Example `.env` file (replace the password before starting):

```dotenv
OFFICEDAYS_ADMIN_PASSWORD=replace-with-a-long-unique-password
OFFICEDAYS_ADMIN_TIMEZONE=Europe/Bucharest
OFFICEDAYS_ADMIN_COUNTRY_CODE=RO
OFFICEDAYS_ADMIN_COUNTRY_NAME=Romania
OFFICEDAYS_DATA_PATH=./data
OFFICEDAYS_UID=1000
OFFICEDAYS_GID=1000
OFFICEDAYS_REQUIRE_HTTPS_FOR_BEARER=true
```

Adjust these settings for your installation:

- `OFFICEDAYS_ADMIN_PASSWORD`: a unique password of at least 12 characters for the initial `Admin` account.
- `OFFICEDAYS_ADMIN_TIMEZONE`: your timezone, such as `Europe/Bucharest`.
- `OFFICEDAYS_ADMIN_COUNTRY_CODE` and `OFFICEDAYS_ADMIN_COUNTRY_NAME`: your initial holiday calendar, such as `RO` and `Romania`.
- `OFFICEDAYS_DATA_PATH`: leave as `./data` for a local installation, or choose a persistent folder of your own.

On Linux, set `OFFICEDAYS_UID` and `OFFICEDAYS_GID` to the values returned by `id -u` and `id -g`. The data folder must be writable by that user.

For testing API tokens over local HTTP, set `OFFICEDAYS_REQUIRE_HTTPS_FOR_BEARER=false`. Keep it `true` when using HTTPS, and use HTTPS for access over the internet.

### 2. Start the app

```bash
docker compose up --build -d
```

Open **http://localhost:5802** and sign in as **Admin** with the password you set. The database and initial administrator account are created on the first start.

To view startup messages:

```bash
docker compose logs -f office-days
```

### 3. Stop or update

Stop the app:

```bash
docker compose down
```

To update a Git checkout to the latest version and rebuild:

```bash
git pull
docker compose up --build -d
```

Your data lives in the configured data folder and survives container rebuilds. Back it up before updating; see [Backups and hosting](#backups-and-hosting).

## Run locally without Docker

### 1. Install and build the web interface

```bash
cd src/OfficeDays.Web
npm ci
npm run build
cd ../..
```

### 2. Start the app

Replace the example password and adjust the timezone and holiday calendar as needed:

```bash
BootstrapAdmin__Password='replace-with-a-long-unique-password' \
BootstrapAdmin__TimeZoneId='Europe/Bucharest' \
BootstrapAdmin__CountryCode='RO' \
BootstrapAdmin__CountryName='Romania' \
Security__RequireHttpsForBearerTokens=false \
dotnet run --project src/OfficeDays --launch-profile http
```

Open **http://localhost:5200** and sign in as **Admin**. The HTTP token setting above is for local testing.

The app creates its SQLite database automatically. Bootstrap settings only create the administrator when the database has no users; changing the bootstrap password later does **not** reset an existing account's password.

If you change the web interface, rebuild it and restart the app. For live frontend development, keep the app running and run `npm run dev` from `src/OfficeDays.Web`, then open the address it prints.

## First-time setup

1. **Set up bank holidays.** Sign in as Admin and open **Admin**. Add any country or regional calendars you need, then enter the bank holidays for the relevant year. Holiday dates are maintained by the administrator, so check them each year.
2. **Create your everyday account.** From the sign-in page, choose **Create an account**. Pick a username, a password of at least 12 characters, your timezone, holiday calendar, and attendance percentage. Email is optional.
3. **Add your vacation dates.** Use the dashboard so your monthly target reflects time off.
4. **Make it yours.** In **Profile**, choose a team day and adjust your attendance requirements if needed.

Your timezone determines what “today” means when a shortcut records attendance. Choose the timezone you want to use for tracking your office days.

Email verification and password recovery are not currently available.

## Everyday use

The **dashboard** is the place to check your monthly target, record visits, manage vacations, and plan upcoming office days. Navigate between months to review earlier attendance or plan ahead. You can plan eligible dates from today onward; past plans cannot be changed.

The calendar distinguishes recorded attendance, upcoming plans, missed plans, and time off. A team day highlight is just a reminder—it does not automatically plan or record a visit.

Open **Reports** to select a year and month range, generate a report, and export it as CSV. Reports start with the month you registered. The current month is shown as in progress, and future months are shown as not started. Refresh a report after changing attendance, vacations, or requirements to include those changes in the export.

## iOS Shortcut setup

You will need an account, a personal API token, and an app address your iPhone can reach. For a hosted installation, use an HTTPS address such as `https://office.example.com`.

**`localhost` on your iPhone refers to the phone itself.** When testing an app running on your computer, use the computer's local network address and make sure the phone can reach it. An arrival automation at the office also needs access to your server from there—for example, through your hosted address or VPN.

### 1. Create a token

Sign in with your everyday account and open **API tokens**. Create a token with a name such as `My iPhone` and copy it immediately: the full token is only shown once.

Treat it like a password. Do not share a shortcut containing your token. If it is lost or exposed, revoke it in the app and create a new one.

### 2. Create and test the shortcut

In the iOS **Shortcuts** app, create a shortcut named **Record office day** and add **Get Contents of URL** with these settings:

| Setting | Value |
| --- | --- |
| URL | `https://office.example.com/api/attendance` — replace the domain with your app's address |
| Method | `POST` |
| Header name | `Authorization` |
| Header value | `Bearer YOUR_TOKEN` — replace `YOUR_TOKEN` with the token you copied |
| Request body | Leave empty; no fields are needed |

Run the shortcut and allow access to your app's address if prompted. Open the dashboard and check that today's attendance appears. This test records a real office day; remove it from the dashboard if you are only trying the setup.

You do not need to send a date, username, or user ID. The token identifies your account, and the app records today in your configured timezone. Running it more than once on the same day will not create duplicate attendance.

### 3. Run it when you arrive at the office

Create a personal automation with **Arrive** as the trigger and choose your office location. Add **Run Shortcut** and select **Record office day**. Configure it to run without asking if you want automatic logging; the available controls depend on your iOS version. Apple's [personal automation guide](https://support.apple.com/guide/shortcuts/apd602971e63/ios) explains those settings.

You can also keep the shortcut manual and run it whenever you arrive. Check the dashboard after your first automated visit to confirm that the phone could reach the server.

### If it does not work

- **Cannot connect:** check the address and whether your phone can reach the server on its current network or VPN.
- **Unauthorized:** check that the header contains `Bearer`, a space, and the complete token, and that the token has not been revoked.
- **HTTPS required:** use an HTTPS address. For trusted local HTTP testing only, disable the bearer-token HTTPS requirement in your setup settings and restart the local app or recreate the container with `docker compose up -d`.
- **Wrong day recorded:** check the account's configured timezone.

## Backups and hosting

To download your own account data, open **Profile → Download my data (JSON)**. This includes your full attendance and requirement history, plans, vacations, preferences, and a snapshot of your holiday calendar, with the app and format versions. Passwords and API tokens are excluded. Import is not available yet. See the [export format and future migration contract](docs/personal-data-export.md).


For Docker installations, the data folder contains the database and the keys used to keep browser logins valid. To make a simple, consistent backup, stop the app with `docker compose down`, copy the **entire data folder** somewhere safe, then start it again with `docker compose up -d`. Keep the backup private.

For access outside your local network, put the app behind an HTTPS reverse proxy and keep `OFFICEDAYS_REQUIRE_HTTPS_FOR_BEARER=true`. The proxy must pass the original scheme using `X-Forwarded-Proto`. The supplied Compose setup accepts forwarded headers, so restrict direct access to port `5802` to trusted systems or your proxy.

Other settings, such as browser session lifetime, can be adjusted in [appsettings.Production.json](src/OfficeDays/appsettings.Production.json) for Docker. Keep passwords in `.env`. Once the initial Admin account exists, you can remove its bootstrap password from `.env`; it is ignored on subsequent starts while users exist.

For custom integrations beyond Shortcuts, see the [HTTP request examples](http/README.md).

## License

[MIT](LICENSE)
