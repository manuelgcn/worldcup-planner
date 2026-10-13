---
name: audit-logger-instrumentation
description: Instrument new use cases with consistent audit logging via AuditLogService/AuditActions, with ISO-8601 timestamp and actor, the way this repo does it.
---

# Audit Logger Instrumentation

Skill nuevo (no usado en el Obligatorio 1) para que cada caso de uso nuevo quede
auditado de forma consistente. La auditoría es **global del sistema** (no atada al
torneo) y debe registrar **toda mutación relevante, uso de semilla y simulación**.

## API del repo (usar tal cual)

`src/WorldCupPlanner.Services/Interfaces/IAuditLogService.cs`:

- `LogPerformedBy(action, details, performedByEmail, affectedEntityType?, affectedEntityId?)`
  — **preferido**: guarda el actor en `AuditLog.PerformedByEmail` (no lo concatenes
  en `details`).
- `Log(action, details, ...)` — log global sin contexto de torneo.
- `Log(action, details, tournamentSettingsId, ...)` — solo cuando el torneo aporta
  contexto real.
- `GetFiltered(AuditLogFilterDto)` — consulta filtrada por fecha/acción/usuario,
  **ejecutada en SQL** (`IQueryable`); no traigas todo y filtres en memoria.

## Procedimiento para instrumentar un caso de uso

1. **Definí la acción** como constante en `src/WorldCupPlanner.Services/AuditActions.cs`, formato
   `SCREAMING_SNAKE` (`RANKING_UPDATED`, `SIMULATION_EXECUTED`, `EXPORT_FIXTURE_CSV`,
   `NOTIFICATION_READ`). No uses literales sueltos en los servicios.
2. **Logueá en el punto de cierre del caso de uso** (el método del servicio que es
   "controlador" del caso de uso), después de que la operación tuvo éxito.
3. Pasá el **actor** (`performedByEmail`) y un `details` claro y parametrizable.
   Para cambios de valor, registrá `anterior → nuevo` (ej. ranking Elo: una entrada
   por equipo).
4. Para **semillas y simulaciones**, incluí `engineName` + `seed` en `details`.
5. Timestamp ISO-8601 lo pone el servicio: no formatees fechas a mano ni uses
   `DateTime.Now` en dominio/servicios fuera de este punto.

## Qué se audita (mínimo)

ABM de usuarios/equipos/estadios, generación automática de equipos, generación de
fixture, modificación de partidos, sorteo de cruces, importación (éxitos y errores),
cambios de ranking, simulaciones (semilla), exportaciones y lectura de notificaciones.

## TDD

Test primero: el test verifica que tras el caso de uso existe **una entrada** con la
`action` correcta, el actor esperado y el `affectedEntityId` correcto. Usá el fake de
auditoría de `tests/WorldCupPlanner.Services.Tests/TestDoubles/` en vez de la implementación real.

## Verificación

- La acción nueva está en `AuditActions.cs` y se usa por nombre, no por literal.
- El log incluye actor (`PerformedByEmail`) y timestamp ISO-8601.
- Las consultas de la vista/exportación pasan por `GetFiltered`, no filtran en memoria.

## Ejemplo de uso documentado

Fase 7: al desacoplar el log del torneo se agregaron `SIMULATION_EXECUTED` (registra
motor + semilla) y `RANKING_UPDATED` (una entrada por equipo con `anterior → nuevo`),
y la vista de auditoría pasó a consumir `GetFiltered` con filtros en SQL.
