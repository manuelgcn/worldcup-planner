# AGENTS.md — Contexto para agentes de IA (Copilot, JetBrains AI, Claude)

Este archivo es la **fuente única de verdad sobre el estado actual y el workflow** del repo para cualquier agente de IA. Si trabajás en Rider con Copilot, en VSCode, o en cualquier otro entorno: leé esto antes de proponer cambios.

> **AGENTS.md vs CLAUDE.md**: AGENTS.md describe el **estado actual + cómo trabajar**. `CLAUDE.md` tiene el detalle de **reglas de negocio** (fixture, clasificación, cruces) y sigue siendo válido para eso, pero su sección de "persistencia en memoria" quedó vieja: **hoy la persistencia es EF Core + SQL** (ver §2). Ante conflicto sobre el estado del código, manda AGENTS.md.

---

## 1. Proyecto

**WorldCupPlanner 2026** — app para gestionar la fase de grupos del Mundial 2026 (48 equipos, 12 grupos de 4), clasificación dinámica y cruces eliminatorios (Round of 32 → Final) con sorteos y simulaciones **determinísticos por semilla**.

Estamos en el **Obligatorio 2 de Diseño de Aplicaciones 1 — ORT**.
- **Entrega**: 17/06/2026 21:00 vía Gestión (PDF ≤ 15 págs.) + **Release/Tag en `main`** (org `IngSoft-DA1`).
- El plan maestro de esta entrega está en `PLAN_OBLIGATORIO_2.md` (12 fases).

## 2. Stack y estructura

- **Lenguaje / runtime**: C# / .NET 8.
- **UI**: Blazor Server (ya implementada y funcional, con páginas reales).
- **Persistencia**: **EF Core (Code First) contra Azure SQL Edge en Docker**. Autosave por operación vía `IUnitOfWork.Commit()` → `SaveChangesAsync`. Nada de "guardar todo" manual.
- **Tests**: MSTest + test doubles propios en `*/TestDoubles/`.
  - Unit/servicios: provider **InMemory** de EF (`UseInMemoryDatabase`).
  - **Lección aprendida**: InMemory **NO valida foreign keys**. Para flujos con FKs reales (jerarquía de `Match`, cruces que referencian partidos fuente) usar **SQLite in-memory** en `DataAccessTest` (ver `PROMPT-fase5-fix-simulacion-grupos.md`).
- **Solution**: `project/SolutionCopa/SolutionCopa.sln`.

Proyectos:

| Proyecto | Namespace | Responsabilidad |
|---|---|---|
| `Domain` | `ProjectCopa` | Entidades, enums, invariantes, excepciones de dominio. Validan en constructor/mutadores. |
| `Services` | `Services` | Casos de uso, orquestación, DTOs, auditoría, motores de simulación. |
| `DataAccess` | `DataAccess` | `AppDbContext`, repos EF, `Configurations/` (`IEntityTypeConfiguration<T>`), `Migrations/`, `IUnitOfWork`. |
| `UI` | (Blazor Server) | Pantallas (`Components/Pages/`). **Sin lógica de negocio.** |
| `Controllers` | (scaffold) | Placeholder, no se usa. |
| `DomainTest` / `ServiceTest` / `DataAccessTest` | — | MSTest por capa. Acá vive el TDD. |

Flujo de dependencias: `UI → Services → Domain` y `Services → DataAccess → Domain`. `Domain` no depende de nadie.

## 3. Estado actual por fases (al 2026-06-07)

Referencia: las fases son las de `PLAN_OBLIGATORIO_2.md`.

