# INSTALL — WorldCupPlanner 2026 (Obligatorio 2)

Guía de instalación, ejecución y carga de datos de prueba. La persistencia
productiva corre sobre **Azure SQL Edge** en Docker con **EF Core 8 (Code
First)**; los tests unitarios usan el provider **EF Core In-Memory**
(F.I.R.S.T.), por lo que no requieren la base levantada.

Resumen de artefactos relevantes:

| Artefacto | Ubicación |
|---|---|
| Solución | `project/SolutionCopa/SolutionCopa.sln` |
| Proyecto de arranque (UI Blazor Server) | `project/SolutionCopa/UI/UI.csproj` |
| Seeder de datos de prueba (consola) | `project/SolutionCopa/DataSeeder/DataSeeder.csproj` |
| Connection string | `project/SolutionCopa/UI/appsettings.json` → `ConnectionStrings:Default` |
| Compose de la base | `docker-compose.yml` (servicio `sqlserver`, contenedor `worldcup-sqledge`) |
| Script de esquema (idempotente) | `db/scripts/seed-empty.sql` |
| Script de esquema + datos | `db/scripts/seed-full.sql` |
| Backup base vacía | `db/backups/seed-empty.bak` |
| Backup base con datos de prueba | `db/backups/seed-full.bak` |

---

## 1. Prerrequisitos

- **.NET 8 SDK** (`dotnet --version` → `8.0.x`). La solución corre con .NET Core 8.
- **Docker** con `docker compose` (Docker Desktop o Docker Engine).
- Herramienta CLI **`dotnet-ef` 8.x** (solo si vas a generar/aplicar migraciones
  manualmente):

  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.*
  ```

- Opcional: cliente SQL (`sqlcmd`, Azure Data Studio, DBeaver) para inspeccionar
  la base. `sqlcmd` ya viene dentro del contenedor en `/opt/mssql-tools/bin/sqlcmd`.

---

## 2. Levantar la base (Azure SQL Edge)

1. Copiá el archivo de entorno y, si querés, ajustá la contraseña (debe cumplir
   la complejidad de SQL Server: ≥8 caracteres con mayúscula, minúscula, número
   y símbolo). El default es `WorldCup2026!Dev`:

   ```bash
   cp .env.example .env
   ```

2. Levantá el contenedor:

   ```bash
   docker compose up -d sqlserver
   ```

3. Verificá que esté arriba:

   ```bash
   docker compose ps
   docker logs worldcup-sqledge --tail 20   # debe decir "Recovery is complete."
   ```

`.env` no se commitea (está en `.gitignore`); el versionado es `.env.example`.
Para frenar preservando datos: `docker compose stop sqlserver`. Para borrar todo
(⚠ destruye el volumen): `docker compose down -v`.

---

## 3. Connection string

Definida en `project/SolutionCopa/UI/appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=WorldCup2026!Dev;TrustServerCertificate=True;"
}
```

Para sobrescribirla sin tocar el archivo, exportá la variable de entorno
**`ConnectionStrings__Default`** (doble guion bajo):

```bash
# bash / zsh
export ConnectionStrings__Default="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
# PowerShell
$env:ConnectionStrings__Default = "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
```

> El password de `appsettings.json` coincide con el de `.env.example` solo por
> comodidad local; no usar ese valor en producción.

---

## 4. Crear el esquema

Hay tres caminos equivalentes; **elegí uno**.

### Opción A — Migraciones EF Core (recomendado para desarrollo)

La UI aplica `Database.Migrate()` al arrancar, así que con la base vacía y la
connection string apuntando bien, basta con ejecutar la app (§6). Para aplicarlas
a mano:

```bash
dotnet ef database update \
  --project project/SolutionCopa/DataAccess \
  --startup-project project/SolutionCopa/UI
```

### Opción B — Script de esquema idempotente

`db/scripts/seed-empty.sql` contiene todas las migraciones (idempotentes, con
control en `__EFMigrationsHistory`). Crear la base y aplicarlo:

```bash
docker exec worldcup-sqledge /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'WorldCup2026!Dev' -C \
  -Q "IF DB_ID('WorldCupPlanner') IS NULL CREATE DATABASE [WorldCupPlanner];"

