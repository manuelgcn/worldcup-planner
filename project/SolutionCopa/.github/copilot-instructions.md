# Repository instructions

- Trabajar en C#/.NET sobre la solución `SolutionCopa`.
- Respetar TDD: primero tests, luego implementación mínima, luego refactor.
- Hacer cambios mínimos y seguros.
- Mantener el código legible y alineado con Clean Code.
- Usar MSTest para las pruebas nuevas.
- Nombrar tests en inglés con intención clara.
- No cambiar comportamiento sin una regla de negocio o test que lo justifique.
- En entidades de dominio, validar reglas de negocio de forma explícita.
- Preferir propiedades inmutables cuando sea posible.
- Antes de refactorizar, asegurar que los tests existentes sigan pasando.
- Si se agrega una regla nueva, crear primero el test que la exprese.
- Respetar la estructura actual del proyecto: `Domain`, `DataAccess`, `Services`, `Controllers`, `UI` y `TestDomain`.

