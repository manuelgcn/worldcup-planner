# TODO

Work found by the backend, security, accessibility and UX audits, the PR #8 code review, the authorization and input-handling audits of November 2026 and the cleanup, security, feature and visual work merged after them that is not done yet.
Each item says where it is and what the fix looks like. Remove an item in the same commit that fixes it. Choices that are the owner's to make are at the end.

## Correctness

- **Multi-write use cases are not atomic.** Every repository call opens its own `DbContext` and saves, and no use case runs in a transaction. A failure halfway can apply Elo twice (`RankingService` saves the teams before `MatchService` saves the match), leave the bracket without its third-place match (`KnockoutFixtureGenerator.AdvanceToNextRound`), leave a partial round of 32 (`GenerateBracket`), or revert Elo twice (`FixtureOrchestrator.DeleteGroupStage`). Fix: wrap each use case in a `TransactionScope`, or save the match and both teams in one `SaveChanges`. Needs SQL Server to prove.
- **Confederation cap only lives in the draw.** `Group.AddTeam` accepts a third UEFA team. Moving the rule into `Group` is right, but about 20 tests build groups with same-confederation fixtures and need new data first.
- **Old notifications keep the old text.** Rows created before "Resultado simulado: …" still read "Se simulo resultado del partido 2031: …". A data migration or simply re-seeding fixes the demo.

## Security

- **Journalists can read the whole audit log**, including every user's email and, in the export, user id (`AuditLog.razor`, `ExportService`). Public signup creates journalists, so anyone can collect the emails. Decide against the course spec: limit the log to Admin, or keep it and defend it.
- **The 128-character password limit only covers new passwords** (`User.ValidatePassword`). `Authenticate` and the current password in `ChangePassword` still hash whatever is typed, bounded only by Blazor's default 32 KB message size. Rejecting longer input at login would lock out an account created earlier with a longer password until its owner changes it from "Mi perfil".
- **Login response time still tells some accounts apart.** An unknown email now costs one 600k PBKDF2 check, but an account whose hash is still the old `salt:hash` (100k) or the unsalted SHA-256 answers a wrong password faster until its owner logs in once; that includes the six accounts of `db/scripts/seed-full.sql`. Only the owner's login can rehash them. The check also makes every unknown-email attempt cost CPU: rate-limit logins per IP if that matters.
- **The login lockout is in memory.** It resets when the app restarts and is per server; acceptable for one instance. It tracks at most 10,000 emails (`UserServices.MaxTrackedLogins`): when all of them are still live it drops the one closest to expiry, so a flood of new emails can reset a real counter or lift an active lock; rate-limit logins per IP if that matters. Wrong passwords sent in parallel are all checked before any of them is counted, so N simultaneous requests get N guesses. Fix: count the attempt before checking the password and forget it on success. The counter is keyed by the account's stored email, so two spellings that lock together reveal that the email is registered; public signup already reveals it ("El email ya esta registrado"), fix both together if it matters.
- **Revoked sessions are kept in memory** (`RevokedSessions`), like the lockout. A restart or a second server forgets them, so a copied session of a logged-out user works again until it expires if the Data Protection keys survive. Fix: store the revoked ids in the database when more than one instance runs.
- **`SessionState.RestoreAsync` lets a database error escape** when a circuit starts (`Routes.razor`), which breaks it. Fix: log it and start logged out without deleting the stored session.
- **`UserFacingError` tells domain errors from framework ones by `Exception.TargetSite`.** The domain still validates with `ArgumentException`, so a message is shown only when WorldCupPlanner code threw it (`WasThrownByOwnCode`): a framework method inlined into that code would pass, and the Spanish `InvalidOperationException` messages of `GroupQualifier` and `ProbabilisticSimulationEngine` get the generic text. Fix: move the domain to its own exceptions and drop the special case.
- **Some handlers bypass `UserFacingError`.** `Fixture.razor` `RunAdminActionAsync`, `MatchResultEditor.razor` `SaveAsync`, `Matches.razor` `ApplyFilters`/`LoadStadiums` and `Stadium.razor` `Remove` still show a raw `ArgumentException` message, and a database error breaks the circuit in `Remove` of `Teams.razor` and `Stadium.razor`, `AddIncident`/`RemoveIncident` of `MatchResultEditor.razor`, `Matches.razor` and the query and export of `AuditLog.razor`. Fix: catch `Exception` and show `UserFacingError.MessageFor`, as `Users.razor` does.
- **The `sa` connection string is in the base `appsettings.json`**, so a run outside Development without `ConnectionStrings__Default` connects as sysadmin with the public password instead of failing; the container image carries the file too (`Dockerfile`). The release package relies on it: `scripts/release/start.ps1` starts the exe with only `--urls`. Fix: have `start.ps1` set `ConnectionStrings__Default` from the `.env` password it already reads, then move the block to `appsettings.Development.json` and say so in both READMEs.
- **The Content-Security-Policy still allows inline styles and three third-party hosts** (`SecurityHeaders.cs`): `style-src 'unsafe-inline'` for the `style` attributes in `Home.razor`, `Stats.razor` and `Teams.razor` and for Blazor's reconnect UI, plus `fonts.googleapis.com`, `fonts.gstatic.com` and `cdn.jsdelivr.net` for Archivo and Bootstrap Icons. Fix: move those styles to classes and self-host the font and the icons under `wwwroot` (`font-src` then needs `'self'`). The policy, `connect-src 'self'` for the Blazor WebSocket included, was only checked in a Chromium browser.