docker cp db/scripts/seed-empty.sql worldcup-sqledge:/tmp/seed-empty.sql
docker exec worldcup-sqledge /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'WorldCup2026!Dev' -C -d WorldCupPlanner -i /tmp/seed-empty.sql
```

### Opción C — Restaurar el backup vacío

`db/backups/seed-empty.bak` es la base recién migrada, sin datos (ver §5.3 para
la mecánica de restore; usar `seed-empty.bak` en lugar de `seed-full.bak`).

---

## 5. Cargar los datos de prueba

La base de datos de demo incluye, según pide la letra:

- **2 administradores, 2 editores, 2 periodistas.**
- **5 estadios.**
- **48 equipos**, de los cuales **46 tienen bandera** (PNG reales → base64).
- **1 torneo** con `TournamentSettings`, **fase de grupos generada y simulada**
  (72 partidos) con el motor *Probabilístico*.
- **Entradas de auditoría** coherentes (alta de usuarios, generación de fixture,
  simulación, recálculo de ranking Elo, notificaciones).

Credenciales sembradas (todas cumplen la política de contraseñas):

| Rol | Email | Password |
|---|---|---|
| Admin | `admin1@copa.com` | `Admin@123` |
| Admin | `admin2@copa.com` | `Admin@456` |
| Editor | `editor1@copa.com` | `Editor@123` |
| Editor | `editor2@copa.com` | `Editor@456` |
| Periodista | `periodista1@copa.com` | `Prensa@123` |
| Periodista | `periodista2@copa.com` | `Prensa@456` |

> Además, al arrancar la UI (§6) se crean, si no existen, tres usuarios de
> conveniencia: `admin@copa.com` / `Admin@123`, `editor@copa.com` / `Editor@123`
> y `periodista@copa.com` / `Periodista@123`.

Elegí **uno** de los tres caminos siguientes.

### 5.1 Opción A — Seeder reproducible (recomendado)

Proyecto de consola externo al código productivo que reutiliza los Services
reales (validación de dominio, generación de fixture y simulación verdaderas).
Resetea el esquema (`EnsureDeleted` + `Migrate`) y vuelve a sembrar todo:

```bash
# Usa la connection string de desarrollo por defecto:
dotnet run --project project/SolutionCopa/DataSeeder

# O pasando una cadena explícita:
dotnet run --project project/SolutionCopa/DataSeeder -- \
  "Server=localhost,1433;Database=WorldCupPlanner;User Id=sa;Password=WorldCup2026!Dev;TrustServerCertificate=True;"
```

Al terminar imprime un resumen con los conteos sembrados. Es la fuente desde la
que se regeneran `seed-full.sql` y `seed-full.bak`.

### 5.2 Opción B — Script SQL con datos

`db/scripts/seed-full.sql` crea la base `WorldCupPlanner` **desde cero** (incluye
`CREATE DATABASE`, esquema, datos y las filas de `__EFMigrationsHistory`). Si la
base ya existe, borrala primero:

```bash
docker cp db/scripts/seed-full.sql worldcup-sqledge:/tmp/seed-full.sql
docker exec worldcup-sqledge /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'WorldCup2026!Dev' -C -i /tmp/seed-full.sql
```

### 5.3 Opción C — Restaurar el backup con datos

```bash
docker cp db/backups/seed-full.bak worldcup-sqledge:/var/opt/mssql/data/seed-full.bak
docker exec worldcup-sqledge /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'WorldCup2026!Dev' -C -Q "
    RESTORE DATABASE [WorldCupPlanner]
    FROM DISK = N'/var/opt/mssql/data/seed-full.bak'
    WITH REPLACE,
      MOVE 'WorldCupPlanner'     TO '/var/opt/mssql/data/WorldCupPlanner.mdf',
      MOVE 'WorldCupPlanner_log' TO '/var/opt/mssql/data/WorldCupPlanner_log.ldf';"
```

---

## 6. Build, ejecución y tests

```bash
# Restaurar y compilar la solución
dotnet restore project/SolutionCopa/SolutionCopa.sln
dotnet build   project/SolutionCopa/SolutionCopa.sln

# Levantar la UI (aplica migraciones pendientes al iniciar)
dotnet run --project project/SolutionCopa/UI/UI.csproj

# Tests (usan EF Core In-Memory; no requieren Docker)
dotnet test project/SolutionCopa/SolutionCopa.sln
```

Una vez corriendo, navegá a la URL que imprime Kestrel (p. ej.
`https://localhost:5001`) e ingresá con cualquiera de las credenciales de §5.

---

## 7. Notas de diseño

- **Autosave por operación**: cada repositorio crea un `AppDbContext` efímero vía
  `IDbContextFactory<AppDbContext>` y persiste en el `Add/Update/Delete` (no hay
  botón "guardar todo").
- Las **migraciones** viven en `project/SolutionCopa/DataAccess/Migrations/`.
- El seeder copia a su carpeta de salida el CSV oficial de equipos
  (`teams-wc2026-48.csv`) y el directorio `flags-master/` con las banderas PNG.
- Los scripts `team_initial.sql`, `users_roles.sql`, `stadiums_settings_audit.sql`
  y `fixture_matches.sql` de `db/scripts/` son fragmentos históricos por
  migración; para instalar usá `seed-empty.sql` / `seed-full.sql`.
