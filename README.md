# WorldCupPlanner 2026

A domain-driven C# application for planning and tracking FIFA World Cup 2026 matches.

---

## Project Description

**WorldCupPlanner 2026** (`SolutionCopa`) is a layered .NET 8 application that models World Cup match scheduling, scoring, and bracket progression. The domain enforces strict invariants via domain exceptions, and all behavior is developed following TDD with MSTest.

---

## Architecture

The solution follows a layered architecture (located in `project/SolutionCopa/`):

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Domain` | Entities, value validation, domain exceptions, interfaces |
| Services | `Services` | Use-case orchestration; depends on domain contracts |
| Data Access | `DataAccess` | Persistence and infrastructure implementations |
| Controllers | `Controllers` | Entry points / adapters |
| DTOs | `Dtos` | Transport contracts between layers and boundaries |
| Tests | `DomainTest`, `ServiceTest` | Unit tests for domain and service behavior |

---

## Domain Model

### `Match`
Base class representing a match between two teams in a stadium on a given date.

**Properties:** `MatchId`, `MatchDate`, `HomeTeam`, `AwayTeam`, `HomeGoals`, `AwayGoals`, `Stadium`

**Invariants:**
- Home and away teams must be different.
- Stadium is required.
- Match date must be ≥ tournament start date.
- Goals can never be negative.
- Removing a goal when the current count is 0 is invalid.

**Behaviors:**
- `ChangeStadium(IStadium)` — change the match venue.
- `SetGoals(int homeGoals, int awayGoals)` — set both scores at once.
- `AddHomeGoal()`, `AddAwayGoal()` — increment scores.
- `RemoveHomeGoal()`, `RemoveAwayGoal()` — decrement scores.
- `HasHomeTeamWon()`, `HasAwayTeamWon()`, `IsDraw()` — result queries.
- `GetWinner()` — returns winning `ITeam` or `null` if draw.

### `MatchGroup : Match`
Extends `Match` for the group stage. Adds:
- `IGroup Group` — the group this match belongs to.
- `IJourney Journey` — the matchday/round within the group.

### `MatchElimination : Match`
Extends `Match` for the knockout stage. Adds:
- `MatchCode` (required) — e.g. "QF1", "SF2".
- `MatchHome` / `MatchAway` — source matches for bracket progression.

---

## Features

- Full match model with group-stage and knockout-stage support.
- Strict domain invariant enforcement via exceptions.
- Individual goal management with full traceability.
- Team, Stadium, Group, Journey, and TournamentSettings models.
- Repository pattern for persistence abstraction.

---

## Technology Stack

- **Language:** C# 12
- **Runtime:** .NET 8
- **Testing:** MSTest 3
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
cd 255503---330734---326829

# Restore dependencies
dotnet restore project/SolutionCopa/SolutionCopa.sln

# Build the solution
dotnet build project/SolutionCopa/SolutionCopa.sln
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

# Run service tests
dotnet test project/SolutionCopa/ServiceTest/ServiceTest.csproj
```

---

## TDD Workflow

This repository follows a strict **Red → Green → Refactor** cycle:

1. **RED** — Write one failing test for one rule or behavior.
2. **GREEN** — Write the minimum code to make the test pass.
3. **REFACTOR** — Improve readability without changing behavior.

No phase is skipped. Each domain rule has a dedicated test.

---

## Error Handling

Domain rule violations throw custom exceptions (`MatchException`, `MatchServicesException`). Guards are explicit and fail-fast with named constant messages.

---

## Repository Structure

```
255503---330734---326829/
├── .github/
│   ├── workflows/
│   ├── skills/
│   └── copilot-instructions.md
├── project/
│   └── SolutionCopa/
│       ├── Domain/
│       ├── Services/
│       ├── DataAccess/
│       ├── Controllers/
│       ├── Dtos/
│       ├── DomainTest/
│       ├── ServiceTest/
│       └── SolutionCopa.sln
├── AGENTS.md
└── README.md (this file)
```

---

## Contribution Guidelines

- Follow the TDD workflow.
- Keep each PR scoped to one feature or fix.
- Use domain exceptions for all rule violations.
- Prefer interface-first modeling at domain boundaries.
- Use explicit guards and fail-fast patterns.

---

## License

TBD
