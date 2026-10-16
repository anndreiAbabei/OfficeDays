# To Do

Sizes include backend, frontend, and verification work. Small features are suitable junior starter tasks; medium features benefit from guidance; large and very large features need design work and can be split into smaller assignments.

For every feature: follow the existing validation, authentication, CSRF, and UI patterns; scope personal data to the signed-in user; use user-local dates where relevant; provide loading, empty, and error states. Verify changed behaviour, including ownership, date boundaries, and calculations where applicable.

## Small

### [ ] Rename API tokens
Allow users to change a token's display name without replacing its credential.

Requirements:
- Add an owner-only update endpoint and inline edit/save/cancel controls.
- Trim and validate names using the existing required-field and length rules.
- Preserve the credential, creation time, last-used time, and revocation state.
- Verify persistence, invalid input, and rejection of another user's edits.

### [ ] Edit vacation dates
Update an existing vacation instead of deleting and recreating it.

Requirements:
- Add an owner-only update endpoint and an edit form.
- Reuse vacation date validation and preserve the existing overlap behaviour.
- Preserve the vacation ID and creation time.
- Refresh affected calendar, planning, and summary data after saving.

### [ ] Vacation labels
Add an optional short label, such as "Summer holiday", to vacations.

Requirements:
- Add a nullable field through a migration that preserves existing vacations.
- Trim input, treat blank labels as absent, and enforce a documented length limit.
- Support labels in creation, editing, and listing; render them as plain text.
- Keep labels separate from attendance calculations.

### [ ] Upcoming holidays widget
Show the next few bank holidays relevant to the user.

Requirements:
- Query the user's jurisdiction from their local today onward, ordered by date.
- Return a bounded number of holiday names and dates.
- Handle year boundaries, multiple holidays on one date, and no configured upcoming holidays.

### [ ] Attendance history filters
Filter recorded attendance by manual or automatic entry.

Requirements:
- Add an optional API filter using the existing `IsManual` field.
- Offer All, Manual, and Automatic choices; default to All.
- Combine with existing period filtering and preserve ordering.
- Filter the list without changing dashboard totals or reports.

### [ ] Remember report filters
Restore the user's preferred report month range.

Requirements:
- Persist start/end months in user preferences and validate their bounds and order.
- Default to the current local year and retain existing defaults for new users.
- Preserve the distinction between unapplied filters and the displayed report/export.

### [ ] Monthly personal note
Store one private note per user and calendar month.

Requirements:
- Enforce one note per user/month with a database constraint.
- Support reading, saving, editing, and deleting from the dashboard.
- Trim input, enforce a documented length limit, and render plain text.
- Keep notes private and separate from calculations.

### [ ] Light mode and dark mode in Profile
Let users choose the application's appearance.

Requirements:
- Offer Light, Dark, and System options and persist the preference.
- Apply the preference consistently across routes and subsequent visits.
- Keep text, controls, calendar states, and focus indicators readable in every theme.

### [ ] Mobile layout on vacation card
Improve vacation creation and list actions on narrow screens.

Requirements:
- Prevent inputs, labels, and buttons from overflowing.
- Keep controls labelled and usable by touch and keyboard.
- Check long ranges/labels, loading states, and validation errors.

### [ ] Cookies banner / cookie information
Explain the cookies used by the application.

Requirements:
- Inventory actual cookie usage before deciding banner content and behaviour.
- Clearly explain essential authentication and security cookies.
- If optional cookies are introduced, provide the necessary choice before enabling them and allow changes later.
- Preserve applicable preferences without disrupting authentication or CSRF protection.

## Medium

### [ ] Plan all team days this month
Preview and add plans for remaining eligible team days in a selected month.

Requirements:
- Require a configured team day and preview dates before confirmation.
- Skip past dates, weekends, holidays, vacations, recorded attendance, and existing plans.
- Revalidate eligibility on the server and prevent duplicates on repeated requests.
- Report the outcome and refresh planning data.

