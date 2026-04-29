# MatchServices: What Must Be Implemented

This file defines what "implemented" means for `MatchServices` in this solution.

## How to Use This Document
- Use this file as the execution checklist (what must exist and what behavior is required).
- Use `MatchServices.Explained.md` as architectural rationale (why DTOs, interfaces, and test doubles were selected).

## 0) What "Async" Means
- `Async` means the method does not block the caller thread while waiting for I/O (database, API, file, etc.).
- In C#, async methods usually return:
  - `Task` -> asynchronous operation with no returned value.
  - `Task<T>` -> asynchronous operation that returns a value of type `T`.
- Why use it here:
  - Services call repositories (I/O). Async keeps the app responsive and scalable.
  - Controllers/UI can `await` service methods.

Example:
```csharp
public Task<MatchDto> GetByIdAsync(int matchId, CancellationToken ct = default);
```

## 1) Service Contract (`IMatchService`)
Create `IMatchService` in `Services/Interfaces` with the following methods.

### `Task<MatchDto> CreateMatchAsync(CreateMatchRequestDto request, CancellationToken ct = default)`
- Purpose: Creates a new match (`MatchGroup` or `MatchElimination`).
- Input:
  - `request` with teams, date, stadium and type-specific fields.
- Returns:
  - Created match mapped to `MatchDto`.
- Errors:
  - Validation (400): invalid teams/date/stadium/phase/match code.
  - Not found (404): referenced entities do not exist.
  - Conflict (409): duplicated identity/code or invalid business state.

### `Task<MatchDto> UpdateMatchAsync(int matchId, UpdateMatchRequestDto request, CancellationToken ct = default)`
- Purpose: Updates editable fields of an existing match.
- Input:
  - `matchId` target match id.
  - `request` with allowed updates.
- Returns:
  - Updated `MatchDto`.
- Errors:
  - 404 if match does not exist.
  - 400/409 for invalid or conflicting changes.

### `Task DeleteMatchAsync(int matchId, CancellationToken ct = default)`
- Purpose: Deletes a match.
- Input: `matchId`.
- Returns: no value.
- Errors:
  - 404 if match does not exist.
  - 409 if deletion is blocked by business dependencies.

### `Task<MatchDto> GetByIdAsync(int matchId, CancellationToken ct = default)`
- Purpose: Gets one match by id.
- Returns: `MatchDto`.
- Errors: 404 if not found.

### `Task<IReadOnlyCollection<MatchDto>> GetByGroupAsync(string groupName, CancellationToken ct = default)`
- Purpose: Gets matches by group.
- Returns: read-only list (empty list is valid).

### `Task<IReadOnlyCollection<MatchDto>> GetByJourneyAsync(int journeyNumber, CancellationToken ct = default)`
- Purpose: Gets matches by journey number.
- Returns: read-only list (empty list is valid).

### `Task<IReadOnlyCollection<MatchDto>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)`
- Purpose: Gets matches in a date range.
- Rule: `from <= to`.
- Errors: 400 if range is invalid.

### `Task SetGoalsAsync(int matchId, int homeGoals, int awayGoals, CancellationToken ct = default)`
- Purpose: Replaces match score.
- Rules: goals must be non-negative.
- Errors: 404 if not found, 400 if invalid goals.

### `Task AddGoalAsync(int matchId, bool isHomeTeam, CancellationToken ct = default)`
- Purpose: Adds one goal to home or away side.
- Input: `isHomeTeam=true` -> home, `false` -> away.
- Errors: 404 if not found.

### `Task RemoveGoalAsync(int matchId, bool isHomeTeam, CancellationToken ct = default)`
- Purpose: Removes one goal from home or away side.
- Rule: cannot remove goal when score is already 0.
- Errors: 404 if not found, 400 if score is 0.

### `Task PublishResultAsync(int matchId, CancellationToken ct = default)`
- Purpose: Publishes final result and advances winner when elimination rules apply.
- Returns: no value.
- Errors:
  - 404 if match does not exist.
  - 409 if winner cannot be advanced because bracket state is invalid.

## 2) Match Types Supported
- Group stage: `MatchGroup`.
- Elimination stage: `MatchElimination` with `Phase` (`Final`, `SemiFinal`, `QuarterFinals`, `RoundOf16`, `RoundOf32`).

## 3) Mandatory Business Rules
- Home team and away team must be different.
- Stadium is required.
- Match date must be valid against championship start date.
- Goals cannot be negative.
- Goal removal when score is 0 is not allowed.
- `MatchElimination` requires:
  - non-empty `MatchCode`
  - `MatchHome` and `MatchAway`
  - different source matches
  - valid `Phase` enum value

## 4) Data Access Dependencies
- Add repository interfaces in `DataAccess/Interfaces`:
  - `IMatchRepository`
  - `ITeamRepository`
  - `IGroupRepository`
  - `IJourneyRepository`
  - `IStadiumRepository`
  - `IChampConfRepository`
- Add `IUnitOfWork` for atomic writes when publishing results and advancing elimination matches.

## 5) DTOs and Mapping
- Create DTOs in `Services/DTOs`:
  - `MatchDto`
  - `CreateMatchRequestDto`
  - `UpdateMatchRequestDto`
  - `GroupStandingDto`
- Do not expose domain entities directly to UI.

## 6) Error Contract
- Validation errors -> 400.
- Not found -> 404.
- Business conflict -> 409.
- Unexpected errors -> 500.

## 7) Definition of Done
- All `IMatchService` methods implemented.
- Unit tests for success and failure paths.
- Coverage for match creation, updates, goals, publication, and elimination phase rules.
- Controllers can consume service methods without using domain internals.

## 8) Recommended Implementation Order
1. Define/confirm DTOs in `Services/DTOs`.
2. Define/confirm `IMatchService` in `Services/Interfaces`.
3. Define/confirm repository contracts in `DataAccess/Interfaces`.
4. Implement `MatchService` method-by-method in TDD order:
   - reads (`GetByIdAsync`, filters)
   - score updates (`SetGoalsAsync`, `AddGoalAsync`, `RemoveGoalAsync`)
   - writes (`CreateMatchAsync`, `UpdateMatchAsync`, `DeleteMatchAsync`)
   - orchestration (`PublishResultAsync` + `IUnitOfWork`)

## 9) Minimum Test Matrix (ServicesTest)
- `CreateMatchAsync`: success, same-team validation, missing dependencies, invalid elimination data.
- `UpdateMatchAsync`: success, not found, invalid edits.
- `SetGoalsAsync` / `AddGoalAsync` / `RemoveGoalAsync`: success and domain failure paths.
- `GetBy...` methods: valid filtering and empty-result behavior.
- `PublishResultAsync`: success path and bracket conflict path.

Use `ServicesTest/TestDoubles/MatchServiceDependencyMocks.cs` to avoid duplicated setup.

## 10) Companion References
- Contract rationale: `Services/MatchServices.Explained.md`
- Service interface: `Services/Interfaces/IMatchService.cs`
- Test doubles: `ServicesTest/TestDoubles/MatchServiceDependencyMocks.cs`
