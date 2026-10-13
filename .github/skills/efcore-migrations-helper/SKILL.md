---
name: efcore-migrations-helper
description: Guide EF Core Code-First migrations in this repo (Azure SQL Edge + TPH) without breaking the migration chain or seeded data.
---

# EF Core Migrations Helper

Skill nuevo (no usado en el Obligatorio 1) para trabajar las migraciones EF Core
de WorldCupPlanner 2026 de forma segura. La persistencia es **EF Core Code-First
contra Azure SQL Edge en Docker**, con jerarquías por **TPH**.

## Antes de tocar el modelo

- Configurá cada entidad con `IEntityTypeConfiguration<T>` en
  `src/WorldCupPlanner.DataAccess/Configurations/` — nunca fluido suelto en `OnModelCreating`.
- Jerarquías (`Match` → `MatchGroup`/`MatchElimination`, `MatchIncident` →
  `YellowCardIncident`/`RedCardIncident`) van por **TPH** con discriminador.
- TDD primero: escribí el test con el provider InMemory (`UseInMemoryDatabase`).
  Para flujos con **foreign keys reales** usá **SQLite in-memory** (InMemory no
  valida FKs).

## Crear y aplicar una migración

```bash
docker compose up -d

dotnet ef migrations add <Nombre> \
  --project src/WorldCupPlanner.DataAccess \
  --startup-project src/WorldCupPlanner.UI

dotnet ef database update \
  --project src/WorldCupPlanner.DataAccess \
  --startup-project src/WorldCupPlanner.UI
```

- Nombre de migración descriptivo en PascalCase (`AddTeamFlag`, `AddMatchIncidents`).
- Siempre `--startup-project UI` (ahí vive la connection string).

## Reglas que no se rompen

- **No edites migraciones ya commiteadas** ni reordenes la cadena. Si te equivocaste
  en una migración no aplicada en remoto: `dotnet ef migrations remove` y rehacela.
- **Columnas nuevas sobre tablas con datos sembrados** (`TournamentSettings`):
  poné `defaultValue` en el `Up()` para no fallar el `Migrate()` de arranque.
- Columnas que pasan a nullable (ej. `AuditLog.TournamentSettingsId`): revisá que
  ningún servicio asuma `!= 0`.
- Revisá el `.cs` generado antes de commitear: que el `Up()`/`Down()` sean simétricos
  y que el snapshot (`AppDbContextModelSnapshot.cs`) entre en el mismo commit.
- La app corre `db.Database.Migrate()` al arrancar (`src/WorldCupPlanner.UI/Program.cs`): una migración
  rota deja la app sin levantar.

## Verificación

- `dotnet build` + las 3 suites verdes.
- `dotnet ef database update` corre limpio sobre una base recreada con
  `docker compose down -v && docker compose up -d`.
- El commit incluye: la migración, el snapshot y la configuración nueva.

## Ejemplo de uso documentado

Para `Team.FlagBase64` (Fase 2): se agregó `SetFlag` + configuración, se generó
`AddTeamFlag` con columna `nvarchar(max)` nullable, y los tests de repositorio se
escribieron primero contra SQLite in-memory para validar el round-trip.
