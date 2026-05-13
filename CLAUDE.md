# CLAUDE.md — Guía para agentes en WorldCupPlanner 2026

Este archivo es la referencia canónica para agentes de IA (Claude, Copilot, etc.) que trabajen sobre este repositorio. Resume el alcance del Obligatorio 1 de Diseño de Aplicaciones 1 (entrega 14/05/2026, 21:00) y el estado actual del código.

---

## 1. Contexto del proyecto

**WorldCupPlanner 2026** es una aplicación para gestionar la fase de grupos del Mundial 2026 (48 equipos, 12 grupos de 4) y preparar los cruces eliminatorios mediante sorteos determinísticos (con semilla) para garantizar trazabilidad y reproducibilidad.

- Carrera: AN / ID — Materia: Diseño de Aplicaciones 1.
- Entrega: vía Gestión (PDF ≤ 15 págs.) + Repositorio GitHub en la organización `IngSoft-DA1` con release/tag en `main` antes de las 21:00 del 14/05/2026.
- Defensa: entre 15/05/2026 y 04/06/2026 (no presentarse = pérdida total de puntos).
- Persistencia: **en memoria** (sin base de datos en esta entrega).
- Nombre del repositorio esperado: `NumeroEstudiante1_NumeroEstudiante2_NumeroEstudiante3`.

---

## 2. Stack y reglas no funcionales

- **Lenguaje / Runtime**: C# en .NET 8.
- **UI**: Blazor Server App. La UI no debe contener lógica de negocio.
- **Testing**: MSTest con TDD (RED → GREEN → REFACTOR). Un commit por test en el flujo de "Generación de fixture" para evidenciar el ciclo.
- **Control de versiones**: Git + **Gitflow** (`main`, `develop`, `feature/*`, `hotfix/*`).
- **CI**: GitHub Actions ejecuta los tests en cada PR a `main` (`.github/workflows/dotnet-tests.yml`).
- **Clean Code**: aplicar capítulos 1–10 y 12 del libro; convenciones C# oficiales de Microsoft.
- **Cobertura**: reporte con JetBrains dotCover. Si una funcionalidad no llega a 90% de líneas, justificar.

---

## 3. Estructura de la solución

Solución única: `project/SolutionCopa/SolutionCopa.sln`.

| Capa | Proyecto | Namespace | Rol |
|---|---|---|---|
| Dominio | `Domain` | `ProjectCopa` | Entidades, enums, invariantes, excepciones de negocio |
| Servicios | `Services` | `Services` | Casos de uso, orquestación, DTOs, auditoría |
| Acceso a datos | `DataAccess` | `DataAccess` | Repositorios en memoria + `IUnitOfWork` |
| UI | `UI` | (Blazor Server) | Pantallas, sólo presentación |
| Controllers | `Controllers` | (scaffold) | Placeholder, no se usa lógica aquí |
| Tests dominio | `DomainTest` | `DomainTest` | MSTest del dominio |
| Tests servicios | `ServiceTest` | `ServiceTest` | MSTest de servicios + test doubles |
| Tests datos | `DataAccessTest` | `DataAccessTest` | MSTest de repositorios |

**Flujo de dependencias permitido**:

```
UI  →  Services  →  Domain
                 ↘  DataAccess  →  Domain
```

`Domain` no depende de nadie. `DataAccess` solo de `Domain`. `Services` de ambos. `UI` consume `Services` (no dominio directo salvo entidades).

---

## 4. Dominio actual (`Domain/`)

Entidades / clases clave ya implementadas:

