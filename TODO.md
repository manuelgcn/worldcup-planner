# TODO

Work found by the backend, security, accessibility and UX audits, the PR #8 code review and the authorization and input-handling audits of November 2026 that is not done yet.
Each item says where it is and what the fix looks like. Remove an item in the same commit that fixes it.

## Correctness

- **Multi-write use cases are not atomic.** Every repository call opens its own `DbContext` and saves, and no use case runs in a transaction. A failure halfway can apply Elo twice (`RankingService` saves the teams before `MatchService` saves the match), leave the bracket without its third-place match (`KnockoutFixtureGenerator.AdvanceToNextRound`), leave a partial round of 32 (`GenerateBracket`), or revert Elo twice (`FixtureOrchestrator.DeleteGroupStage`). Fix: wrap each use case in a `TransactionScope`, or save the match and both teams in one `SaveChanges`. Needs SQL Server to prove.
- **Public `MatchService` methods bypass business rules.** `AddGoal`, `RemoveGoal`, `PublishResult`, `SimulateMatch` and `SetGoals` are only called from tests: they skip the knockout-winner check, apply Elo to unplayed matches or skip Elo and notifications. Fix: delete them (and their tests), or route them through `ReapplyMatchResult` and `EnsureKnockoutResultHasWinner`.
- **`MaxMatchesPerDay` means two things.** The group scheduler reads it as kick-off slots per stadium, the knockout scheduler as matches per day in total. Pick one and document it.
- **`DaysBetweenJourneys` is now a minimum.** When one matchday does not fit in that many days, matchdays are spaced further apart. Say so wherever the setting is described.
- **Group scheduler divides by zero with a single stadium.** Unreachable today because `FixtureOrchestrator` requires four stadiums, but `GroupFixtureGenerator` should reject it itself.
- **Confederation cap only lives in the draw.** `Group.AddTeam` accepts a third UEFA team. Moving the rule into `Group` is right, but about 20 tests build groups with same-confederation fixtures and need new data first.
- **Knockout rounds ignore `DaysBetweenJourneys`.** `KnockoutFixtureGenerator` starts the round of 32 the day after the last group match and each round the day after the previous one. With the demo settings, a team from groups G–L can play on two consecutive days, and the third-place match is the day after the semi-finals. Fix: use `Math.Max(1, settings.DaysBetweenJourneys)` as the gap, keep the final after the third-place match, and assert the gap in `KnockoutRounds_EachRoundStartsAfterThePreviousRoundEnds`.
- **`MatchService.UpdateMatch` drops the actor** when the request carries a `TournamentSettingsId`: it logs through `LogAuditAction`, which has no email. The UI never sends one today. Fix: an `IAuditLogService` overload that takes both.
- **Old notifications keep the old text.** Rows created before "Resultado simulado: …" still read "Se simulo resultado del partido 2031: …". A data migration or simply re-seeding fixes the demo.

## Security

