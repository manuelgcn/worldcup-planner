# AGENTS.md

Guidance for AI coding agents (Claude Code, Copilot, etc.) working on WorldCupPlanner.
Read this before proposing changes. Keep it current: fix any outdated line in the same
commit that makes it outdated.

## Project

WorldCupPlanner manages a 48-team World Cup: 12 groups of 4, group-stage fixture, knockout
bracket (Round of 32 → Final), Elo ranking with a forecast for unplayed matches, interchangeable
simulation engines, tournament statistics and team profiles, journalist notifications, CSV/XLSX
export, an ICS calendar of the fixture and a global audit log. Draws and simulations are
**deterministic**: every shuffle uses a configurable seed.

- The UI brand is **Golazo** (logo: `src/WorldCupPlanner.UI/wwwroot/img/golazo-mark.svg`); the solution, projects and namespaces keep the WorldCupPlanner name.
- C# / .NET 8, Blazor Server UI.
- EF Core 8 (Code-First) on Azure SQL Edge (Docker). TPH for the `Match` and `MatchIncident` hierarchies.
- MSTest. Unit tests use the EF InMemory provider; flows that need real foreign keys use SQLite in-memory (`tests/WorldCupPlanner.DataAccess.Tests`, `TestDoubles/SqliteDbContextFactory.cs`).

## Solution layout

Single solution at the root: `WorldCupPlanner.sln`. Projects live in `src/`, tests in `tests/`.
Each project's root namespace equals its name.

| Project | Responsibility |
|---|---|
| `WorldCupPlanner.Domain` | Entities, enums, invariants, domain exceptions. Depends on nothing. |
| `WorldCupPlanner.Services` | Use cases, orchestration, DTOs, auditing, simulation engines. |
| `WorldCupPlanner.DataAccess` | `AppDbContext`, repositories, `Configurations/`, `Migrations/`. |
| `WorldCupPlanner.UI` | Blazor Server pages. Presentation only, no business logic. |
| `WorldCupPlanner.DataSeeder` | Console app that resets the DB and seeds a full demo tournament. |
| `WorldCupPlanner.{Domain,Services,DataAccess,UI}.Tests` | MSTest suites, one per layer. The UI suite covers that project's `.cs` classes, not the Razor components. |

Allowed dependencies: `UI → Services → Domain` and `Services → DataAccess → Domain`.

## Commands

```bash
dotnet build
dotnet test                                   # no database needed

# Single test
dotnet test tests/WorldCupPlanner.Domain.Tests \
  --filter "FullyQualifiedName~WorldCupPlanner.Domain.Tests.MatchTests.MatchCanGetMatchId"

# New migration (always with the UI as startup project)
dotnet ef migrations add <Name> \
  --project src/WorldCupPlanner.DataAccess --startup-project src/WorldCupPlanner.UI
```

The UI applies pending migrations and seeds settings and, unless `Demo:Enabled` is false, the
three demo users on startup. `docker compose --profile app up` runs it in a container on
`127.0.0.1:8080`, and `GET /health` reports whether the database answers (`DatabaseHealthCheck`).

## Business rules

