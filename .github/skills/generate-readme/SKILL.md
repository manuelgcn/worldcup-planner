---
name: generate-readme
description: Project-owned skill that regenerates the repository README.md and the agent instructions file (AGENTS.md) from the live codebase, with the obligatory sections.
---

# Generate README (skill propio)

Skill **propio** del equipo para el Obligatorio 2. A diferencia de `create-readme`
(genérico, del Obligatorio 1), este skill conoce la estructura concreta de
WorldCupPlanner 2026 y produce **dos artefactos coordinados**:

1. `README.md` (raíz del repo) — documentación principal del proyecto.
2. La sección de instrucciones para el agente en `AGENTS.md` (raíz) — manteniendo
   sincronizada la copia en `project/SolutionCopa/AGENTS.md`.

## Cuándo usarlo

- Antes de un release/tag, para que el README refleje el estado real.
- Cuando se agrega una capa, proyecto o dependencia externa.
- Cuando cambian las credenciales seed o el modo de ejecución.

## Procedimiento

1. **Leer la fuente de verdad** antes de escribir nada:
   - `project/SolutionCopa/SolutionCopa.sln` y los `.csproj` (proyectos y paquetes).
   - `docker-compose.yml`, `INSTALL.md`, `appsettings.json` (ejecución y config).
   - `PLAN_OBLIGATORIO_2.md` (alcance vigente) y `AGENTS.md` (estado por fases).
2. **Componer `README.md`** con estas secciones obligatorias, en este orden:
   - **Descripción**: qué resuelve el sistema y para quién (1 párrafo).
   - **Tecnologías**: .NET 8, Blazor Server, EF Core + Azure SQL Edge (Docker),
     MSTest, librerías de exportación (CsvHelper / ClosedXML).
   - **Estructura de la solución**: tabla capa → proyecto → responsabilidad,
     respetando `UI → Services → Domain` y `Services → DataAccess → Domain`.
   - **Instrucciones de ejecución**: levantar Docker, migraciones, correr la app,
     credenciales seed por rol (Admin / Editor / Periodista).
   - **Compilar y testear**: comandos de `dotnet build` / `dotnet test` por capa.
   - **Contribución**: TDD, gitflow, no mover lógica de negocio a UI.
3. **Actualizar las instrucciones del agente** en `AGENTS.md`: estado por fases,
   convenciones y la lista "lo que un agente NO debe hacer". No duplicar reglas de
   negocio que ya viven en `CLAUDE.md`; referenciarlas.
4. **Sincronizar** `project/SolutionCopa/AGENTS.md` con la copia de la raíz.

## Reglas de estilo

- Conciso y accionable; sin marketing ni emojis.
- Todo comando debe poder copiarse y correr tal cual desde la raíz del repo.
- Identificadores de código en inglés; prosa en español.
- No inventar secciones ni dependencias: si algo no está en el código, no va.

## Verificación

- El README lista cada proyecto real de la solución y ninguno inexistente.
- Los comandos de build/test/migraciones coinciden con los de `AGENTS.md` §6 y §8.
- Las dos copias de `AGENTS.md` (raíz y `project/SolutionCopa/`) son idénticas.