## Import and validation

- **The CSV header is recognised by shape** (`CsvStreamTeamParser.EnsureIsHeader`): a title with a digit in the third column ("Ranking 2026") is rejected as a team, and a headerless first row with a misspelt confederation and no digit in the ranking is still skipped. The help text of `Import.razor` does not say that the file must be UTF-8 or that `;` works as separator. Fix: require known header titles and complete the help text.
- **A team's flag cannot be changed after it is created**: there is no screen for it (`Teams.razor`). The upload check (`ImageHelper.IsSupportedImage`) also accepts any XML file that contains `<svg` and rejects an SVG that starts with a comment or a DOCTYPE. Fix: an edit form, and parsing the XML root if flags are ever shown outside `<img>`.
- **Names accept invisible format characters.** `Team`, `Stadium` and `User` reject control characters, but `char.IsControl` is false for the Unicode Format category, so a zero-width space inside "Uruguay" gives a second team that looks the same; the stadium description is neither trimmed nor checked for control characters. Fix: also reject `UnicodeCategory.Format`, unless the zero-width joiners of Persian or Indic names must stay valid.
- **`Teams.razor` and `StadiumServices.NextStadiumId` compute the id as max + 1.** Two simultaneous creates collide; the second now gets the generic error instead of breaking the circuit. Fix: let the database generate the id.

## Performance

- `Fixture.razor` `ReloadState` still reads the stadium list for a count, the elimination graph twice (`KnockoutFixtureGenerator.CanAdvanceToNextRound` and `IsBracketComplete`) and the match graph again for the standings (`GetStandingsForAllGroups`), although the snapshot it has just loaded already holds the groups and both sets of matches. Fix: compute them from the snapshot and add a stadium count to it (`IStadiumRepository.Count()` exists).
- Whole-table reads that should be SQL queries: `KnockoutFixtureGenerator.EnsureGroupStageComplete`, `GroupStandingsService.IsGroupStageComplete` (called by `Simulator.razor`) and `TournamentSimulationService.EnsureGroupStageNotAlreadyPlayed` load every match to check the group ones, and `TeamProfileService.GetProfile` loads every match and filters by team in memory.
- Simulating the group stage costs about nine round trips per match (same root cause as the atomicity item).

## Cleanup

