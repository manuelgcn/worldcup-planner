---
name: generate-readme
description: Project-owned skill that regenerates the repository README.md and the agent instructions file (AGENTS.md) from the live codebase.
---

# Generate README

Project-specific skill: it knows WorldCupPlanner's structure and keeps two files in sync
with the code:

1. `README.md` — the project's front page.
2. `AGENTS.md` — instructions for AI coding agents (`CLAUDE.md` just imports it).

## When to use

- Before tagging a release, so the README reflects the real state.
- After adding a layer, project or external dependency.
- When seed credentials or the way to run the app change.

## Procedure

1. **Read the sources of truth** before writing anything:
   - `WorldCupPlanner.sln` and the `.csproj` files (projects and packages).
   - `docker-compose.yml` and `src/WorldCupPlanner.UI/appsettings.json` (how it runs and is configured).
   - `.github/workflows/` (CI and release).
2. **Write `README.md`** with these sections, in order:
   - Summary: what the system does, the stack in one sentence, the CI badge and the home screenshot.
   - Features, then the tournament rules (from the business rules in `AGENTS.md`).
   - Getting started: Docker, running the app, demo data, accounts per role and configuration.
   - Tests.
   - Architecture: layer → project → responsibility, respecting `UI → Services → Domain` and `Services → DataAccess → Domain`.
   - Repository layout, release package, contributing, authors and license.
3. **Update `AGENTS.md`**: layout, commands, business rules, conventions and workflow.

## Style

- Concise and actionable, in English. Plain prose: no emoji headings, no marketing adjectives,
  bold only where a term needs it.
- No badges with hand-written numbers (test count, coverage): they go stale. Only badges that
  CI keeps up to date.
- Every command must run as-is from the repository root.
- Do not invent sections or dependencies: if it is not in the code, it does not go in.
- Test and coverage numbers come from an actual `dotnet test` run, never from memory.

## Verification

- The README lists every real project in the solution and none that does not exist.
- Build, test and migration commands match the ones in `AGENTS.md`.
