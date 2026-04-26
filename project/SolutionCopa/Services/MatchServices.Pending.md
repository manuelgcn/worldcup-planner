# MatchServices: What Must Be Implemented

This file defines what "implemented" means for `MatchServices` in this solution.

## 1) Service Contract
- Create `IMatchService` in `Services/Interfaces`.
- Methods required:
  - `CreateMatchAsync`
  - `UpdateMatchAsync`
  - `DeleteMatchAsync`
  - `GetByIdAsync`
  - `GetByGroupAsync`
  - `GetByJourneyAsync`
  - `GetByDateRangeAsync`
  - `SetGoalsAsync`
  - `AddGoalAsync`
  - `RemoveGoalAsync`
  - `PublishResultAsync`

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
- Add repository interfaces in `DataAccess`:
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