- ✅ **Fase 0–1 (persistencia)**: Docker + Azure SQL Edge, `AppDbContext`, repos migrados a EF, `IUnitOfWork`. Jerarquía `Match`/`MatchGroup`/`MatchElimination` con **TPH**.
- ✅ **Fase 2 (extensiones de entidades)**: `Team.FlagBase64` (+ `SetFlag`), `Team.UpdateFifaRanking(int)` (clamp 300–2500), jerarquía `MatchIncident` (`YellowCardIncident`, `RedCardIncident`, TPH), rol **`Periodista`** en `UserRoles`. Migraciones hasta `20260606223107_AddMatchIncidents`.
- 🚧 **Fase 3 (ranking Elo)**: **en curso por otro integrante** en `feature/ranking-elo`. Ya existen tests (`DomainTest/EloRankingCalculatorTests.cs`); la implementación (`EloRankingCalculator`, `RankingService`) puede no estar mergeada todavía. **No la toques** desde otras ramas.
- ✅ **Fase 4 (motores de simulación intercambiables)**: `IMatchSimulationEngine`, registro de motores, selección persistida y simulaciones auditadas.
- ✅ **Fase 5 (notificaciones)**: notificaciones persistidas para periodistas, lectura individual/masiva y UI.
- ✅ **Fase 6 (exportación CSV/XLSX)**: `IExporter` + `IExportService`, CsvHelper/ClosedXML, exportación de fixture/auditoría por rango, autorización Admin/Periodista y auditoría.
- ✅ **Fase 7 (auditoría global)**: `AuditLog` desacoplado del torneo (`TournamentSettingsId` nullable como contexto opcional), actor estructurado `PerformedByEmail`, filtros por fecha/acción/usuario ejecutados en SQL (`IAuditLogRepository.GetFiltered`), acciones `SIMULATION_EXECUTED` y `RANKING_UPDATED`, vista y exportación filtradas. Migración `20260611232454_MakeAuditLogsGlobalWithActor`.
- ✅ **Fase 10 (skills de IA)**: 2 skills nuevos (`efcore-migrations-helper`, `audit-logger-instrumentation`) + 1 propio (`generate-readme`) en `.github/skills/`; `README.md` regenerado con `generate-readme`; instrucciones de agente en este `AGENTS.md`.
- ⬜ **Fases 8, 9, 11, 12**: UI rol Periodista, refactor GRASP/Demeter/SOLID, datos de prueba + backups, documentación.

**Roles**: `UserRoles { Admin, Editor, Periodista }`.

## 4. Convenciones de código (mirá el archivo vecino antes de inventar)

Referencias de estilo vivo: `Team.cs`, `FixtureOrchestrator.cs`, `TournamentSimulationService.cs`, `AuditActions.cs`.

- **Identificadores en inglés** (clases, métodos, propiedades): `Team.Name`, `FifaRanking`, `MatchService.SimulateMatch`, `AuditActions.GenerateFixture`. **Strings de UI en español** (las `.razor` están en español).
- **Mensajes de error y números mágicos como constantes con nombre** arriba de la clase (ver `private const string ... Message` en `Team.cs`/`FixtureOrchestrator.cs`).
- **Validación fail-fast**: en el constructor/mutador del dominio para invariantes intrínsecas; reglas que cruzan entidades o requieren contexto (existencia, unicidad, "ese motor existe") van en `Services`.
- **Excepciones**: dominio (`SettingsException`, `MatchException`, …) ≠ servicio (`MatchServicesException`, `FixtureOrchestratorException`, …). Para familias nuevas, primary constructor: `public sealed class FooException(string message) : Exception(message);`.
- **Inmutabilidad**: propiedades `{ get; }` por defecto; **setter privado solo cuando hay un mutador con validación** (patrón `Stadium.Update`).
- **Determinismo (no negociable)**: todo sorteo/simulación usa `new Random(semilla)` (Fisher–Yates donde aplica). **Prohibido** `Random` sin semilla o `DateTime.Now` en dominio/servicios — rompe la reproducibilidad que pide la letra.
- **EF**: configuración por `IEntityTypeConfiguration<T>` en `DataAccess/Configurations/` (no fluido suelto en `OnModelCreating`). Jerarquías por **TPH**. No traigas todo a memoria con `ToListAsync()` cuando podés filtrar con `IQueryable` — la rúbrica penaliza grandes volúmenes en memoria.

## 5. TDD (obligatorio)

Ciclo, **un commit por paso**, con prefijo en el mensaje:

```
RED:      test nuevo que NO pasa/compila (símbolo inexistente cuenta como RED)
GREEN:    implementación mínima que lo hace pasar
REFACTOR: limpieza sin cambiar comportamiento (tests siguen verdes)
```

Reglas: test primero siempre; GREEN mínimo; una regla de negocio nueva → primero un test que la exprese; cobertura objetivo ≥ 90% por clase nueva (dotCover). `Match` es abstracta: en tests usá factories estáticos o test-doubles (`MockMatch`, `TestMatch`), nunca `new` directo.

## 6. Migraciones EF (comandos)

```bash
# Levantar la base
docker compose up -d

# Crear una migración (siempre con startup-project UI)
dotnet ef migrations add <Nombre> --project project/SolutionCopa/DataAccess --startup-project project/SolutionCopa/UI

# Aplicarla
dotnet ef database update --project project/SolutionCopa/DataAccess --startup-project project/SolutionCopa/UI
```

- **No edites migraciones ya commiteadas.** Al agregar columnas a tablas con datos sembrados (`TournamentSettings`), poné `defaultValue` en el `Up()`.
- La app aplica `db.Database.Migrate()` al arrancar (`UI/Program.cs`) y siembra `TournamentSettings`, admin y editor.

## 7. Auditoría

