# WorldCupPlanner 2026 — Build de Entrega 1

Build self-contained para Windows x64. **No requiere instalar .NET 8** — todo
está incluido.

## Cómo correrlo

1. Entrar a la carpeta `win-x64/`.
2. Doble click en `UI.exe` (o desde cmd: `UI.exe`).
3. Abrir el browser en la URL que muestra la consola (por defecto
   `http://localhost:5000`).

## Credenciales seed

| Rol    | Email             | Password   |
|--------|-------------------|------------|
| Admin  | admin@copa.com    | Admin@123  |
| Editor | editor@copa.com   | Editor@123 |

## Configuración

Las semillas (FixtureSeed, CrossingsSeed, SimulationSeed, CompletionSeed) y
parámetros del torneo se leen de `win-x64/appsettings.json`, sección
`TournamentSettings`. Cambiar valores y reiniciar para verlo aplicado.

## Versión

- Tag: `v1.0.1-entrega1`
- Build: self-contained `win-x64`, .NET 8.0

### Cambios respecto a `v1.0.0-entrega1`

- `fix(fixture)`: la generación de fixture ahora respeta los cupos de
  confederación por grupo (máx. 1 no-UEFA, máx. 2 UEFA) usando Fisher-Yates
  determinístico sobre `FixtureSeed`.
- `fix(matches)`: los filtros de la página de partidos funcionan de forma
  independiente, "Hasta" incluye el mismo día y la lista incluye los partidos
  de eliminación.
- `feat(bracket)`: en `/fixture`, los partidos de eliminación que se definen
  por penales muestran el resultado de la tanda y el equipo que pasó.
