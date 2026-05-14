# WorldCupPlanner 2026

## Ejecutar la release

Build self-contained para Windows x64. **No requiere instalar .NET 8.**

1. Entrar a la carpeta `release-build/win-x64/`.
2. Doble click en `UI.exe` (o desde cmd: `UI.exe`).
3. Abrir el browser en la URL que muestra la consola (por defecto `http://localhost:5000`).

### Credenciales seed

| Rol    | Email           | Password   |
|--------|-----------------|------------|
| Admin  | admin@copa.com  | Admin@123  |
| Editor | editor@copa.com | Editor@123 |

### Configuracion

Las semillas y parametros del torneo se leen de `release-build/win-x64/appsettings.json`, seccion `TournamentSettings`. Cambiar valores y reiniciar para verlos aplicados.

### Version

- Tag: `v1.0.1-entrega1`
- Build: self-contained `win-x64`, .NET 8.0

**Cambios respecto a `v1.0.0-entrega1`:**

- `fix(fixture)`: la generacion de fixture respeta los cupos de confederacion por grupo (max. 1 no-UEFA, max. 2 UEFA) con Fisher-Yates deterministico sobre `FixtureSeed`.
- `fix(matches)`: los filtros de partidos funcionan de forma independiente, "Hasta" incluye el mismo dia y la lista incluye partidos de eliminacion.
- `feat(bracket)`: en `/fixture`, los partidos que se definen por penales muestran el resultado de la tanda y el equipo que paso.

---

## Resumen del repositorio

WorldCupPlanner 2026 es una solucion .NET 8 por capas para gestionar un torneo mundial de 48 equipos: usuarios, equipos, estadios, grupos, partidos, fixture de grupos y cruces eliminatorios. La logica de negocio vive en el dominio; los servicios orquestan; la UI solo presenta.

### Estructura de la solucion

| Capa | Proyecto | Rol |
|---|---|---|
| Dominio | `Domain` | Entidades, invariantes, enums y excepciones de negocio |
| Servicios | `Services` | Casos de uso, coordinacion, DTOs y auditoria |
| Datos | `DataAccess` | Repositorios en memoria e `IUnitOfWork` |
| UI | `UI` | Blazor Server, solo presentacion |
| Pruebas | `DomainTest`, `ServiceTest`, `DataAccessTest` | MSTest |

### Reglas clave

- Ordenamiento y simulacion deterministicos con semillas (`FixtureSeed`, `CompletionSeed`, `CrossingsSeed`, `SimulationSeed`).
- Cupos por confederacion: UEFA 16, CONMEBOL 7, CONCACAF 7, CAF 9, AFC 8, OFC 1.
- Fixture de 12 grupos × 6 partidos con rotacion de estadios por nombre normalizado.
- Clasificacion: Pts → DG → GF → sorteo con `CrossingsSeed`.
- Toda operacion con semilla queda registrada en el log de auditoria (timestamp ISO-8601 + usuario).

### Compilar y testear

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

### Contribucion

- TDD: red → green → refactor. Un commit por ciclo en el flujo de fixture.
- No mover logica de negocio a UI ni Controllers.
- Conservar nombres publicos existentes (en especial en `User` y DTOs).
- Usar constantes y helpers compartidos; no literales sueltos.
