# MatchServices Design Rationale

This document explains **why** the suggested DTOs, interfaces, and test doubles exist for the Services layer.

It complements `MatchServices.Pending.md` (what to implement) with architectural reasons (why it should be implemented this way).

## 1) Why DTOs Exist

DTOs (Data Transfer Objects) separate the service contract from domain internals.

- They prevent controllers/UI from depending directly on domain classes (`Match`, `MatchGroup`, `MatchElimination`).
- They keep the public API stable even if domain models evolve.
- They allow stricter input/output control (security and validation boundaries).

### `MatchDto`
**Why:**
- Outbound model for read operations.
- Exposes data needed by UI/API consumers without exposing mutable domain state.
- Represents both match types with a single response shape (`IsElimination`, optional `MatchCode`, optional `Phase`, optional group/journey fields).

### `CreateMatchRequestDto`
**Why:**
- Inbound model for creation use case.
- Groups all required IDs and mode-specific fields in one place.
- Makes validation explicit at service boundary:
  - team and stadium identifiers
  - group/journey for group stage
  - source matches + phase + code for elimination stage

### `UpdateMatchRequestDto`
**Why:**
- Allows partial updates without forcing the caller to resend all fields.
- Reduces accidental overwrites.
- Clarifies which properties are editable after creation (date, stadium, teams).

### `GroupStandingDto`
**Why:**
- Output model focused on standings/read-model use case.
- Keeps aggregation logic result separate from match entity model.
- Avoids leaking internal calculations or domain traversal details to controllers.

## 2) Why `IMatchService` Interface Exists

`IMatchService` defines the **application contract** for match use cases.

- Decouples controllers from concrete implementation details.
- Enables replacement/refactor of implementation without changing consumers.
- Simplifies testing (controller tests mock `IMatchService`).
- Makes async behavior explicit (`Task`, `Task<T>`) for I/O operations.

### Why async methods
- Service operations call repositories (database/file/network in future implementations).
- Async improves scalability and avoids blocking request threads.
- `CancellationToken` enables cancellation in web/API workflows.

## 3) Why Repository Interfaces Exist (DataAccess/Interfaces)

Repository interfaces are ports between Services and persistence.

### `IMatchRepository`
**Why:** centralized match persistence and queries (by id/group/journey/date) behind one abstraction.

### `ITeamRepository`, `IGroupRepository`, `IJourneyRepository`, `IStadiumRepository`, `IChampConfRepository`
**Why:**
- Resolve referenced entities by id during create/update workflows.
- Keep domain validation in service layer independent from concrete storage technology.

### `IUnitOfWork`
**Why:**
- Coordinates atomic writes when an operation modifies multiple records.
- Important for operations like `PublishResultAsync` where consistency matters.

## 4) Why These Test Doubles Were Selected

Current test doubles are in `ServicesTest/TestDoubles/MatchServiceDependencyMocks.cs`.

### `MatchServiceDependencyMocks`
**Why this shape:**
- Provides all repository and unit-of-work mocks in one fixture object.
- Reduces repetitive setup code across tests.
- Makes tests focus on behavior instead of boilerplate.

### `MatchServiceTestFactory`
**Why factory helpers:**
- Produces valid domain objects quickly (`Match`, `MatchGroup`, `MatchElimination`).
- Avoids copy/paste constructor setup and keeps tests readable.
- Ensures tests start from valid defaults, then override only relevant data.

### Why Moq-based doubles (not in-memory repositories)
- Fast and isolated unit tests.
- Verify interaction contracts (`CommitAsync` called once, repository calls with expected args).
- No dependence on infrastructure or database configuration.

## 5) Layering Rules (Recommended)

- Controllers depend on `IMatchService` and DTOs.
- Services depend on domain + repository interfaces.
- DataAccess implements repository interfaces.
- Tests for Services mock repositories and unit of work.

This keeps responsibilities clear and minimizes coupling.

## 6) Practical Benefit Summary

This design gives:
- clearer boundaries,
- easier testability,
- safer refactoring,
- and a cleaner path to implement `MatchService` incrementally with TDD.