- **Seeds** live in `TournamentSettings` (`FixtureSeed`, `CompletionSeed`, `CrossingsSeed`, `SimulationSeed`) next to `StartDate`, `MaxMatchesPerDay` and `DaysBetweenJourneys`. Every seeded operation is audited.
- **Shuffling** is always Fisher–Yates with `new Random(seed)`. Never use an unseeded `Random` or `DateTime.Now` in Domain or Services.
- **String normalization**: lowercase, strip accents, non-alphanumerics → space, collapse spaces, trim (`TextNormalizer`).
- **Confederation quotas**: UEFA 16, CONMEBOL 7, CONCACAF 7, CAF 9, AFC 8, OFC 1 (48 total). A group may hold up to 2 UEFA teams; other confederations at most 1.
- **Group fixture**: 6 matches per group. Matchday 1: T1–T4, T2–T3. Matchday 2: T1–T3, T2–T4. Matchday 3: T1–T2, T3–T4. Kick-off 14:00; a group's last matchday is played at the same day and time. Stadiums rotate by normalized name: match *n* → `stadium[(n-1) mod count]`.
- **Schedule settings**: `MaxMatchesPerDay` (1–3) is kick-off slots per stadium and day in the group stage (`GroupFixtureGenerator`) and matches per day in total in the knockout stage (`KnockoutRoundScheduler`). `DaysBetweenJourneys` is a minimum between a group's matchdays and the exact gap between knockout rounds, counted from the previous round's last match (the last group match for the round of 32); the final is the day after the third-place match.
- **Standings**: points → goal difference → goals for → seeded draw (`CrossingsSeed`).
- **Round of 32**: 12 winners + 12 runners-up + 8 best third places. A1–A8 (winners vs thirds), B1–B4 (remaining winners vs weakest runners-up), B5–B8 (remaining runners-up). Avoid same-group pairings when possible. Then C1–C8 (Ai vs Bi), D1–D4, semi-finals, third place and final.
- **Match editing**: only date, stadium and result. Group matches are locked once the knockout stage is generated; a knockout match is locked once the next round is built from it, and its result cannot be a draw.
- **Roles**: `UserRoles { Admin, Editor, Periodista }`. Passwords are stored as salted PBKDF2-SHA256 (`v2:<iterations>:<salt>:<hash>`, 600k iterations; the older `salt:hash` with 100k iterations and the unsalted SHA-256 are still accepted, and any hash weaker than the current one is replaced on the next successful login) and must have 8 to 128 chars with upper, lower, digit and symbol. A login with an unknown email still verifies a password (`User.PasswordMatchesNoAccount`), so it takes as long as a wrong password on an account with a current hash. Five failed logins within five minutes lock an email for five minutes (`UserServices`, counted in memory). A user changes their own password with `UserServices.ChangePassword`: it needs the current password, and a wrong one counts as a failed login. User id 1 is the system admin: `UserServices` refuses to edit or delete it or to change its roles or password.
- **Session**: `SessionState` keeps the user id, email, a session id, a digest of the stored password hash and an 8-hour expiry in `ProtectedSessionStorage`; `Routes` restores it before any page renders (prerender is off) and `MainLayout` revalidates it on every navigation, logging out when it expired, was revoked by a logout or a new login (`RevokedSessions`, in memory), the user is gone, or that id now has another email or another password (so a password change ends the account's other sessions). After editing the signed-in user or changing their password call `RefreshUserAsync`, not `LoginAsync`, which opens a new 8-hour window. Every handler that changes or exports data starts with `Session.StillHasAnyRoleAsync` and the roles its button requires (`RevalidateAsync` in the profile page): it reads the user again and is false when that fails.
- **Demo accounts** are created at startup only when `Demo:Enabled` is not false and they are missing; existing accounts are never modified. With the setting off `/login` hides their credentials and the app refuses to start unless some user is an Admin (`DemoAccounts`).
- **Import**: the team CSV must be UTF-8 with a header row, up to 256 KB (`CsvStreamTeamParser`, `Import.razor`); a flag upload must be a PNG, JPEG, WebP or SVG by content, up to 200 KB (`ImageHelper.IsSupportedImage`).

## Code conventions

Follow the neighbouring file. Good references: `Team.cs`, `FixtureOrchestrator.cs`, `TournamentSimulationService.cs`, `AuditActions.cs`.

- Identifiers in English; user-facing strings (Razor pages, exception messages) in Spanish.
- Error messages and magic numbers as named `private const` fields at the top of the class.
- Fail fast: validate intrinsic invariants in domain constructors and mutators; cross-entity rules (existence, uniqueness) belong in Services.
- Domain exceptions (`MatchException`, `SettingsException`, …) are separate from service exceptions (`MatchServicesException`, …).
- Properties are `{ get; }` by default; a private setter only behind a validating mutator (see `Stadium.Update`).
- `User` keeps its lower-case members (`idUser`, `name`, `lastname`, `email`, `password`, `dateOfBirth`); tests depend on them.
- `Match` is abstract and `MatchGroup`/`MatchElimination` use static factories. In tests use the factories or the doubles under `*/TestDoubles/`.
- UI styles: design tokens (`--m26-*`) and classes shared by several pages (`.m26-card`, `.pane`, `.admin-grid`, `.auth-*`, buttons) live in `src/WorldCupPlanner.UI/wwwroot/app.css`; a page's `.razor.css` only holds what that page alone uses, since scoped CSS never reaches other components. The day and night themes both read those tokens (night values under `html[data-theme="night"]`): new styles use a token, not a literal colour.
- UI security: the Content-Security-Policy in `src/WorldCupPlanner.UI/SecurityHeaders.cs` (pinned by `SecurityHeadersTests`) only lets scripts load from the app, so no inline `<script>` and no HTML `on*=` attributes (Blazor `@on…` handlers are fine): page scripts are files under `wwwroot/` loaded from `App.razor`. A new external host goes in the policy and in its test.
- Shared UI components: `PasswordField` for password inputs, `ScoreCell` for a match score and `FeedbackAlerts` for the errors and the success message of an action; an unexpected exception is shown through `UserFacingError.MessageFor`.
- EF configuration goes in `IEntityTypeConfiguration<T>` classes under `src/WorldCupPlanner.DataAccess/Configurations/`. Filter with `IQueryable` in SQL instead of loading whole tables.
- Audit actions are `SCREAMING_SNAKE` constants in `src/WorldCupPlanner.Services/AuditActions.cs`; log mutations through `AuditLogService` (ISO-8601 timestamp + actor: pass the actor's email to `LogPerformedBy`, and the row also stores the user id it resolves to).

## Workflow

- Gitflow: `feature/*` from `develop`, PR to `develop`; `main` only receives releases.
- TDD for all non-UI code: RED → GREEN → REFACTOR, one commit per step with that prefix. Other commits use `feat:`, `fix:`, `docs:`, `chore:`.
- Before a PR: build plus all four test suites green. CI (`.github/workflows/dotnet-tests.yml`) runs them on pushes and pull requests to `main` and `develop`. Coverage (`.runsettings`) leaves out migrations, `*.razor` and `Program.cs`, so UI logic that deserves a test goes in a `.cs` class.
- Never edit a committed migration; when adding a column to a seeded table, give it a `defaultValue`.
- Do not add NuGet packages, reformat whole files or change line endings without being asked.

## AI skills

Reusable agent skills live in `.github/skills/` (index in its `README.md`). If your tool does
not load skills, read the `SKILL.md` and apply it by hand.
