# Copilot Instructions for SolutionCopa (proposed improvements)

Overview

This file tells Copilot-style assistants how to work effectively in this repository (SolutionCopa / WorldCupPlanner). Keep recommendations concise and action-oriented.

Build, test, and lint

- Target SDK: .NET 8
- From repository root:
  - Restore: dotnet restore project\SolutionCopa\SolutionCopa.sln
  - Build:   dotnet build project\SolutionCopa\SolutionCopa.sln
  - Test:    dotnet test  project\SolutionCopa\SolutionCopa.sln

- Run a single test method (example):
  dotnet test project\SolutionCopa\DomainTest\DomainTest.csproj --filter "FullyQualifiedName~DomainTest.MatchTests.MatchCanGetMatchId"

- Run all tests in a test class (example):
  dotnet test project\SolutionCopa\DomainTest\DomainTest.csproj --filter "FullyQualifiedName~DomainTest.MatchTests"

- Lint: no dedicated linter configured. Prefer dotnet format/formatting rules if needed.

High-level architecture

- Main solution: project/SolutionCopa/SolutionCopa.sln (multi-project .NET 8 solution)
- Layer roles:
  - Domain (ProjectCopa): core entities, enums, domain exceptions, invariants
  - Services (Services namespace): application orchestration, DTOs, MatchService, UserServices
  - DataAccess: repository and unit-of-work contracts (I*Repository, IUnitOfWork)
  - Controllers / UI: thin adapters / scaffolding
  - Tests: DomainTest, ServiceTest, DataAccessTest (MSTest)
- Dependency flow:
  Services -> Domain, DataAccess
  DataAccess -> Domain
  Tests reference appropriate layers for focused unit tests
- Match domain:
  - Match is abstract; Group vs Elimination specializations exist (MatchGroup, MatchElimination)
  - Construction via MatchFactory/MatchService based on CreateMatchRequestDto.IsElimination
  - Deterministic behavior uses seeded RNGs (fixture/simulation/seeds)

Key conventions and repository-specific rules

- Preserve public API and model shapes used in tests (exact property names): idUser, name, lastname, email, password, dateOfBirth
- Namespaces to preserve: Domain -> ProjectCopa, Services -> Services, Tests -> DomainTest/ServiceTest
- Domain validations are fail-fast: validate in constructors/mutators and throw domain-specific exceptions (MatchException, MatchServicesException for service-level validation)
- Tests instantiate concrete test doubles for abstract types (e.g., TestMatch in DomainTest/TestDoubles)
- Service tests use in-memory fakes/stubs under ServiceTest/TestDoubles; domain tests use focused stubs and DataTestMethods
- Fixture and simulation algorithms rely on seeds (FixtureSeed, SimulationSeed, CompletionSeed). Keep deterministic behavior in tests by injecting seeds.
- CI note: .github/workflows/mstest-main.yml refers to an older path (project213/WorldCupPlanner/*). Use local project/SolutionCopa paths as ground truth when running commands locally.

Other AI-assistant files

- No CLAUDE.md, AGENTS.md, .cursorrules, CONVENTIONS.md, or similar assistant config files were found at repo root. If present later, include their relevant instructions here.

How Copilot/assistant should behave

- Prefer using the solution-level dotnet commands shown above for builds and tests.
- When editing domain rules, keep logic in Domain/Services, not in UI/controllers.
- For multi-file changes, propose a minimal, test-backed change set. Run relevant unit tests before committing.
- When in doubt about feature scope or choices that affect domain invariants (seeds, fixture rules, match invariants), ask maintainer.

Applying changes

- If changes modify public contracts or test fixtures, update tests accordingly and run dotnet test across affected projects.

Notes / TODOs for maintainers

- Consider updating CI workflow to point to project/SolutionCopa paths.
- Optionally add a small CONTRIBUTING.md describing branch names and TDD requirement (develop, feature/*, hotfix/*).

---

If you'd like, apply these changes to replace the existing .github/copilot-instructions.md or merge parts into it. Also, do you want configuration for MCP servers (e.g., Playwright) added for this project type?