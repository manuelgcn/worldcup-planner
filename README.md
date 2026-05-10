# WorldCupPlanner 2026

WorldCupPlanner 2026 es una solucion .NET 8 por capas para modelar y administrar un torneo mundial: usuarios, equipos, estadios, grupos, partidos, fixture, cruces y auditoria.

## Proposito

El objetivo del proyecto es mantener la logica de negocio en el dominio y la orquestacion en servicios, con reglas explicitas y tests MSTest. La solucion busca ser deterministica en todo lo que dependa de semillas, y fail-fast ante datos invalidos.

## Estructura de la solucion

| Capa | Proyecto | Rol |
|---|---|---|
| Dominio | `Domain` | Entidades, invariantes, enums y excepciones del negocio |
| Servicios | `Services` | Casos de uso, coordinacion, DTOs y auditoria |
| Datos | `DataAccess` | Contratos de repositorio y unidad de trabajo |
| UI / Controladores | `UI`, `Controllers` | Puntos de entrada y adaptadores |
| Pruebas | `DomainTest`, `ServiceTest`, `DataAccessTest` | Validacion con MSTest |

Namespaces principales:

- `ProjectCopa` en dominio.
- `Services` en servicios.
- `DataAccess` en acceso a datos.
- `DomainTest` y `ServiceTest` en pruebas.

## Clases principales del dominio

### `User`

Representa un usuario del sistema.

Condiciones:

- `email` obligatorio y con formato valido basico.
- `password` con minimo 8 caracteres, mayuscula, numero y caracter especial.
- La contrasena se guarda cifrada con SHA256, no en texto plano.
- Permite asignar roles no excluyentes.

Propiedades relevantes:

- `idUser`
- `name`
- `lastname`
- `email`
- `password`
- `dateOfBirth`
- `Roles`

### `Team`

Representa una seleccion/equipo del torneo.

Condiciones:

- Nombre obligatorio.
- Longitud maxima: 60 caracteres.
- Ranking FIFA entre 300 y 2500.
- Se asocia a una `Confederation`.

### `Stadium`

Representa un estadio del torneo.

Condiciones:

- `Name` obligatorio y maximo 80 caracteres.
- `City` obligatoria y maximo 60 caracteres.
- `Capacity` minimo 20000.

### `Group`

Representa un grupo del torneo.

Condiciones:

- `Name` obligatorio.
- Maximo 4 equipos por grupo.

### `Journey`

Representa una jornada/fecha del fixture de grupos.

### `TournamentSettings`

Centraliza la configuracion deterministica del torneo.

Propiedades:

- `Id`
- `StartDate`
- `MaxMatchesPerDay`
- `DaysBetweenJourneys`
- `FixtureSeed`
- `CrossingsSeed`
- `SimulationSeed`
- `CompletionSeed`

Condiciones:

- `MaxMatchesPerDay` entre 1 y 3.
- `DaysBetweenJourneys` mayor o igual a 1.
- Valores por defecto:
  - inicio: `2026-06-01`
  - max por dia: `3`
  - separacion entre jornadas: `3`
  - semillas: `0`

### `Match`

Clase abstracta base para cualquier partido.

Responsabilidades:

- Validar fecha contra el inicio del torneo.
- Mantener equipos, goles, estadio y ganador.
- Permitir cambio de estadio y manejo de goles.

Condiciones:

- Home team y away team no pueden ser el mismo.
- El estadio es obligatorio.
- Los goles no pueden ser negativos.
- No se puede quitar un gol si el marcador esta en cero.

Metodos relevantes:

- `ChangeStadium`
- `SetGoals`
- `SetHomeGoals`
- `SetAwayGoals`
- `AddHomeGoal`
- `AddAwayGoal`
- `RemoveHomeGoal`
- `RemoveAwayGoal`
- `GetWinner`

### `MatchGroup`

Especializa `Match` para fase de grupos.

Agrega:

- `Group`
- `Journey`

### `MatchElimination`

Especializa `Match` para fase eliminatoria.

Agrega:

- `Phase`
- `MatchCode`
- `MatchHome`
- `MatchAway`

Condiciones:

