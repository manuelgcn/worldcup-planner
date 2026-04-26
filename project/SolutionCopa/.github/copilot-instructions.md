# Repository instructions (Copilot)

> **El contexto completo vive en [`AGENTS.md`](../../../AGENTS.md) en la raíz del repo.** Leer ese archivo antes de proponer cambios. Lo de abajo es un resumen mínimo.

## Reglas no negociables

- Trabajar en C# / .NET 8 sobre `project/SolutionCopa/SolutionCopa.sln`.
- TDD estricto: **test primero**, implementación mínima, refactor con tests verdes. Un commit por cada paso del ciclo, prefijado `RED:` / `GREEN:` / `REFACTOR:`.
- Tests con MSTest. Mocks con Moq (cuando se introduzca como dependencia).
- Naming en español para entidades de dominio (`AgregarEquipo`, `RankingFifa`); naming en inglés para nombres de tests.
- Mensajes de error como `private const string` arriba de la clase.
- Validar reglas en el constructor del dominio; reglas con contexto en `*Services`.
- Determinismo obligatorio: `new Random(semilla)` + Fisher–Yates. Nunca `DateTime.Now` ni `Random` sin semilla en dominio/services.
- Excepciones: `ArgumentException` para input, `InvalidOperationException` para estado/negocio, o excepción custom con primary constructor.
- Cobertura objetivo ≥ 90% por clase.

## Lo que NO hay que hacer

- No `git add .` — stagear sólo archivos creados/modificados deliberadamente.
- No reformatear ni cambiar line-endings de archivos existentes.
- No introducir librerías nuevas sin confirmación.
- No tocar `project/SolutionCopa/TestDomain/` (legacy).
- No saltarse el RED para "ir más rápido".

## Estructura

`Domain` → entidades. `Services` → orquestación en memoria. `DomainTest` → todos los tests unitarios. `Controllers`, `DataAccess`, `UI` → vacíos por ahora.

## Skills del repo

Ver `.github/skills/README.md`. Los más relevantes para el día a día:

- `csharp-mstest` — para escribir tests.
- `git-commit` — para mensajes de commit.
- `add-educational-comments` — sólo reglas no obvias.
- `create-readme` — al actualizar README.
