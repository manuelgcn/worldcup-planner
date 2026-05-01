# Ideas basicas para una clase de Servicios (enfocada en Partido)

Una clase de servicio de `Partido` coordina casos de uso de la aplicacion. No es UI ni persistencia: orquesta reglas de negocio y repositorios.

## 1. Responsabilidad principal

- Recibir una solicitud (crear, actualizar, consultar, publicar resultado).
- Validar reglas de negocio antes de guardar cambios.
- Invocar repositorios para leer/escribir datos.
- Devolver DTOs simples al controlador/UI.

## 2. Que no deberia hacer

- No acceder directo a base de datos (eso va en DataAccess).
- No mezclar logica de presentacion (mensajes de pantalla, formato UI).
- No exponer entidades de dominio directamente si hay contrato por DTO.

## 3. Casos de uso utiles para Partido

- `CreateMatchAsync`: crear partido de grupo o eliminacion.
- `UpdateMatchAsync`: editar fecha, estadio o equipos permitidos.
- `GetByIdAsync` y filtros (`GetByGroupAsync`, `GetByJourneyAsync`, `GetByDateRangeAsync`).
- `SetGoalsAsync`, `AddGoalAsync`, `RemoveGoalAsync`.
- `PublishResultAsync`: cerrar resultado y avanzar ganador si aplica.

## 4. Reglas de negocio tipicas a cubrir

- Local y visitante no pueden ser el mismo equipo.
- Estadio obligatorio.
- Fecha valida dentro del campeonato.
- Goles no negativos.
- No quitar gol si el marcador ya esta en 0.
- En eliminacion: fase valida, codigo no vacio y partidos fuente correctos.

## 5. Estructura recomendada (simple)

1. Validar input y reglas.
2. Consultar dependencias (equipos, grupo, estadio, etc.).
3. Ejecutar accion de dominio.
4. Persistir cambios (repositorio + unit of work si corresponde).
5. Mapear a DTO de salida.

## 6. Testing que te conviene (TDD)

- **RED**: crear test por regla/caso de uso.
- **GREEN**: implementar lo minimo para pasar.
- **REFACTOR**: limpiar sin cambiar comportamiento.

Para `Partido`, prioriza:
- creacion correcta e invalida,
- actualizacion y no encontrado,
- gestion de goles (altas/bajas/limites),
- publicacion de resultado y conflictos de llave.

## 7. Idea clave para que te sirva en Partido

Si mantienes el servicio como "orquestador de reglas + repositorios", vas a tener:
- codigo mas testeable,
- menos acoplamiento con UI/DataAccess,
- y cambios mas seguros cuando aparezcan nuevas reglas del torneo.
