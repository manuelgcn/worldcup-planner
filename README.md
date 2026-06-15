# WorldCupPlanner 2026

> README generado con el skill propio `generate-readme` (ver `.github/skills/`).

## Descripción

WorldCupPlanner 2026 es una solución .NET 8 por capas para gestionar un torneo
mundial de 48 equipos (12 grupos de 4): usuarios y roles, equipos con bandera,
estadios, grupos, partidos e incidencias, fixture de grupos y cruces eliminatorios
(Round of 32 → Final). Incorpora ranking dinámico (Elo), motores de simulación
intercambiables, notificaciones a periodistas, exportación de fixture y auditoría, y
auditoría global del sistema. La lógica de negocio vive en el dominio; los servicios
orquestan; la UI solo presenta.

## Tecnologías

- **C# / .NET 8**.
- **Blazor Server** para la UI.
- **EF Core (Code-First)** contra **Azure SQL Edge** en Docker (autosave por
  operación vía `IUnitOfWork`). Jerarquías por TPH.
- **MSTest** + test doubles (provider InMemory de EF; SQLite in-memory para FKs).
- **CsvHelper** y **ClosedXML** para exportación CSV/XLSX.

## Estructura de la solución

Solución única: `project/SolutionCopa/SolutionCopa.sln`.

| Capa | Proyecto | Rol |
|---|---|---|
| Dominio | `Domain` (`ProjectCopa`) | Entidades, invariantes, enums y excepciones de negocio |
| Servicios | `Services` | Casos de uso, orquestación, DTOs, auditoría y motores de simulación |
| Datos | `DataAccess` | `AppDbContext`, repos EF, `Configurations/`, `Migrations/`, `IUnitOfWork` |
| UI | `UI` | Blazor Server, solo presentación |
| Pruebas | `DomainTest`, `ServiceTest`, `DataAccessTest` | MSTest |

Dependencias: `UI → Services → Domain` y `Services → DataAccess → Domain`.
`Domain` no depende de nadie.

## Ejecución

Detalle completo en `INSTALL.md`. Resumen:

```bash
# 1. Levantar la base (Azure SQL Edge)
cp .env.example .env
docker compose up -d sqlserver

# 2. Aplicar migraciones (la app también las aplica al arrancar)
dotnet ef database update \
  --project project/SolutionCopa/DataAccess \
  --startup-project project/SolutionCopa/UI

# 3. Correr la UI
dotnet run --project project/SolutionCopa/UI/UI.csproj
```

Abrir el browser en la URL que muestra la consola.

### Correr desde la Release formal

Para evaluar la entrega sin compilar, descargar el asset
`WorldCupPlanner-<tag>-win-x64.zip` de la **Release** de GitHub (tag
`v2.0.0-entrega2`), extraerlo completo y, con Docker Desktop iniciado, ejecutar
`start.ps1`: levanta y espera Azure SQL Edge antes de iniciar la app
self-contained. La integridad del zip se verifica con el `.sha256` adjunto. No
ejecutar `UI.exe` con la base detenida (detalle en `INSTALL.md`).

### Credenciales seed

| Rol        | Email                 | Password       |
|------------|-----------------------|----------------|
| Admin      | admin@copa.com        | Admin@123      |
| Editor     | editor@copa.com       | Editor@123     |
| Periodista | periodista@copa.com   | Periodista@123 |

### Configuración

Las semillas y parámetros del torneo (`FixtureSeed`, `CompletionSeed`,
`CrossingsSeed`, `SimulationSeed`, fechas) se leen de
`project/SolutionCopa/UI/appsettings.json`, sección `TournamentSettings`. La
connection string vive en `ConnectionStrings:Default` (sobrescribible con la env
`ConnectionStrings__Default`).

## Compilar y testear

```bash
dotnet restore project/SolutionCopa/SolutionCopa.sln
dotnet build   project/SolutionCopa/SolutionCopa.sln
dotnet test    project/SolutionCopa/SolutionCopa.sln
```

Pruebas por capa:

```bash
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj
dotnet test project/SolutionCopa/ServiceTest/ServiceTest.csproj
dotnet test project/SolutionCopa/DataAccessTest/DataAccessTest.csproj
```

## Reglas clave

- Ordenamiento y simulación determinísticos con semillas; toda operación con semilla
  queda auditada (timestamp ISO-8601 + usuario).
- Cupos por confederación: UEFA 16, CONMEBOL 7, CONCACAF 7, CAF 9, AFC 8, OFC 1.
- Fixture de 12 grupos × 6 partidos con rotación de estadios por nombre normalizado.
- Clasificación: Pts → DG → GF → sorteo con `CrossingsSeed`.

## Contribución

- TDD: red → green → refactor. Un commit por paso en el flujo de fixture.
- Gitflow: `feature/*` desde `develop`; `main` solo para release/tag.
- No mover lógica de negocio a UI ni Controllers.
- Conservar nombres públicos existentes (en especial en `User` y DTOs).
- Convenciones e instrucciones de agente en `AGENTS.md`; reglas de negocio en `CLAUDE.md`.

## Uso de IA generativa

Skills del repo en `.github/skills/` (índice en `.github/skills/README.md`).
Para el Obligatorio 2 (Fase 10) se agregaron dos skills nuevos
(`efcore-migrations-helper`, `audit-logger-instrumentation`) y un skill **propio**
(`generate-readme`) que produce este README y mantiene `AGENTS.md`.