- `Phase` valida.
- `MatchCode` obligatorio.
- Los partidos fuente no pueden ser nulos.
- Los partidos fuente no pueden ser el mismo objeto.

### `AuditLog`

Entidad de auditoria del backend.

Campos:

- `Timestamp`
- `Action`
- `Details`
- `TournamentSettingsId`
- `AffectedEntityType`
- `AffectedEntityId`

Se usa para registrar acciones de negocio y cambios de estado.

## Capa de servicios

### `UserServices`

Gestiona altas, bajas, cambios y roles de usuarios.

Reglas:

- No permite emails duplicados.
- No permite ids duplicados.
- Expone busqueda por email e id.
- Dispara auditoria en operaciones mutantes cuando el logger esta inyectado.

### `TeamServices`

Gestiona equipos del sistema.

Reglas:

- Nombre unico.
- Cupos por confederacion.
- Busqueda y eliminacion por nombre.
- Orden deterministico para fixture con Fisher-Yates y semilla.
- Exige exactamente 48 equipos para ordenar fixture.

### `StadiumServices`

Gestiona estadios.

Reglas:

- Nombre unico.
- Alta, baja, consulta y actualizacion.
- Dispara auditoria en mutaciones.

### `FixtureService`

Construye el fixture de grupos.

Reglas:

- Exige 48 equipos.
- Exige al menos 4 estadios.
- Crea 12 grupos: `A` a `L`.
- Ordena equipos por ranking FIFA.
- Rompe empates de forma deterministica usando semilla.
- Distribuye 4 bombos de 12 equipos.

### `MatchFactory`

Fabrica instancias de partidos segun el tipo.

Responsabilidades:

- Crear partidos de grupo o eliminatoria.
- Resolver equipos, estadio, grupo, jornada y configuracion del torneo.
- Aplicar cambios de estadio en actualizaciones.

### `MatchService`

Orquesta el ciclo de vida de los partidos.

Casos de uso:

- crear partido
- actualizar partido
- eliminar partido
- agregar gol
- quitar gol
- fijar marcador
- publicar resultado
- simular partido
- buscar por id, grupo, jornada o rango de fechas

Tambien registra auditoria de las operaciones mutantes y usa la configuracion deterministica del torneo.

### `AuditLogService`

Guarda y exporta registros de auditoria.

Funcionalidades:

- crear logs
- listar todos
- filtrar por `TournamentSettingsId`
- exportar a texto

### `AuditActions`

Constantes de acciones de auditoria para evitar strings sueltos.

## Auditoria en backend

La auditoria backend existe y cubre varios flujos de mutacion:

- usuarios
- equipos
- estadios
- fixture
- partidos

Cada log guarda accion, detalle, entidad afectada y contexto del torneo. Parte del flujo sigue dependiendo de un contexto por defecto para la configuracion del torneo, por lo que la cobertura es funcional pero todavia no uniforme en todos los caminos.

## Reglas y condiciones clave

- El dominio usa excepciones especificas para reglas de negocio.
- Los servicios usan validaciones explicitas y fallan rapido ante datos invalidos.
- El ordenamiento y simulacion usan semillas para reproducibilidad.
- Las pruebas deben ser deterministicas.
- La UI no debe contener logica de negocio.

## Pruebas

La solucion usa MSTest.

Comandos:

```bash
dotnet restore project/SolutionCopa/SolutionCopa.sln
dotnet build project/SolutionCopa/SolutionCopa.sln
dotnet test project/SolutionCopa/SolutionCopa.sln
```

Pruebas focalizadas:

```bash
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj
dotnet test project/SolutionCopa/ServiceTest/ServiceTest.csproj
dotnet test project/SolutionCopa/DataAccessTest/DataAccessTest.csproj
```

## Estado actual

- El dominio principal esta modelado.
- Los servicios base estan implementados.
- La capa de auditoria existe y ya registra parte de las mutaciones.
- UI y controladores siguen siendo superficiales/scaffold en el estado actual.

## Recomendaciones de contribucion

- Mantener TDD: red, green, refactor.
- Conservar nombres y contratos existentes.
- No mover reglas de negocio a UI ni controladores.
- Preferir constantes y helpers compartidos antes que literales repetidos.
