# AGENTS.md

## Scope
This file is the global working agreement for this workspace:
- `SolutionCopa/`
- Applies to Domain, Services, DataAccess, Controllers, Dtos, and tests.

If a future `SolutionCopa/AGENTS.md` is created, it may add stricter local rules, but this file remains the baseline.

## Product Context
Project: WorldCupPlanner 2026 (`SolutionCopa`).
Architecture style: layered architecture + DDD-oriented domain model.
Testing stack: MSTest (+ Moq for test doubles).

## Layer Responsibilities
- `Domain`: entities, value/rule validation, domain exceptions, interfaces.
- `Services`: orchestration/use cases, depends on domain contracts.
- `DataAccess`: persistence/infrastructure implementations.
- `Controllers`: entry points/adapters.
- `Dtos`: transport contracts between layers/boundaries.
- `DomainTest`: unit tests for domain behavior and invariants.

## Mandatory Development Workflow (TDD)
1. RED: write one failing test for one rule/behavior.
2. GREEN: implement minimum code to pass that test.
3. REFACTOR: improve readability/structure without behavior change.

Do not skip phases.

## Test Conventions
- Framework: MSTest.
- One assert per test whenever possible (team convention for clarity).
- Prefer focused test names that describe behavior.
- Use Moq for interfaces (`ITeam`, `IStadium`, `IChampConf`, etc.).
- Keep domain tests deterministic (no clock/network/random side effects).

## Current Domain Decisions
### Match (`Domain/Match.cs`)
- Rich domain model with invariant enforcement.
- Guards include:
  - home and away teams must be different.
  - stadium is required.
  - match date must be >= tournament start date.
  - championship configuration is required when using config overload.
- Uses `MatchException` for domain rule violations.
- Uses explicit behavior method `ChangeStadium(IStadium)`.

### MatchGroup (`Domain/MatchGroup.cs`)
- Inherits from `Match`.
- Uses interfaces only for extra context:
  - `IGroup Group`
  - `IJourney Journey`
- Group/Journey concrete classes are intentionally out of scope here and owned by another teammate.

### MatchElimination (`Domain/MatchElimination.cs`)
- Inherits from `Match`.
- Adds:
  - `MatchCode` (required)
  - source matches for bracket progression (required)

## Interfaces and Ownership Notes
- Keep using interface-first modeling in domain boundaries.
- `IGroup` and `IJourney` are contracts only in this workspace stage.
- Do not add concrete `Group` or `Journey` classes unless explicitly requested by team owner.

## Clean Code Rules (Applied)
- Use PascalCase names.
- Keep guard clauses explicit and fail fast.
- Avoid magic strings in logic; use named constants for messages.
- Prefer small private helper methods for repeated validation logic.

## Suggested Test Backlog (Generalizations)
- `MatchGroup`: creation, required dependencies, getters, inherited match behavior.
- `MatchElimination`: creation, required code/parents, getters, inherited match behavior.
- Keep each rule as an independent red/green/refactor cycle.

## Collaboration Rules
- Do not revert unrelated local changes.
- If unexpected modifications appear, stop and ask owner before proceeding.
- Keep changes scoped to the requested task.
- Prefer small PRs aligned with TDD cycles.

## Useful Commands
```bash
dotnet test C:/Users/Tommy/Desktop/Diseno1/255503---330734---326829/project/SolutionCopa/DomainTest/DomainTest.csproj

dotnet test C:/Users/Tommy/Desktop/Diseno1/255503---330734---326829/project/SolutionCopa/DomainTest/DomainTest.csproj --filter "FullyQualifiedName~MatchTests"
```

