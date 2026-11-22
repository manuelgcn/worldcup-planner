# TODO

Work found by the backend, security, accessibility and UX audits and the PR #8 code review of November 2026 that is not done yet.
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
- **Password hashes have no version or iteration count** (`salt:hash`, 100k PBKDF2 iterations, below the current OWASP 600k). Store `v2:iterations:salt:hash` and rehash on login when the count is lower, reusing the legacy-upgrade path in `UserServices`.
- **The audit actor is an email the user can change.** Store the user id as well.
- **The login lockout is in memory.** It resets when the app restarts and is per server; acceptable for one instance. It tracks at most 10,000 emails (`UserServices.MaxTrackedLogins`): when all of them are still live it drops the one closest to expiry, so a flood of new emails can reset a real counter; rate-limit logins per IP if that matters. Wrong passwords sent in parallel are all checked before any of them is counted, so N simultaneous requests get N guesses. Fix: count the attempt before checking the password and forget it on success.
- **The 8-hour session expiry is only checked on restore** (`SessionState.RestoreAsync`). A tab whose circuit stays open never expires, and `LoginAsync`, which `EditUser.razor` and `Users.razor` also call after an edit, restarts the window. Fix: keep the expiry in memory, check it in `RevalidateAsync`, and add a refresh that keeps it.
- **`RevalidateAsync` only catches `UserServicesException`.** A brief database error on any navigation now breaks the circuit. Fix: log it and keep the current user.
- **`UserFacingError` shows every `ArgumentException` message**, so English framework messages and the "(Parameter 'name')" suffix from the domain reach users. Fix: show the message only when it was thrown from `WorldCupPlanner` code, or move the domain to its own exceptions.

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