- Constantes en `Services/AuditActions.cs` (formato `SCREAMING_SNAKE`). Hoy cubren user/team/stadium/group/journey/match/fixture/knockout/simulación/exportación/notificaciones, más `SIMULATION_EXECUTED` (simulación individual exitosa, registra motor y semilla) y `RANKING_UPDATED` (reservada para Elo post-merge de `feature/ranking-elo`).
- Toda mutación relevante, uso de semilla y simulación se loggea con **timestamp ISO-8601 + usuario** vía `AuditLogService` / `LogPerformedBy`.
- Los logs son **globales (del sistema)**: `Log`/`LogPerformedBy` crean entradas sin torneo; `AuditLog.TournamentSettingsId` es `int?` y solo se pasa explícitamente cuando aporta contexto real. `LogPerformedBy` guarda el actor en `AuditLog.PerformedByEmail` (no concatena " by <email>" en `Details`; los logs históricos conservan el actor dentro de `Details`).
- Consultas filtradas vía `IAuditLogService.GetFiltered(AuditLogFilterDto)` → `IAuditLogRepository.GetFiltered` (rango de fechas inclusivo + acción y actor parciales case-insensitive, ejecutado en SQL con `IQueryable`). La vista de auditoría y `ExportService.ExportAuditLog` usan estos filtros; no cargar todo y filtrar en memoria.

## 8. Workflow (gitflow)

1. Una rama por feature: `feature/<nombre>` desde `develop`. PR a `develop`. `main` solo para release/tag.
2. TDD con commits RED/GREEN/REFACTOR; `feat:`/`chore:`/`docs:` para lo que no es ciclo TDD.
3. **Stageá solo lo deliberado**: nunca `git add .` / `-A`. No mezcles cambios funcionales con line-endings/reformateos/renombres.
4. Antes del PR: `dotnet build` + las 3 suites verdes (`DomainTest`, `ServiceTest`, `DataAccessTest`).
5. Al mergear `develop` dentro de tu rama, resolvé conflictos **entendiendo ambos lados** (nunca `--ours`/`--theirs` a ciegas).
6. Cada uso de IA queda referenciado en el commit/PR (herramienta + contexto).

```bash
dotnet build project/SolutionCopa/SolutionCopa.sln
dotnet test  project/SolutionCopa/SolutionCopa.sln
```

## 9. Skills de IA

En `.github/skills/` (índice en `.github/skills/README.md`):

- **Obligatorio 1**: `add-educational-comments`, `create-readme`, `csharp-mstest`, `git-commit`.
- **Obligatorio 2 (Fase 10)** — 2 skills nuevos + 1 propio:
  - `generate-readme` (**propio**): regenera `README.md` + estas instrucciones de agente desde el código vivo.
  - `efcore-migrations-helper`: migraciones EF Core (SQL Edge + TPH) sin romper la cadena ni los datos sembrados.
  - `audit-logger-instrumentation`: auditar casos de uso nuevos vía `AuditLogService`/`AuditActions` (ISO-8601 + actor).

Si tu entorno soporta skills, invocalas; si no, leé el `SKILL.md` y aplicá las reglas a mano. El `README.md` actual fue generado con `generate-readme`.

## 10. Lo que un agente NO debe hacer

- ❌ Reformatear archivos enteros o cambiar line-endings/encoding/BOM.
- ❌ `git add .` / `git add -A`.
- ❌ Renombrar miembros públicos existentes (los tests dependen de ellos), salvo renombres explícitamente indicados en el prompt de la tarea.
- ❌ Editar migraciones ya commiteadas o romper la cadena de migraciones.
- ❌ Introducir librerías nuevas (Newtonsoft, AutoMapper, etc.) sin avisar.
- ❌ Saltarse RED → GREEN → REFACTOR.
- ❌ `Random` sin semilla o `DateTime.Now` en dominio/servicios.
- ❌ Lógica de negocio en `.razor`/`.razor.cs` o en `Controllers`.
- ❌ Traer grandes volúmenes a memoria innecesariamente (usar `IQueryable`).
- ❌ Crear el PDF/documentación de entrega dentro del repo (va por Gestión).

## 11. Próximos pasos

1. **Fase 3 (Elo)** — en curso (otro integrante, `feature/ranking-elo`); al mergear, verificar que `RankingService` audite con `AuditActions.RankingUpdated`.
2. Fases 8–12 según `PLAN_OBLIGATORIO_2.md` (UI Periodista, GRASP/SOLID, skills, datos de prueba, documentación).

---

**Última actualización**: 2026-06-13.
**Mantener este archivo vivo**: cuando una sección quede desactualizada, corregirla en el mismo commit que introduce el cambio. Existe una copia en `project/SolutionCopa/AGENTS.md` que debe quedar **sincronizada** con esta.