### [ ] Explain the monthly target
Show how the required office-day count was calculated.

Requirements:
- Show weekdays, holiday/vacation exclusions, eligible days, requirement percentage, and rounding.
- Derive the explanation from the same rules as dashboard and report calculations.
- Avoid double-counting overlapping exclusions.
- Handle historical requirements and zero eligible days.

### [ ] Will my plans meet the target?
Show projected attendance if upcoming eligible plans are completed.

Requirements:
- Combine eligible attendance and upcoming plans without double-counting dates.
- Display projected days and the remaining gap against the selected month's target.
- Distinguish projected attendance from actual attendance and compliance.
- Refresh when relevant attendance, plans, vacations, or requirements change.

### [ ] Preview vacation impact
Show how a proposed vacation changes office requirements before saving.

Requirements:
- Provide a read-only preview using the existing calculation rules.
- Show before/after requirements for every affected month.
- Account for weekends, holidays, overlapping vacations, and historical percentages.
- For edits, replace the original range in the preview rather than counting both.
- Revalidate on save; a preview does not reserve or persist changes.

### [ ] Attendance import with preview
Import attendance from a narrowly defined CSV format.

Requirements:
- Publish a sample with unambiguous dates and define upload limits.
- Preview valid dates, invalid rows, and duplicates before confirmation.
- Define whether invalid rows reject the whole import or may be explicitly skipped.
- Apply existing attendance rules and prevent duplicates on retries.
- Report imported/skipped/failed rows and refresh affected views.

### [ ] Export user data
Download personal data in a documented JSON format.

Requirements:
- Define a versioned schema covering profile/preferences, requirement history, attendance, plans, vacations, and notes when available.
- Export only the current user's data; identify any referenced shared holiday calendar.
- Exclude passwords, password hashes, token credentials/hashes, and other security secrets.
- Include export time and format version, with a download action and useful error feedback.
- Keep restore/import separate from this download-only feature.

### [ ] Create iOS shortcut file and download it
Help users install an iOS Shortcut for recording attendance.

Requirements:
- Verify a supported way to generate or distribute an importable Shortcut before selecting the implementation.
- Configure the app URL and authenticated attendance request.
- Provide token setup instructions without exposing credentials in logs or shared downloads.
- Explain successful, repeated, and failed requests and verify installation on iOS.

### [ ] Accessibility improvements
Split accessibility work into focused, independently reviewable tasks.

Requirements:
- Respect reduced-motion preferences.
- Review contrast and communicate calendar states through more than colour alone.
- Support calendar keyboard navigation and visible focus indicators.
- Provide meaningful labels, error associations, and announcements for dynamic feedback.
- Check core flows with keyboard and screen-reader use.

### [ ] Enable the PWA
Allow installation with an appropriate standalone experience.

Requirements:
- Add the manifest, icons, and launch/display settings.
- Verify installation and standalone navigation on target devices.
- Define asset caching and update behaviour to avoid stale application code after deployment.
- Provide a clear offline state without unintentionally caching private API data.
- Treat offline changes as a separate feature.

## Large

### [ ] Recurring office plans
Generate plans from weekly patterns with effective dates and exceptions.

Requirements:
- Support selected weekdays and a bounded effective date range.
- Define generation/storage behaviour and how pattern edits affect existing plans.
- Respect holidays, vacations, attendance, past dates, and explicit date exceptions.
- Preview generated dates, prevent duplicates, and preserve historical attendance.

### [ ] Reminder system
Notify users about upcoming plans or missing attendance.

Requirements:
- Define delivery channels and opt-in reminder preferences.
- Schedule using user timezones, including daylight-saving transitions.
- Recheck relevance before delivery and retry without duplicate notifications.
- Allow disabling reminders and expose delivery outcomes for troubleshooting.
- Protect delivery credentials.

### [ ] Calendar integration
Expose planned office days to external calendars.