- Dead code: `INotificationService.NotifyMatchSimulated`, `IGroupStandingsService.PendingGroupStageMatchCount`, `Match.AddHomeGoal`/`AddAwayGoal`/`RemoveHomeGoal`/`RemoveAwayGoal` and `IAuditLogRepository.GetAll` have no caller in `src`; neither have `MatchService.CreateMatch` and `DeleteMatch`, which also log without an actor. Fix: delete them with their tests and test doubles.
- Duplicated: the Spanish phase labels (`Matches.razor` `PhaseLabel`, `TeamProfile.razor`, `FixtureCalendarWriter`), the figure tally styles (`Home.razor.css`, `Stats.razor.css`, `TeamProfile.razor.css`) and the password rules (`PasswordField.razor` repeats `User.ValidatePassword`). Fix: one mapping in Services, one class in `app.css`, and rules exposed by the Domain.
- Shortcuts kept from the merge: `AuditLogService.CreateInstance` takes `IUserRepository` as optional (a service built without it records no user id), and the narrow repository queries (`Count`, `CountPlayed`, `LatestGroupMatchDate`, `MaxMatchId`, `GetSourceMatchIds`, `GetByRole`) are default interface members that fall back to `GetAll()`, so a renamed override silently loads the table again. Fix: make both required and update the test fakes.
- Missing tests: the healthy branch of `DatabaseHealthCheck` (the UI test project references no SQLite or InMemory provider), the Razor components (`ScoreCell`, `GroupStandingsTable`, `PasswordField`, `FeedbackAlerts`; bUnit would be a new package) and the scripts under `wwwroot`.

## UI and accessibility

- Focus moves to the feedback block only where `FeedbackAlerts` is used (`Fixture`, `Simulator`, `Teams`, `Stadium`, `Users` and the change-password card). Still without it: `LogIn`, `Signup`, the profile form of `EditUser`, `Matches`, `Import`, `AuditLog`, `Home` and `MatchResultEditor`.
- Untested in the browser: the 0-0 confirmation checkbox, the "Regenerar fixture" confirmation, the simulator's "no fixture yet" notice, the role-based hints, the knockout dates with non-default settings, the knockout rows of the team profile, a session ending in an open or a duplicated tab (covered with a manual clock in `SessionStateTests`), the "No se pudo confirmar que tengas permiso…" message, the `.ics` file in a calendar client, `prefers-reduced-motion`, and motion and print outside Chromium. Run them on a fresh database.
- The fixture export keeps a stale "Hasta" date: `Fixture.razor` `ReloadState` sets `exportToDate ??=` once, so a page opened before the fixture existed exports only the start date until it is reloaded. Fix: recompute the default range when the fixture is generated or deleted.
- Keyboard and contrast gaps: the reconnect notice does not make the page behind it inert (`App.razor`, `reconnect.js`); the yellow bar of the current stage on the home page has 1.5:1 against the day background (`Home.razor.css`, `.stage.current::before`; the bold label carries the state); the forecast figures on `/matches` are only in the `title` and the hidden text (`MatchForecast.razor`), so touch users see the bar without numbers.

## Operations

- **Not run yet:** `docker compose --profile app up` (the image was built and run by hand: non-root user, `/health`, HEALTHCHECK), Dependabot and CodeQL on GitHub (their files were only parsed), and the `.runsettings` coverage exclusions on the Linux CI. Check the first run of each.
- **The `app` container is only fit for loopback.** It serves plain HTTP on 8080 (no HSTS or HTTPS redirection without an HTTPS port), shows UTC in the audit log and the notifications (`ToLocalTime()` with no `TZ`), and keeps its Data Protection keys inside the container, so recreating it ends every session. Fix: a TLS-terminating proxy, a `TZ` variable on the `app` service, and a volume with `PersistKeysToFileSystem`.
- **`/health` is anonymous and unthrottled.** With the database down it waits for the SQL connect timeout (15 s by default) while the Docker HEALTHCHECK gives up at 10 s. Fix: a short `Connect Timeout` for the check, or cache the result for a few seconds.

## Decisions for the owner

What the code does today, and the alternative.

