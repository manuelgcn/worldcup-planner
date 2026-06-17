# INSTALL — WorldCupPlanner 2026 (Obligatorio 2)

Instalación, ejecución y datos de prueba. La persistencia corre sobre **Azure SQL
Edge** (Docker) con **EF Core 8 Code-First**; los tests usan EF Core In-Memory, así
que **no necesitan la base levantada**.

> ¿Solo querés probar la entrega ya compilada? Usá la carpeta `release-build/` (o el
> ZIP de la Release) y seguí su `README.md`: `start.ps1` levanta todo y `seed.ps1`
> carga la demo. Lo de abajo es para correr desde el código fuente.

## 1. Requisitos

- **.NET 8 SDK** (`dotnet --version` → `8.0.x`).
- **Docker** con `docker compose` (Docker Desktop o Engine).
- Opcional: `dotnet-ef` 8.x solo si vas a generar migraciones a mano
  (`dotnet tool install --global dotnet-ef --version 8.0.*`).

## 2. Levantar la base

```bash
cp .env.example .env          # password por defecto: WorldCup2026!Dev
docker compose up -d sqlserver
docker compose ps             # debe estar "running"
```

La connection string vive en `project/SolutionCopa/UI/appsettings.json`
(`ConnectionStrings:Default`) y coincide con la de `.env`. Para sobrescribirla sin
tocar el archivo, exportá la env `ConnectionStrings__Default`.

## 3. Crear el esquema

La UI aplica las migraciones sola al arrancar, así que **con la base vacía no tenés
que hacer nada más**: pasá al paso 4. Si querés aplicarlas a mano:

```bash
dotnet ef database update \
  --project project/SolutionCopa/DataAccess \
  --startup-project project/SolutionCopa/UI
```

## 4. Ejecutar y testear

```bash
dotnet restore project/SolutionCopa/SolutionCopa.sln
dotnet build   project/SolutionCopa/SolutionCopa.sln
dotnet run     --project project/SolutionCopa/UI/UI.csproj   # levanta la UI
dotnet test    project/SolutionCopa/SolutionCopa.sln         # tests (sin Docker)
```

Abrí la URL que imprime Kestrel (p. ej. `https://localhost:5001`) y entrá con las
credenciales del paso 5.

## 5. Datos de prueba

Al arrancar la UI se crean usuarios básicos: `admin@copa.com` / `Admin@123`,
`editor@copa.com` / `Editor@123`, `periodista@copa.com` / `Periodista@123`.

Para la **demo completa** (2 admin, 2 editores, 2 periodistas, 5 estadios, 48 equipos
con bandera, fase de grupos generada y simulada, auditoría):

```bash
dotnet run --project project/SolutionCopa/DataSeeder
```

Resetea el esquema y vuelve a sembrar todo reutilizando los Services reales. Login
de la demo: `admin1@copa.com` / `Admin@123` (ver el resumen que imprime al terminar).

## 6. Scripts (`.ps1`)

Pensados para Windows + el paquete de Release. Todos validan Docker y leen el
password de `.env`.

| Script | Ubicación | Qué hace |
|---|---|---|
| `start.ps1` | `release-build/` | Levanta Azure SQL Edge, espera a que acepte conexiones y arranca la app self-contained en `http://localhost:5000`. |
| `seed.ps1` | `release-build/` | Restaura la demo desde `db/backups/seed-full.bak` (o `seed-empty.bak` con `seed.ps1 empty`). Requiere la app detenida. |
| `build-release.ps1` | `scripts/` | Genera el asset de la Release: `dotnet publish` self-contained + empaqueta scripts, `docker-compose.yml`, `.env` y `db/` en un ZIP con `.sha256` dentro de `artifacts/`. Uso: `scripts/build-release.ps1 -Version v2.0.0-entrega2`. |

## 7. Correr la Release publicada (macOS / Linux / Windows)

El asset portátil (`WorldCupPlanner-<tag>-portable.zip`) **no es self-contained**:
corre con el **runtime de .NET 8** en cualquier SO, así que necesitás **.NET 8
instalado** y **Docker** para la base.

### Requisitos
- .NET 8 Runtime/SDK (`dotnet --version` → `8.0.x`).
- Docker con `docker compose` (Docker Desktop en macOS/Windows). En Apple Silicon
  la base es **Azure SQL Edge**, que corre nativo en ARM64.

### Pasos
```bash
# 1. Base de datos (Azure SQL Edge)
cp .env.example .env                 # password: WorldCup2026!Dev (igual que appsettings)
docker compose up -d sqlserver
docker compose ps                    # estado "running"

# 2. Ejecutar la app portátil (carpeta del ZIP de la Release)
cd portable
dotnet UI.dll                        # aplica migraciones y crea usuarios al arrancar
```
Abrí la URL que imprime Kestrel (p. ej. `https://localhost:5001`).
Login: `admin@copa.com` / `Admin@123`.

### Demo completa (48 equipos + fixture simulado)
Desde el código fuente: `dotnet run --project project/SolutionCopa/DataSeeder`.

### Si algo falla
- **La app cae con error de conexión** → la base no está arriba: `docker compose up -d sqlserver`.
- **Puerto 1433 ocupado** → liberá el puerto o ajustá `docker-compose.yml` + la connection string.
- **Password no coincide** → `.env (MSSQL_SA_PASSWORD)` debe ser igual al de `appsettings.json`.

## 8. Notas

- **Autosave por operación**: cada repositorio usa un `AppDbContext` efímero vía
  `IDbContextFactory` y persiste en cada `Add/Update/Delete` (no hay "guardar todo").
- Las migraciones viven en `project/SolutionCopa/DataAccess/Migrations/`.
- Alternativas manuales (restaurar `.bak` o correr `db/scripts/seed-*.sql` con
  `sqlcmd`) siguen disponibles si no querés usar el seeder ni los `.ps1`.
