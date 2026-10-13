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
   - `project/SolutionCopa/SolutionCopa.sln` and the `.csproj` files (projects and packages).
   - `docker-compose.yml` and `UI/appsettings.json` (how it runs and is configured).
   - `.github/workflows/` (CI and release).
2. **Write `README.md`** with these sections, in order:
   - Summary: what the system does and for whom.
   - Engineering highlights and features.
   - Tech stack.
   - Architecture: layer → project → responsibility, respecting `UI → Services → Domain` and `Services → DataAccess → Domain`.
   - Getting started: Docker, migrations, run the app, demo data, seed credentials per role.
   - Running the tests.
   - Project structure, authors and license.
3. **Update `AGENTS.md`**: layout, commands, business rules, conventions and workflow.

## Style

- Concise and actionable, in English.
- Every command must run as-is from the repository root.
- Do not invent sections or dependencies: if it is not in the code, it does not go in.
- Test and coverage numbers come from an actual `dotnet test` run, never from memory.

## Verification

- The README lists every real project in the solution and none that does not exist.
- Build, test and migration commands match the ones in `AGENTS.md`.
