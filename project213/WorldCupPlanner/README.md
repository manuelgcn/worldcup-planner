# WorldCupPlanner

Base del obligatorio de DA1 con arquitectura por capas en .NET 8 y tests con MSTest.

## Estructura

- `src/WorldCupPlanner.Web`: UI Blazor Server.
- `src/WorldCupPlanner.Domain`: reglas de negocio puras.
- `src/WorldCupPlanner.Application`: casos de uso.
- `src/WorldCupPlanner.Infrastructure`: implementaciones técnicas.
- `src/WorldCupPlanner.Contracts`: contratos/DTOs compartidos.
- `tests/*`: proyectos de tests MSTest.

## Comandos rápidos

```powershell
dotnet restore "project213/WorldCupPlanner/WorldCupPlanner.sln"
dotnet build "project213/WorldCupPlanner/WorldCupPlanner.sln" --configuration Release
dotnet test "project213/WorldCupPlanner/WorldCupPlanner.sln" --configuration Release
```

## CI

El workflow `/.github/workflows/mstest-main.yml` ejecuta restore, build y MSTest en:

- cada `push` a `main` (incluye merges),
- cada `pull_request` hacia `main`,
- ejecución manual (`workflow_dispatch`).