- `User` — `idUser`, `name`, `lastname`, `email`, `password` (SHA256, no texto plano), `dateOfBirth`, `Roles`. Reglas de password: ≥8 chars, mayúscula, minúscula, número, especial. **No cambiar los nombres `idUser`, `name`, `lastname`, `email`, `password`, `dateOfBirth`**: los tests dependen de ellos.
- `Team` — `Id`, `Name` (1–60, único), `FifaRanking` (300–2500), `IsFictitious`, `Confederation`.
- `Stadium` — `Name` (1–80, único), `City` (1–60), `Description` (opcional 1–400), `Capacity` (≥ 20.000).
- `Group` — etiqueta `A`–`L`, máx. 4 equipos.
- `Journey` — jornada del fixture de grupos.
- `Match` (abstracta) — fecha, equipos, estadio, goles, ganador.
- `MatchGroup` — `Match` + `Group` + `Journey`.
- `MatchElimination` — `Match` + `Phase` + `MatchCode` + `MatchHome` + `MatchAway`.
- `AuditLog` — `Timestamp` ISO-8601, `Action`, `Details`, `TournamentSettingsId`, `AffectedEntityType`, `AffectedEntityId`.
- `TournamentSettings` — semillas y parámetros (ver §6).
- Helpers de dominio: `GroupFixtureGenerator`, `GroupQualifier`, `StadiumScheduleTracker`.
- Enums: `Confederation` (AFC, CAF, CONCACAF, CONMEBOL, OFC, UEFA), `Phase`, `UserRoles`.
- Excepciones específicas en `Domain/Exceptions/`: `MatchException`, `SettingsException`, `AuditLogException`, etc.

---

## 5. Servicios actuales (`Services/`)

- `UserServices` — ABM de usuarios, asignación de roles, auditoría en mutaciones.
- `TeamServices` — CRUD, cupos por confederación, orden determinístico (Fisher–Yates + `FixtureSeed`), exige 48 equipos para fixture, generación automática de ficticios con `CompletionSeed`.
- `StadiumServices` — CRUD con auditoría.
- `GroupServices`, `JourneyServices` — soporte para fixture.
- `FixtureService` — construye los 12 grupos, valida 48 equipos y ≥4 estadios, ordena por ranking DESC, rompe empates con `FixtureSeed`, arma 4 bombos × 12 y reparte A→L en round-robin.
- `MatchService` + `MatchFactory` — alta/edición/eliminación de partidos, goles, simulación y publicación de resultado.
- `MatchSimulationEngine` — algoritmo basado en `RankingActual` + `SimulationSeed`.
- `EliminationBuilder`, `EliminationOrchestrator`, `QualificationService`, `Fase2`, `TournamentSimulationService` — clasificación post-grupos y cruces (Round of 32 → Final).
- `ImportService` + `CsvStreamTeamParser` (interfaz `ITeamParser`) — importación CSV con encabezados exactos `Nombre, Confederación, RankingFIFA`.
- `AuditLogService` + constantes `AuditActions` — registro de mutaciones (ya cubre user, team, stadium, group, journey, match, fixture, knockout).

DTOs en `Services/DTOs/`. Excepciones de servicio en `Services/Exceptions/` (separadas de las de dominio).

---

## 6. Parámetros configurables (semillas) — obligatorios

Centralizados en `TournamentSettings`. Deben poder configurarse por UI o archivo (`appsettings.json`).

| Parámetro | Default | Uso |
|---|---|---|
| `FixtureSeed` | 0 | Fisher–Yates para empates de ranking y orden de bombos |
| `CompletionSeed` | 0 | Generación de equipos ficticios (`UEFA_01`, ...) y su ranking |
| `CrossingsSeed` | 0 | Fisher–Yates para sortear cruces de fase 2 |
| `SimulationSeed` | 0 | Simulación reproducible de resultados |
| `StartDate` | 2026-06-01 | Fecha base del calendario |
| `MaxMatchesPerDay` | 3 | Tope partidos/día (separación 4h) |
| `DaysBetweenJourneys` | 3 | Distancia entre jornadas D, D+3, D+6 |

**Toda invocación de una semilla debe registrarse en el log de auditoría** (timestamp ISO-8601 + usuario).

---

## 7. Reglas de negocio críticas a respetar

