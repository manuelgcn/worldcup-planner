# Agent instructions for "Obligatorio I — WorldCupPlanner 2026"

Resumen rápido
- Proyecto: WorldCupPlanner 2026 — administrar fase de grupos (12 grupos x 4 equipos), generar fixture determinístico y preparar cruces eliminatorios reproducibles.
- Entregable: aplicación en memoria (C# .NET 8, Blazor Server), pruebas MSTest (TDD), documentación PDF enviada por Gestión; código y compilado en GitHub (release/tag en main).

Parámetros configurables (usar en appsettings.json o config):
- SemillaFixture
- SemillaCompletar
- SemillaCrucesFase
- SemillaSimulation
- FechaInicioTorneo (default: 2026-06-01)
- MaxPartidosPorDia (default: 3)
- SeparacionEntreFechas (default: 3)

Reglas clave — resumen operativo
- Tecnología: C#/.NET 8 (Blazor Server App). Repositorio con gitflow.
- TDD obligatorio para todas las clases (no UI). Tests en MSTest; evidenciar RED/GREEN/REFACTOR con commits (un commit por test).
- Fisher–Yates determinístico para desempates y barajados usando System.Random(semilla).
- Normalización: lowercase, sin tildes, eliminar no-alfa numéricos → espacios colapsados, trim.
- Auditoría: registrar timestamp ISO-8601, usuario y acción para todas las operaciones importantes (altas/ediciones/eliminaciones, generadores, sorteos, imports, cambios de partido).

Funcionalidad mínima (prioritaria para agentes)
1) Gestión de Usuarios (ABM)
   - Campos: nombre, apellido, email, fechaNacimiento (MM-DD-AAAA), password.
   - Requisitos de contraseña: ≥8 chars, mayúscula, minúscula, número, carácter especial; persistir cifrada (do not store plain text).
   - Roles: Administrador, Editor (no excluyentes).
   - IA: Este requisito puede implementarse usando IA; incluir tests y documentar herramientas y contexto de uso.

2) Equipos (CRUD) — validaciones
   - Nombre (1–60, único), Confederación (AFC, CAF, CONCACAF, CONMEBOL, OFC, UEFA), RankingFIFA (300–2500 int).
   - Cupos por confederación (UEFA 16, CONMEBOL 7, CONCACAF 7, CAF 9, AFC 8, OFC 1).
   - Generar hasta 48 equipos con SemillaCompletar (nombres determinísticos: UEFA_01, ...). Auditar cantidad y semilla.

3) Estadios (CRUD)
   - Nombre (1–80 único), Ciudad (1–60), Descripción opcional (1–400), Capacidad ≥20000.

4) Generación de fixture (fase de grupos)
   - Requerimiento previo: 48 equipos y ≥4 estadios.
   - Ordenar equipos por RankingFIFA DESC; desempates con Fisher–Yates (SemillaFixture).
   - 4 bombos de 12 equipos; distribuir A→L en round-robin.
   - Calendario: 6 partidos por grupo (J1,J2,J3), fechas separadas por SeparacionEntreFechas días a partir de FechaInicioTorneo; hora inicio 14:00; máximo MaxPartidosPorDia por día; rotación circular de estadios (orden por nombre normalizado asc).

5) Cruces eliminatorios (Round of 32)
   - Orden por Pts, DG, GF; desempate final por sorteo con SemillaCrucesFase.
   - Seleccionar A1..A12, B1..B12, mejores 8 terceros, aplicar reglas de emparejamiento evitando mismo grupo cuando sea posible.
   - Barajar listas con Fisher–Yates (SemillaCrucesFase).

6) Importación CSV
   - Encabezados exactos: Nombre, Confederación, RankingFIFA. Validar unicidad, confederación y rango.
   - Generar 3 implementaciones (por agente) en ramas separadas: prioritizar (a) Extensibilidad, (b) Bajo acoplamiento/alta cohesión, (c) Facilidad de comprensión. Nombres de ramas sugeridos:
     - feature/import-extensible
     - feature/import-lowcoupling
     - feature/import-readable

7) Simulación de resultados
   - Basada en RankingActual y SemillaSimulation para reproducibilidad.

8) Auditoría y logs
   - Registrar cada uso de semilla, timestamp ISO-8601 y usuario; registrar generación de equipos, fixture, sorteos, imports y cambios de partidos.

IA generativa — reglas para agentes
- Documentar todas las herramientas de IAG usadas y contexto (p. ej. "GitHub Copilot - generación de código de usuarios (tests incluidos)").
- Incluir en el repositorio: skills o archivos de instrucciones utilizados (links y ejemplo de uso). Requerido usar al menos una skill recomendada (Add Educational Comments, Create-readme, csharp-mstest, git-commit) — prefer csharp-mstest para código generado.
- El código generado por IA debe revisarse, refactorizarse y cubrirse con tests. El equipo es responsable del resultado.
- Para la importación, generar y documentar 3 alternativas con ramas y justificar la seleccionada.

Entregables (por equipo)
- Documentación PDF (<=15 páginas, sin anexos): carátula, descripción, diagramas de paquetes y clases, tabla de responsabilidades, TDD commits (RED/GREEN/REFACTOR), cobertura dotCover, anexos con pruebas funcionales (screenshots/videos links). Entregar PDF via Gestión (no en repo).
- Repositorio Git en organización IngSoft-DA1:
  - Código fuente y tests (merge a main), carpeta con la app compilada, Release y Tag en main antes de la fecha/hora límite.

Comandos útiles
- dotnet restore project/SolutionCopa/SolutionCopa.sln
- dotnet build project/SolutionCopa/SolutionCopa.sln
- dotnet test project/SolutionCopa/SolutionCopa.sln

Checklist rápido para agentes
- [ ] Implementar/validar parámetros de semilla y config.
- [ ] Implementar tests MSTest para todas las entidades y servicios (TDD).
- [ ] Instrumentar logging/auditoría con timestamp ISO-8601 y usuario en cada operación crítica.
- [ ] Generar 3 ramas para import alternativas y documentarlas.
- [ ] Añadir skills/instructions files al repo y documentar su uso en README o agent.md.
- [ ] Crear Release + Tag en main antes del cierre y asegurarse que PDF se sube por Gestión.

Notas finales
- Defensa presencial entre 15/05/2026 y 04/06/2026; no presentación implica pérdida total de puntos.
- Repo name must follow: NumeroEstudiante1_NumeroEstudiante2_NumeroEstudiante3

---
Agent: use this file as canonical summary of the obligatorio. For any implementation decision not covered here, consult the course forums and document the rationale in the repository.