- **Journalists can read the whole audit log**, including every user's email (`AuditLog.razor`, `ExportService`). Public signup creates journalists, so anyone can collect the emails. Decide against the course spec: limit the log to Admin, or keep it and defend it.
- **Demo credentials are shown on `/login` and there is no way to change a password.** Fine for the localhost demo; put both behind a `Demo:Enabled` setting and add a change-password use case before any real deployment.
- **The 128-character password limit only covers new passwords** (`User.ValidatePassword`). `Authenticate` still hashes whatever is typed at login, bounded only by Blazor's default 32 KB message size. Rejecting longer input at login would lock out an account created earlier with a longer password, and there is no change-password use case to recover it.
- **Login response time still tells some accounts apart.** An unknown email now costs one 600k PBKDF2 check, but an account whose hash is still the old `salt:hash` (100k) or the unsalted SHA-256 answers a wrong password faster until its owner logs in once; that includes the six accounts of `db/scripts/seed-full.sql`. Only the owner's login can rehash them. The check also makes every unknown-email attempt cost CPU: rate-limit logins per IP if that matters.
- **The audit actor is an email the user can change.** Store the user id as well.
- **The login lockout is in memory.** It resets when the app restarts and is per server; acceptable for one instance. It tracks at most 10,000 emails (`UserServices.MaxTrackedLogins`): when all of them are still live it drops the one closest to expiry, so a flood of new emails can reset a real counter or lift an active lock; rate-limit logins per IP if that matters. Wrong passwords sent in parallel are all checked before any of them is counted, so N simultaneous requests get N guesses. Fix: count the attempt before checking the password and forget it on success. The counter is keyed by the account's stored email, so two spellings that lock together reveal that the email is registered; public signup already reveals it ("El email ya esta registrado"), fix both together if it matters.
- **The 8-hour session expiry is only checked on restore** (`SessionState.RestoreAsync`). A tab whose circuit stays open never expires, and `LoginAsync`, which `EditUser.razor` and `Users.razor` also call after an edit, restarts the window. Fix: keep the expiry in memory, check it in `RevalidateAsync`, and add a refresh that keeps it.
- **`RevalidateAsync` only catches `UserServicesException`.** A brief database error on any navigation now breaks the circuit. Fix: log it and keep the current user.
- **`UserFacingError` shows every `ArgumentException` message**, so English framework messages and the "(Parameter 'name')" suffix from the domain reach users. Fix: show the message only when it was thrown from `WorldCupPlanner` code, or move the domain to its own exceptions.
- **Mutating handlers act on the roles the page was rendered with.** Only navigation (`MainLayout`) and `Users.razor` call `Session.RevalidateAsync`, and the services check no role. A user whose role was removed, or who was deleted, in another tab can still act from a page left open: `Fixture.razor` `RunAdminActionAsync`, `Simulator.razor` `RunSimulationAsync`, `MatchResultEditor.razor` `SaveAsync`/`AddIncident`/`RemoveIncident`, `Teams.razor` `Create`/`AssignFlags`/`Remove`, `Stadium.razor` `Create`/`Remove`, `Import.razor` `OnFileSelected`, and the loads and exports of `AuditLog.razor` and `Fixture.razor` `HandleExportFixtureAsync`. Fix: one `SessionState.StillHasAnyRoleAsync(params UserRoles[])` that revalidates and checks, called first in each handler (do not reuse `IsAdmin` there: it dereferences `CurrentUser!`). Fix the `RevalidateAsync` item above in the same change, because every click will then read the user.
- **`UserServices.AssignRole` has no system-admin guard**, unlike `RemoveRole`, `UpdateUser`, `RemoveUser` and `ToggleRole`. In `Users.razor` the rules "not the system admin" and "not your own Admin role" are only `disabled` attributes, which the browser's dev tools can remove. Fix: the same guard in `AssignRole` with a test, and an early return in `Users.razor` `ToggleRole` for one's own Admin role.
- **Logging out does not end the user's other tabs.** `StoredSession` has no session id, so a duplicated tab (the browser copies `sessionStorage`) stays signed in after "Cerrar sesión". Fix: a session id in `StoredSession` and an in-memory set of revoked ids checked in `RestoreAsync` and `RevalidateAsync`.
- **Notification audit rows have no actor.** `NotificationService.MarkAsRead` and `MarkAllAsRead` log through `Log`. Fix: take `performedByEmail` and use `LogPerformedBy`.
- **`Fixture.razor` shows raw exception messages** (`ReloadState`, and the `InvalidOperationException` catch in `RunAdminActionAsync`) instead of going through `UserFacingError`: a database outage shows the SQL client text to any signed-in user.
- **The `sa` connection string is in the base `appsettings.json`**, so a run outside Development without `ConnectionStrings__Default` connects as sysadmin with the public password instead of failing. Fix: move it to `appsettings.Development.json` and say so in the README.
- **`DataSeeder` `Mask` only hides `Password=`.** `Pwd=`, `Password =` or a quoted password containing `;` are printed. Fix: print only `DataSource` and `InitialCatalog` from a `SqlConnectionStringBuilder`.
- **The audit-log export has no row limit** (`ExportService.ExportAuditLog`), and any journalist can request it. Fix: cap the rows and ask for a narrower date range.

