# WorldCupPlanner 2026

A domain-driven C# application for planning and tracking FIFA World Cup 2026 matches.

---

## Project Description

**WorldCupPlanner 2026** (`SolutionCopa`) is a layered .NET 8 application that models World Cup match scheduling, scoring, and bracket progression. The domain enforces strict invariants via domain exceptions, and all behavior is developed following TDD with MSTest and Moq.

---

## Architecture

The solution follows a layered architecture:

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Domain` | Entities, value validation, domain exceptions, interfaces |
| Services | `Services` | Use-case orchestration; depends on domain contracts |
| Data Access | `DataAccess` | Persistence and infrastructure implementations |
| Controllers | `Controllers` | Entry points / adapters |
| DTOs | `Dtos` | Transport contracts between layers and boundaries |
| Tests | `DomainTest` | Unit tests for domain behavior and invariants |

---

## Domain Model

### `Match`
Base class representing a match between two teams in a stadium on a given date.

**Properties:** `MatchId`, `MatchDate`, `HomeTeam`, `AwayTeam`, `HomeGoals`, `AwayGoals`, `Stadium`

**Invariants:**
- Home and away teams must be different.
- Stadium is required.
- Match date must be ≥ tournament start date.
- Championship configuration is required when using the config overload.
- Goals can never be negative.
- Removing a goal when the current count is 0 is invalid.

**Behaviors:**
- `ChangeStadium(IStadium)` — change the match venue.
- `SetGoals(int homeGoals, int awayGoals)` — set both scores at once.
- `SetHomeGoals(int goals)` — set the home score independently.
- `SetAwayGoals(int goals)` — set the away score independently.
- `AddHomeGoal()` — increment home score by one.
- `AddAwayGoal()` — increment away score by one.
- `RemoveHomeGoal()` — decrement home score by one (throws if 0).
- `RemoveAwayGoal()` — decrement away score by one (throws if 0).
- `HasHomeTeamWon()` — returns `true` when home goals > away goals.
- `HasAwayTeamWon()` — returns `true` when away goals > home goals.
- `IsDraw()` — returns `true` when scores are equal.
- `GetGoalDifferenceForHome()` — returns `HomeGoals - AwayGoals`.
- `GetGoalDifferenceForAway()` — returns `AwayGoals - HomeGoals`.
- `GetWinner()` — returns winning `ITeam` or `null` if draw.

### `MatchGroup : Match`
Extends `Match` for the group stage. Adds:
- `IGroup Group` — the group this match belongs to.
- `IJourney Journey` — the matchday/round within the group.

Inherits all scoring behaviors from `Match`.

### `MatchElimination : Match`
Extends `Match` for the knockout stage. Adds:
- `MatchCode` (required) — e.g. "QF1", "SF2".
- `MatchHome` / `MatchAway` — source matches used for bracket progression (must be different, both required).

Inherits all scoring behaviors from `Match`.

---

## Features

- Match creation with full invariant enforcement.
- Group-stage matches (`MatchGroup`) with group and journey context.
- Knockout-stage matches (`MatchElimination`) with bracket source tracking.
- Individual home/away goal management (`SetHomeGoals`, `SetAwayGoals`, `AddHomeGoal`, `AddAwayGoal`, `RemoveHomeGoal`, `RemoveAwayGoal`).
- Result queries (`HasHomeTeamWon`, `HasAwayTeamWon`, `IsDraw`).
- Goal difference calculations (`GetGoalDifferenceForHome`, `GetGoalDifferenceForAway`).
- Interface-first domain boundaries (`ITeam`, `IStadium`, `IChampConf`, `IGroup`, `IJourney`).

---

## Technology Stack

- **Language:** C# 12
- **Runtime:** .NET 8
- **Testing:** MSTest 3, Moq 4
- **Architecture:** Layered + DDD-oriented domain model
- **IDE:** JetBrains Rider / Visual Studio

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Any IDE with C# support (Visual Studio 2022, Rider, VS Code + C# Dev Kit)

---

## Setup and Run

```bash
# Clone the repository
git clone https://github.com/IngSoft-DA1/255503---330734---326829.git
cd 255503---330734---326829/project/SolutionCopa

# Restore dependencies
dotnet restore SolutionCopa.sln

# Build the solution
dotnet build SolutionCopa.sln
```

---

## Test Execution

```bash
# Run all tests
dotnet test project/SolutionCopa/SolutionCopa.sln

# Run only domain tests
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj

# Run a specific test class
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj --filter "FullyQualifiedName~MatchTests"
```

---

## TDD Workflow

This repository follows a strict **Red → Green → Refactor** cycle:

1. **RED** — Write one failing test for one rule or behavior. Commit with `test(RED):` prefix.
2. **GREEN** — Write the minimum code to make the test pass. Commit with `feat(GREEN):` prefix.
3. **REFACTOR** — Improve readability and structure without changing behavior. Commit with `refactor:` prefix.

No phase is skipped. Each domain rule has a dedicated test.

---

## Error Handling

All domain rule violations throw `MatchException` (located in `Domain/Exceptions/MatchException.cs`), a custom exception derived from `Exception`. Guards are explicit and fail-fast, with named constant messages to avoid magic strings.

---

## Repository Structure

```
255503---330734---326829/
├── project/
│   └── SolutionCopa/
│       ├── Domain/               # Entities, exceptions, interfaces
│       │   ├── Match.cs
│       │   ├── MatchGroup.cs
│       │   ├── MatchElimination.cs
│       │   ├── Exceptions/
│       │   │   └── MatchException.cs
│       │   └── Interfaces/
│       │       ├── ITeam.cs
│       │       ├── IStadium.cs
│       │       ├── IChampConf.cs
│       │       ├── IGroup.cs
│       │       └── IJourney.cs
│       ├── Services/             # Use-case orchestration
│       ├── DataAccess/           # Persistence infrastructure
│       ├── Controllers/          # Entry points / adapters
│       ├── DomainTest/           # MSTest unit tests
│       │   ├── MatchTests.cs
│       │   ├── MatchGroupTests.cs
│       │   └── MatchEliminationTests.cs
│       └── SolutionCopa.sln
├── README.md
└── AGENTS.md
```

---

## Contribution Guidelines

- Follow the TDD workflow described above.
- Keep each PR scoped to one feature or fix (prefer small PRs aligned with TDD cycles).
- Use `MatchException` for all domain rule violations.
- Do not revert unrelated local changes.
- Interface-first modeling: prefer `ITeam`, `IStadium`, etc. over concrete classes at domain boundaries.
- Use PascalCase for all public members.
- Keep guard clauses explicit and fail-fast.
- One assertion per test whenever possible.

---

## License

TBD
