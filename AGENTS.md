# AGENTS.md

Guidance for AI coding agents (Claude Code, Copilot, etc.) working on WorldCupPlanner.
Read this before proposing changes. Keep it current: fix any outdated line in the same
commit that makes it outdated.

## Project

WorldCupPlanner manages a 48-team World Cup: 12 groups of 4, group-stage fixture, knockout
bracket (Round of 32 → Final), Elo ranking, interchangeable simulation engines, journalist
notifications, CSV/XLSX export and a global audit log. Draws and simulations are
**deterministic**: every shuffle uses a configurable seed.

- C# / .NET 8, Blazor Server UI.
- EF Core 8 (Code-First) on Azure SQL Edge (Docker). TPH for the `Match` and `MatchIncident` hierarchies.
- MSTest. Unit tests use the EF InMemory provider; flows that need real foreign keys use SQLite in-memory (`DataAccessTest`).

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
| `WorldCupPlanner.{Domain,Services,DataAccess}.Tests` | MSTest suites, one per layer. |

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

The UI applies pending migrations and seeds settings plus default users on startup.

## Business rules

- **Seeds** live in `TournamentSettings` (`FixtureSeed`, `CompletionSeed`, `CrossingsSeed`, `SimulationSeed`) next to `StartDate`, `MaxMatchesPerDay` and `DaysBetweenJourneys`. Every seeded operation is audited.
- **Shuffling** is always Fisher–Yates with `new Random(seed)`. Never use an unseeded `Random` or `DateTime.Now` in Domain or Services.
- **String normalization**: lowercase, strip accents, non-alphanumerics → space, collapse spaces, trim (`TextNormalizer`).
- **Confederation quotas**: UEFA 16, CONMEBOL 7, CONCACAF 7, CAF 9, AFC 8, OFC 1 (48 total). A group may hold up to 2 UEFA teams; other confederations at most 1.
- **Group fixture**: 6 matches per group. Matchday 1: T1–T4, T2–T3. Matchday 2: T1–T3, T2–T4. Matchday 3: T1–T2, T3–T4. Kick-off 14:00; a group's last matchday is played at the same day and time. Stadiums rotate by normalized name: match *n* → `stadium[(n-1) mod count]`.
- **Standings**: points → goal difference → goals for → seeded draw (`CrossingsSeed`).
- **Round of 32**: 12 winners + 12 runners-up + 8 best third places. A1–A8 (winners vs thirds), B1–B4 (remaining winners vs weakest runners-up), B5–B8 (remaining runners-up). Avoid same-group pairings when possible. Then C1–C8 (Ai vs Bi), D1–D4, semi-finals, third place and final.
- **Match editing**: only date, stadium and result. Group matches are locked once the knockout stage is generated; a knockout match is locked once the next round is built from it, and its result cannot be a draw.
- **Roles**: `UserRoles { Admin, Editor, Periodista }`. Passwords are stored as salted PBKDF2-SHA256 (`salt:hash`, 100k iterations) and must have ≥ 8 chars with upper, lower, digit and symbol.

## Code conventions

Follow the neighbouring file. Good references: `Team.cs`, `FixtureOrchestrator.cs`, `TournamentSimulationService.cs`, `AuditActions.cs`.

- Identifiers in English; user-facing strings (Razor pages, exception messages) in Spanish.
- Error messages and magic numbers as named `private const` fields at the top of the class.
- Fail fast: validate intrinsic invariants in domain constructors and mutators; cross-entity rules (existence, uniqueness) belong in Services.
- Domain exceptions (`MatchException`, `SettingsException`, …) are separate from service exceptions (`MatchServicesException`, …).
- Properties are `{ get; }` by default; a private setter only behind a validating mutator (see `Stadium.Update`).
- `User` keeps its lower-case members (`idUser`, `name`, `lastname`, `email`, `password`, `dateOfBirth`); tests depend on them.
- `Match` is abstract and `MatchGroup`/`MatchElimination` use static factories. In tests use the factories or the doubles under `*/TestDoubles/`.
- EF configuration goes in `IEntityTypeConfiguration<T>` classes under `src/WorldCupPlanner.DataAccess/Configurations/`. Filter with `IQueryable` in SQL instead of loading whole tables.
- Audit actions are `SCREAMING_SNAKE` constants in `src/WorldCupPlanner.Services/AuditActions.cs`; log mutations through `AuditLogService` (ISO-8601 timestamp + actor).

## Workflow

- Gitflow: `feature/*` from `develop`, PR to `develop`; `main` only receives releases.
- TDD for all non-UI code: RED → GREEN → REFACTOR, one commit per step with that prefix. Other commits use `feat:`, `fix:`, `docs:`, `chore:`.
- Before a PR: build plus all three test suites green. CI (`.github/workflows/dotnet-tests.yml`) runs them on every push and PR.
- Never edit a committed migration; when adding a column to a seeded table, give it a `defaultValue`.
- Do not add NuGet packages, reformat whole files or change line endings without being asked.

## AI skills

Reusable agent skills live in `.github/skills/` (index in its `README.md`). If your tool does
not load skills, read the `SKILL.md` and apply it by hand.