## Import and validation

- **The CSV import decodes invalid UTF-8 silently** (`CsvStreamTeamParser`): a Windows-1252 file saved from Excel stores "M?xico" and reports success. Fix: a strict `UTF8Encoding(false, throwOnInvalidBytes: true)` and an `ImportServiceException` ("El archivo debe estar guardado en UTF-8.") on `DecoderFallbackException`.
- **The CSV parser is `line.Split(',')`** and always skips the first line: no quoted fields, no header check, no `;` separator. A file without a header loses its first team silently. Fix: CsvHelper's `CsvParser` (already referenced) with `DetectDelimiter`, and reject a file whose first row is data.
- **`ImportService` accepts a number as confederation** (`Enum.TryParse` reads "3" as CONMEBOL). Fix: accept names only.
- **The import cap is 5 MB** for a file that needs under 5 KB (`Import.razor`); a file of commas allocates millions of DTOs before the first row is validated. Fix: 256 KB.
- **A flag upload accepts any file** (`Teams.razor` `OnFlagSelected`), and there is no screen to change a flag afterwards. Fix: check the image signature before storing it. `ImageHelper.GuessImageMimeType` also decodes the whole flag on every render to read 12 bytes: decode a 256-character prefix.
- **Names accept control and invisible characters and punctuation-only values**, and the admin forms store them untrimmed (`Team`, `Stadium`, `User`; `Teams.razor`, `Stadium.razor`, `Users.razor`, `EditUser.razor`). Fix: reject `char.IsControl` and names that normalize to empty, and trim at the call sites as `Signup.razor` does.
- **`Teams.razor` and `Stadium.razor` compute the id as max + 1.** Two simultaneous creates collide and the `DbUpdateException` breaks the circuit. Fix: catch `Exception` and show it through `UserFacingError`, as `Users.razor` does.

## Performance

- `Fixture.razor` `ReloadState` still reads the full match graph twice more through `IsGroupStageComplete` and `PendingGroupStageMatchCount`.
- Whole-table reads that should be SQL queries: `KnockoutFixtureGenerator.NextMatchId` and `LastGroupMatchDate`, `MatchService.GetLockedKnockoutMatchIds`, the `GetAll().Count` calls in `TournamentDashboardService` and `FixtureViewService`, and `NotificationService` loading every user to find journalists.
- Simulating the group stage costs about nine round trips per match (same root cause as the atomicity item).

## Cleanup

- Dead code: `GroupServices`/`IGroupService` and `JourneyServices`/`IJourneyService` are registered but never injected; `TeamServices.OrderForFixture`, `AuditLogService.GetAll`/`ExportAsText`, `AuditActions.SimulateMatch` and `AuditActions.UpdateTeam` are unused.
- Magic numbers against the `private const` convention: `Stadium.cs` (80, 60, 20000), `FixtureService.cs:46`, and the two different id offsets in `EliminationBuilder.cs` (2000) and `KnockoutFixtureGenerator.cs` (3000).

## UI and accessibility

- Focus is not moved after most actions (fixture steps, simulator, confirmations, user edit); only the group detail does it. Send focus to the result or the next control.
- Untested in the browser because the demo tournament is complete: the 0-0 confirmation checkbox, the "Regenerar fixture" confirmation, the simulator's "no fixture yet" notice, the role-based hints and the knockout dates with non-default settings. Run them on a fresh database.
- The 8-hour session expiry has not been exercised end to end.
- Exporting the audit log when the screen hit its 500-row cap and "Desde" is empty names the file `audit-log_00010101_…`. Use the date of the oldest exported row instead.