- **Demo accounts and first admin**
  - The system admin (user id 1) cannot change its password (`UserServices.ChangePassword`), although on an empty database that account is `admin@copa.com` with a published password. Keep the guard, or let id 1 change it.
  - The other demo accounts can change their password and email from "Mi perfil", after which the credentials on `/login` and in the README stop working. Accept it for the local demo, or block the demo emails.
  - With `Demo:Enabled` false the app refuses to start without an Admin and only warns about leftover demo accounts (`DemoAccounts.VerifyDisabled`); the first Admin can only come from one start with the setting on or from the DataSeeder, both with published passwords. Alternatives: a first admin read from configuration, removing the leftover accounts, `false` as the default outside Development.
- **Audit log and journalists** (whether journalists keep reading the log is the Security item above)
  - The export refuses more than 10,000 rows (`ExportService.MaxAuditLogRows`), for Admins too. Confirm the number, or exempt Admins.
  - Rows written before the `AddAuditLogPerformedByUserId` migration, `db/backups/seed-full.bak` included, have no user id. Backfill them by current email (wrong for anyone who changed it since), or leave them.
  - A user's own email change is logged under the new email and the id, without the old email. Add "old → new" to the details (one more email in a log journalists read), or leave it.
- **Knockout calendar**
  - `MaxMatchesPerDay` is kick-off slots per stadium in the group stage and matches per day in total in the knockout stage. Keep both meanings, or make `KnockoutRoundScheduler` use slots per stadium, which shortens its calendar.
  - The final is the day after the third-place match (`DaysFromThirdPlaceToFinal`); the alternative is a full `DaysBetweenJourneys` gap. With the demo settings (gap 3, 3 matches a day) it is now 24 days after the last group match, 14 when rounds were one day apart: confirm, or lower the demo gap.
- **Import strictness**
  - The import accepts UTF-8 only (a UTF-16 file with a BOM used to import), and a stray quote inside an unquoted field rejects the whole file with its row number instead of being read literally.
  - `Team` and `Stadium` rows are no longer validated when EF loads them, so a row saved under older rules or edited by hand shows up instead of failing every page that lists it. The alternative is to fail on load.
- **Statistics, team profile, forecast and calendar file**
  - A match decided on penalties counts as a draw in `/stats` and in the profile's record (FIFA's convention), not as a win for the shoot-out winner. `/stats` also ranks defences by total goals against, not per match, and shows the top five.
  - The profile's record includes knockout matches, so its points differ from `/ranking` and the group table (the page says so); the alternative is the group stage only. It labels the Elo value "Ranking FIFA", as `/ranking` does.
  - The forecast gives a draw 26 % between equal rankings (`EloRankingCalculator.MaxDrawShare`), shrinking with the gap: teams far apart read 98 / 1 / 1.
  - The `.ics` times are floating (no time zone), a regenerated fixture reuses the event UIDs (`match-<id>@worldcupplanner`), and `ExportService` reads `TimeProvider.System` for DTSTAMP instead of an injected clock.
- **Visual choices**
  - Copy: the loader says "Entrando en calor…", the theme toggle shows the current theme, and a rejected reconnect waits on a "Recargar" button instead of reloading by itself. The domain and older service messages have no accents ("La contrasena debe tener al menos 8 caracteres"), newer ones do: pick one.
  - The password checklist never ticks "Una mayúscula" or "Una minúscula" for Ñ or accented letters, because `User.ValidatePassword` only counts A–Z and a–z. Reword the rules, or widen the domain rule.
  - Bracket: 32 tab stops (every round-of-32 team row) or one with arrow keys; the third-place match under the final or in its own column.
  - The standings highlight lights every row whose position changed, up to all four of a group after one result; the alternative is only rows that crossed a qualification line.
- **Repository and CI settings**
  - `SECURITY.md` names GitHub private vulnerability reporting as the only channel, and it was disabled on the repository when last checked (November 2026). Enable it, or give another contact.
  - Dependabot opens its pull requests against `main` because `develop` does not exist on the remote, and its Docker entry ignores major updates. Push `develop` and set `target-branch`, or accept them on `main`.
  - The CodeQL workflow needs a public repository or GitHub Advanced Security, and code scanning "default setup" turned off.