- **Normalización de strings**: minúsculas, sin tildes, no-alfanuméricos → espacio, colapsar espacios, trim.
- **Fisher–Yates determinístico con `System.Random(semilla)`** es el único método permitido para barajar (empates de ranking, bombos, cruces).
- **Cupos por confederación**: UEFA 16, CONMEBOL 7, CONCACAF 7, CAF 9, AFC 8, OFC 1 → total 48.
- **Fixture**: 6 partidos por grupo. Jornada 1 (E1-E4, E2-E3), Jornada 2 (E1-E3, E2-E4), Jornada 3 (E1-E2, E3-E4). Hora de inicio 14:00. Última jornada de cada grupo el mismo día y misma hora. Rotación circular de estadios por orden de nombre normalizado asc: `Partido n → Estadio[(n-1) mod CantEstadios]`.
- **Restricción de grupos**: UEFA puede repetir hasta 2 equipos en un grupo; el resto no.
- **Edición de partidos**: solo fecha, estadio y resultado. Una vez generados los cruces de fase 2, **no se permite editar partidos de fase de grupos**.
- **Orden de clasificación**: Pts → DG → GF → sorteo con `CrossingsSeed`.
- **Cruces fase 2** (32 equipos): 12 primeros + 12 segundos + 8 mejores terceros. Emparejamientos A1..A8 (1ros vs 3ros), B1..B4 (primeros restantes vs segundos peores), B5..B8 (segundos restantes entre sí). Evitar emparejar mismo grupo cuando sea posible.
- **Bracket post-32**: Octavos C1..C8 = ganadores Ai vs Bi; Cuartos D1..D4; Semis S1..S2; Tercer puesto (perdedores de S1/S2); Final.

---

## 8. Auditoría — qué se loggea

Mínimo a registrar con timestamp ISO-8601 y usuario:

- Altas y ediciones de usuarios.
- ABM de estadios y equipos (incluye eliminaciones).
- Generación automática de equipos.
- Generación de fixture.
- Modificación de partidos.
- Sorteo de cruces.
- Importación (éxitos y errores).

Las constantes están en `Services/AuditActions.cs`.

---

## 9. Requerimientos de IA Generativa (sección 3 de la letra)

1. **Gestión de Usuarios (ABM)** debe implementarse con apoyo de IA, **incluyendo tests** y respetando las convenciones del resto del código.
2. **Skills**: usar **al menos una** de las recomendadas (`add-educational-comments`, `create-readme`, `csharp-mstest`, `git-commit`). Están en `.github/skills/`. Documentar uso con un ejemplo concreto.
3. **Importación**: generar **3 implementaciones** con un agente, cada una en su rama:
   - `feature/import-extensible` — priorizar extensibilidad (fácil agregar campos).
   - `feature/import-lowcoupling` — bajo acoplamiento + alta cohesión.
   - `feature/import-readable` — Clean Code (nombres, métodos que hacen una sola cosa, constantes).
   Elegir una justificadamente y mergearla al flujo principal.
4. **Responsabilidad**: el equipo debe entender el código generado. Desconocimiento es motivo de penalización.

`agent.md` y `.github/copilot-instructions.md` ya contienen instrucciones operativas; mantenerlos sincronizados con este archivo.

---

## 10. UI mínima requerida

1. Login.
2. Usuarios (ABM) — sólo Administrador.
3. Equipos + generador automático para llegar a 48.
4. Estadios — sólo Administrador.
5. Fixture y cruces (12 grupos visibles, navegación a grupo, generación de cruces fase 2, bracket que avanza).
6. Listado de partidos con filtros por fecha, estadio, grupo y fase.
7. Edición de partido (fecha, estadio, resultado).
8. Visualización de log.
9. Importar (CSV).

Estado actual de páginas Blazor (`UI/Components/Pages/`): `LogIn`, `Signup`, `EditUser`, `Teams`, `Stadium`, `Fixture`, `Matches`, `LogOut`, `Home`, `Error`. Faltan/incompletas: visualización de log, import, y la vista de cruces / bracket de fase 2.

