# Copilot Instructions for `SolutionCopa`

## Build, test, and lint

Run from repository root (`255503---330734---326829`):

```bash
dotnet restore project/SolutionCopa/SolutionCopa.sln
dotnet build project/SolutionCopa/SolutionCopa.sln
dotnet test project/SolutionCopa/SolutionCopa.sln
```

Run a single test method:

```bash
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj --filter "FullyQualifiedName~DomainTest.MatchTests.MatchCanGetMatchId"
```

Run all tests in one class:

```bash
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj --filter "FullyQualifiedName~DomainTest.MatchTests"
```

There is no dedicated lint command configured in the repository.

## High-level architecture

- Main solution: `project/SolutionCopa/SolutionCopa.sln` (.NET 8).
- Layer roles and dependencies (from `.sln` + `.csproj` + implementation):
  - `Domain` (`ProjectCopa`): core entities, enums, interfaces, and domain exceptions.
  - `Services` (`namespace Services`): application orchestration (`MatchService`, `UserServices`), DTO mapping, and service-level exceptions.
  - `DataAccess`: repository and unit-of-work interfaces consumed by services (`IMatchRepository`, `ITeamRepository`, `IStadiumRepository`, `IGroupRepository`, `IJourneyRepository`, `IChampConfRepository`, `IUnitOfWork`).
  - `DomainTest`, `ServicesTest`: MSTest suites, including test doubles/stubs for domain interfaces and in-memory fake repositories for service tests.
  - `Controllers` and `UI`: currently scaffold-level placeholders.
- Dependency flow:
  - `Services` -> `Domain`, `DataAccess`
  - `DataAccess` -> `Domain`
  - `DomainTest` -> `Domain`, `Services`
  - `ServicesTest` -> `Domain`, `DataAccess`, `Services`
- Match model structure:
  - `Match` is abstract and owns shared match state/rules (date, teams, stadium, goals, winner logic).
  - `MatchGroup` and `MatchElimination` are static-factory-based specializations with extra invariants (`Group/Journey` and `Phase/MatchCode/SourceMatches`).
  - `MatchService` selects group vs elimination construction based on `CreateMatchRequestDto.IsElimination` and optional championship configuration.

## Key conventions in this codebase

- Keep existing API shape for users exactly as-is: `idUser`, `name`, `lastname`, `email`, `password`, `dateOfBirth`. Tests depend on these member names.
- Preserve namespace conventions used today:
  - Domain: `ProjectCopa`
  - Services: `Services`
  - Tests: `DomainTest`, `ServicesTest`
- Match-domain rule violations use `MatchException`; service orchestration/lookup validation uses `MatchServicesException`.
- Domain invariants are constructor/mutator-centric and fail-fast (validate first, then assign).
- Interface-first boundaries are expected for match-related entities (`ITeam`, `IStadium`, `IChampConf`, `IGroup`, `IJourney`) and for persistence (`I*Repository`, `IUnitOfWork`).
- Because `Match` is abstract, tests instantiate concrete test doubles (`DomainTest/TestDoubles/TestMatch`) or derived stubs, not `Match` directly.
- Service tests use in-memory fakes/stubs under `ServicesTest/TestDoubles`; domain tests use explicit stubs and occasional `[DataTestMethod]` coverage for enum/rule matrices.
- In tests, prefer explicit typing and named arguments where this codebase already uses them (especially in `ServicesTest/MatchServiceTest.cs`).
- Follow AGENTS.md project constraints when implementing features:
  - Gitflow branch model (`develop`, `feature/*`, `hotfix/*`)
  - TDD cycle for non-UI work (RED -> GREEN -> REFACTOR)
  - Keep business rules out of UI/controllers and in `Domain`/`Services`
  - Deterministic behavior for seeded random flows (`SemillaFixture`, `SemillaCompletar`, `SemillaCrucesFase`, `SemillaSimulation`)
- `.github/workflows/mstest-main.yml` still points to an old solution path (`project213/WorldCupPlanner/...`); use local `project/SolutionCopa/...` commands as source of truth unless CI is updated.