Requirements:
- Start with a date-range calendar download; add subscription feeds in a later phase.
- Use stable event identifiers and correct all-day date boundaries.
- Define how changes and removals appear on subsequent refreshes.
- Protect private feeds with revocable access and exclude unrelated personal data.
- Explain that external clients control subscription refresh timing.

### [ ] Part-time working schedules
Calculate eligibility from each user's working weekdays.

Requirements:
- Store effective-dated schedules and preserve historical behaviour.
- Apply schedules consistently to calculations, planning, vacation impact, and reports.
- Define interactions with holidays, team days, and requirement changes.
- Migrate existing users without changing their current results.
- Verify schedule boundaries, overlapping exclusions, and zero-eligible-day months.

### [ ] Attendance change history
Show when attendance was added, changed, or removed, and by whom.

Requirements:
- Write audit entries in the same transaction as attendance mutations.
- Capture actor, action, timestamp, affected date, and relevant before/after values.
- Define handling of repeated requests and automated/token-based actions.
- Provide an owner-scoped paginated history with filters.
- Define retention and avoid recording credentials or unnecessary personal data.

### [ ] Backup/restore user data
Restore personal data from a supported backup format.

Requirements:
- Distinguish portable personal backups from administrator full-instance backups.
- Version the format and validate compatibility, references, dates, and upload limits.
- Preview affected data and explicitly define merge versus replace behaviour.
- Restore atomically, handle conflicts predictably, and prevent cross-user writes.
- Exclude security credentials from portable backups and document failed-restore recovery.
- Verify round trips for attendance, requirements, plans, and vacations.

## Very large

### [ ] Customizable dashboard cards/widgets
Let each user select which cards and widgets are visible on their dashboard.

Requirements:
- Define a widget catalogue with stable identifiers, names, descriptions, and sensible default visibility.
- Provide a customization screen or mode with preview and save/cancel controls.
- Persist visibility per user on the backend and restore it across devices and sessions.
- Define which dashboard elements are optional; keep customization accessible even when all optional widgets are hidden.
- Include reset-to-default and preserve the current dashboard for users without saved preferences.
- Handle new, retired, and unknown widget identifiers without breaking saved configurations.
- Load data according to visible widgets while accounting for shared requests and dependencies.
- Keep layouts usable on mobile and desktop, including individual widget loading/error states.
- Hiding a widget must not delete data, change calculations, or change authorization.
- Verify persistence, user isolation, empty selections, reset behaviour, and catalogue changes.
- Deliver visibility selection first; scope ordering, drag-and-drop, resizing, and multiple saved layouts as separate follow-up phases.

### [ ] Allow offline changes
Make supported changes without a connection and synchronize later.

Requirements:
- Define supported offline operations and which private data may be stored on the device.
- Queue changes durably with visible pending, synced, failed, and conflicting states.
- Make retries idempotent and define conflict resolution across devices.
- Revalidate permissions and business rules during sync, including plans that have become past dates.
- Handle expired sessions, logout, account switching, and updates without leaking data or silently losing changes.
- Test interrupted sync, partial failures, and reconnects.

### [ ] Teams and shared planning
Allow groups to coordinate planned office attendance.

Requirements:
- Model teams, membership, invitations, and administration roles.
- Define plan visibility and protect private profile/vacation information.
- Provide a shared weekly roster and team settings.
- Define invitation expiry, leaving/removing members, and team deletion behaviour.
- Keep personal requirements and actual attendance independent of shared planning.
- Verify authorization for every operation and revoke access after membership removal.

## Completed

- [x] Plan days to go to the office
- [x] Report page
- [x] Show team day in calendar
- [x] Added "Created by @andrei" — GitHub page

## Suggested junior task order

1. Rename API tokens.
2. Edit vacation dates.
3. Upcoming holidays widget.
4. Plan all team days this month, with guidance on date and eligibility rules.