---

## 11. Documentación PDF (entrega por Gestión)

Estructura esperada (≤ 15 págs. sin anexos):

- Carátula según documento 302 + link al repo.
- Descripción del sistema (problema, audiencia, bugs conocidos).
- Diagramas de paquetes (alto nivel + hojas con notación `paqueteA::paqueteB`) + tabla de responsabilidades por paquete.
- Diagramas de clases del dominio y de servicios (con multiplicidad, navegabilidad, etc.).
- Tabla de responsabilidades por clase.
- Mecanismos generales (UI↔dominio, almacenamiento en memoria, manejo de excepciones, polimorfismo).
- Análisis detallado de "Generación de fixture": clases involucradas, cohesión, acoplamiento.
- Evidencia de TDD para "Generación de fixture": commits con etiquetas RED/GREEN/REFACTOR.
- Reporte dotCover a nivel clase (justificar < 90%).
- Anexos: pruebas funcionales de Usuarios ABM (capturas o videos), explicación del algoritmo de fixture, dependencias externas, análisis crítico del código generado por IA, identificación de la rama de import elegida y justificación.

**La documentación NO va al repo** — sólo por Gestión.

---

## 12. Comandos útiles

```bash
# Restaurar / compilar / testear toda la solución
dotnet restore project/SolutionCopa/SolutionCopa.sln
dotnet build   project/SolutionCopa/SolutionCopa.sln
dotnet test    project/SolutionCopa/SolutionCopa.sln

# Tests por capa
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj
dotnet test project/SolutionCopa/ServiceTest/ServiceTest.csproj
dotnet test project/SolutionCopa/DataAccessTest/DataAccessTest.csproj

# Filtrar un test puntual
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj \
  --filter "FullyQualifiedName~DomainTest.MatchTests.MatchCanGetMatchId"
```

---

## 13. Checklist operativo para agentes

- [ ] Mantener TDD: escribir el test antes (RED), implementación mínima (GREEN), refactor (REFACTOR).
- [ ] No mover reglas de negocio a UI ni Controllers.
- [ ] Conservar nombres públicos existentes (sobre todo en `User` y DTOs); los tests dependen de ellos.
- [ ] Toda operación con semilla debe loggearse vía `AuditLogService` con timestamp ISO-8601.
- [ ] Cada uso de IA en un commit/PR debe quedar referenciado (herramienta + contexto).
- [ ] Las 3 variantes de importación viven en sus ramas (`feature/import-*`), la elegida se mergea con justificación.
- [ ] Las skills usadas están commiteadas en `.github/skills/` con un ejemplo de uso documentado.
- [ ] Antes del cierre: PR a `main`, release + tag, app compilada en una carpeta del repo.
- [ ] PDF subido por Gestión (no por GitHub).

---

## 14. Convenciones que ya están en el repo y conviene preservar

- Excepciones de **dominio** ≠ excepciones de **servicio** (`MatchException` vs `MatchServicesException`).
- Validar primero, asignar después (fail-fast en constructores/mutadores).
- Las clases con factory estática (`MatchGroup`, `MatchElimination`) no se instancian con `new` desde tests; usar los factories o test doubles bajo `*/TestDoubles/`.
- En tests de servicios: fakes/mocks en `ServiceTest/TestDoubles/Mocks/`.
- `Match` es abstracta — heredar (`MockMatch`, `TestMatch`) para tests del comportamiento base.
- Constantes locales y mensajes parametrizables ya se usan dentro de las clases (ver `Team.cs`, `ImportService.cs`); seguir ese patrón en vez de literales sueltos.

---

> **Nota**: si alguna decisión no está cubierta acá, consultar la letra (`Primer Obligatorio Marzo 2026.pdf`), el foro de Aulas y dejar registrada la justificación en el repo. El foro cerró el 08/05/2026.
