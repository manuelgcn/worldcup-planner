# Tasks necesarias para implementar MatchService

## Objetivo
Implementar `MatchService` con responsabilidades claras de aplicacion, usando repositorios para persistencia y manteniendo TDD.

## Backlog de implementacion

1. **Definir contrato del servicio**
   - Confirmar/crear `IMatchService` en `Services/Interfaces`.
   - Acordar metodos minimos: crear, actualizar, eliminar, obtener por id, filtros, goles, publicar resultado.

2. **Definir contratos de acceso a datos**
   - Crear/confirmar `IMatchRepository` en `DataAccess/Interfaces`.
   - Agregar interfaces de dependencias (`ITeamRepository`, `IStadiumRepository`, etc.) solo si el caso de uso lo requiere.
   - Definir `IUnitOfWork` para operaciones atomicas (ej: `PublishResultAsync`).

3. **Alinear DTOs del servicio**
   - Crear/confirmar `CreateMatchRequestDto`, `UpdateMatchRequestDto`, `MatchDto`.
   - Evitar exponer entidades de dominio directamente al controlador/UI.

4. **Escribir tests de servicio (RED)**
   - Crear tests MSTest por caso de uso y por regla de negocio.
   - Cubrir caminos felices y errores: validacion, no encontrado, conflicto.

5. **Implementar minimo para pasar tests (GREEN)**
   - Implementar `MatchService` metodo por metodo.
   - Reusar validaciones del dominio y agregar validaciones de aplicacion (existencia, duplicados, conflictos).

6. **Refactor seguro**
   - Eliminar duplicacion, mejorar nombres y extraer helpers privados donde aporte claridad.
   - Mantener comportamiento respaldado por tests.

7. **Wiring en capa superior**
   - Registrar implementaciones en DI.
   - Conectar controladores para consumir `IMatchService`.

8. **Criterios de cierre**
   - Todos los tests del servicio en verde.
   - No hay estado en memoria dentro del servicio (sin lista interna de partidos).
   - Persistencia centralizada en repositorios.

## Nota clave de arquitectura
`MatchService` **no** debe administrar una lista propia de partidos como fuente oficial.  
Debe orquestar reglas y delegar almacenamiento/consulta a `IMatchRepository`.
