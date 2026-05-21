# INSTALL — WorldCupPlanner 2026

Guía de instalación y ejecución para el Obligatorio 2 de Diseño de Aplicaciones 1.
Cubre el entorno mínimo para levantar la base de datos, compilar la solución y
correr los tests. La capa de persistencia productiva usa **Azure SQL Edge** sobre
Docker; los tests siguen usando el provider en memoria de EF Core (F.I.R.S.T.).

> Fase 0 — *scaffolding*. Las migraciones EF Core, el `AppDbContext` y el
> registro de DI definitivo se incorporan en la Fase 1 (rama
> `feature/db-efcore`).

---

## 1. Prerrequisitos

- **.NET 8 SDK** (probado con `8.0.420`). Verificá con `dotnet --version`.
- **Docker** (Docker Desktop en Windows/macOS o Docker Engine en Linux) con
  soporte para `docker compose`.
- Herramienta CLI **`dotnet-ef`** versión `8.0.x` (necesaria a partir de la
  Fase 1):

  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.*
  # Si ya estaba instalada en otra versión:
  dotnet tool update --global dotnet-ef --version 8.0.*
  ```

- Opcional: cliente SQL (`sqlcmd`, Azure Data Studio, DBeaver, etc.) para
  inspeccionar la base.

---

## 2. Levantar la base (Azure SQL Edge)

El `docker-compose.yml` de la raíz define un único servicio `sqlserver` que
corre **Azure SQL Edge** y publica el puerto `1433`.

1. Copiá el archivo de ejemplo y, si querés, ajustá la contraseña:

   ```bash
   cp .env.example .env
   ```

   La contraseña debe cumplir la complejidad de SQL Server: mínimo 8
   caracteres, con mayúscula, minúscula, número y carácter especial. El
   default es `WorldCup2026!Dev`.

2. Levantá el contenedor en segundo plano:

   ```bash
   docker compose up -d sqlserver
   ```

3. Verificá que esté corriendo:

   ```bash
   docker compose ps
   docker logs worldcup-sqledge --tail 20
   ```

   El log debe terminar con `Recovery is complete.` o equivalente.

4. Para frenarlo (preservando los datos del volumen `mssql-data`):

   ```bash
   docker compose stop sqlserver
   ```

   Para borrarlo y empezar de cero (⚠ destruye datos):

   ```bash
   docker compose down -v
   ```

> `.env` **no** se commitea (está en `.gitignore`). El archivo versionado es
> `.env.example`.

---

## 3. Connection string

La cadena de conexión productiva vive en
`project/SolutionCopa/UI/appsettings.json`, sección `ConnectionStrings:Default`:

```json
"ConnectionStrings": {
  "Default": "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=WorldCup2026!Dev;TrustServerCertificate=True;"
}
```

Para sobrescribirla sin modificar el archivo (recomendado en producción y en
CI), exportá la variable de entorno **`ConnectionStrings__Default`** (dos
guiones bajos):

```bash
# bash / zsh
export ConnectionStrings__Default="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"

# PowerShell
$env:ConnectionStrings__Default = "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
```

> La password del `appsettings.json` de dev coincide con la de `.env.example`
> únicamente por comodidad local. **No** se debe usar ese password en
> ambientes productivos.

---

## 4. Migraciones EF Core (pendiente de Fase 1)

Los comandos quedan documentados para que el equipo los tenga a mano cuando
la Fase 1 introduzca `AppDbContext`. Hoy todavía no hay migraciones para
aplicar.

```bash
# Crear la migración inicial (cuando AppDbContext exista)
dotnet ef migrations add Initial \
  --project project/SolutionCopa/DataAccess \
  --startup-project project/SolutionCopa/UI

# Aplicar/actualizar el esquema contra la base productiva
dotnet ef database update \
  --project project/SolutionCopa/DataAccess \
  --startup-project project/SolutionCopa/UI
```

---

## 5. Build, test y ejecución

Todo se opera contra la solución `project/SolutionCopa/SolutionCopa.sln`.

```bash
# Restaurar dependencias
dotnet restore project/SolutionCopa/SolutionCopa.sln

# Compilar
dotnet build project/SolutionCopa/SolutionCopa.sln

# Correr todos los tests (Domain, Services, DataAccess)
dotnet test project/SolutionCopa/SolutionCopa.sln

# Levantar la UI Blazor Server (startup project)
dotnet run --project project/SolutionCopa/UI/UI.csproj
```

Filtros útiles:

```bash
dotnet test project/SolutionCopa/DomainTest/DomainTest.csproj
dotnet test project/SolutionCopa/ServiceTest/ServiceTest.csproj
dotnet test project/SolutionCopa/DataAccessTest/DataAccessTest.csproj
```

---

## 6. Nota de diseño para Fase 1 (recordatorio)

En Fase 1, junto con la introducción de EF Core, hay dos cambios estructurales
de DI que **todavía no están aplicados** y que el equipo debe tener presentes:

- `AppDbContext` se registra con `AddDbContext<AppDbContext>(...)` y queda
  **scoped por request**.
- Los repositorios y `IUnitOfWork`, hoy `AddSingleton` en
  `UI/Program.cs`, deben migrar a **`AddScoped`** para acompañar el ciclo de
  vida del `DbContext` (que no es thread-safe).
- `IUnitOfWork.Commit()` deja de ser un no-op y pasa a envolver
  `SaveChangesAsync()` — **autosave por operación**, sin botón "guardar todo".

Mientras tanto, el código productivo sigue usando los repositorios en memoria
de la Fase 0 / Obligatorio 1.
