# WorldCupPlanner 2026 — Release (Entrega 2)

Hay **dos formas de correr la entrega**, ambas usan Azure SQL Edge, así que
**Docker tiene que estar instalado y abierto** (el primer arranque baja la imagen):

- **Portátil — cualquier SO (macOS / Linux / Windows)**: carpeta `portable/`.
  Requiere **.NET 8 instalado** (no es self-contained). Levantar la base con
  `docker compose up -d sqlserver` y, dentro de `portable/`, ejecutar
  `dotnet UI.dll`. Es la opción recomendada fuera de Windows. Detalle en
  `INSTALL.md` §7.
- **Self-contained Windows x64**: carpeta `win-x64/` + los `.ps1` de abajo.
  **No requiere instalar .NET.** Solo Windows.

> Los comandos `.ps1` de abajo van en PowerShell, dentro de esta carpeta (Windows).

## Arrancar (Windows, self-contained)

```powershell
Set-ExecutionPolicy -Scope Process Bypass   # una vez por ventana
.\start.ps1
```

Abrí `http://localhost:5000`. Login: `admin@copa.com` / `Admin@123`.
Detener: **Ctrl + C** (la base sigue levantada).

## Cargar la demo (48 equipos + fixture simulado)

El restore necesita la app desconectada, por eso este orden:

```powershell
.\start.ps1      # 1. levanta todo
# 2. Ctrl + C para cortar SOLO la app (la base queda arriba)
.\seed.ps1       # 3. carga la demo
.\start.ps1      # 4. vuelve a arrancar la app
```

Login demo: `admin1@copa.com` / `Admin@123`.

## Qué hace cada script

| Script | Qué hace |
|---|---|
| `start.ps1` | Valida Docker, levanta Azure SQL Edge, espera a que acepte conexiones y arranca la app en `http://localhost:5000`. En el primer arranque crea la base, aplica migraciones y crea los usuarios básicos. |
| `seed.ps1` | Restaura la demo completa (`db/backups/seed-full.bak`). Requiere la app detenida. `.\seed.ps1 empty` reinicia la base a vacía (solo esquema). |

## Si algo falla

- **"Docker Desktop no esta iniciado"** → abrí Docker Desktop y esperá a que diga *Running*.
- **`seed.ps1` falla en el RESTORE** → la app sigue conectada: cortala con Ctrl+C y reintentá.
- **La app cierra con error de conexión** → arrancaste `UI.exe` antes que la base. Usá siempre `start.ps1`.

## Contenido del asset

`win-x64/` (app self-contained Windows), `portable/` (app framework-dependent
cross-platform, `dotnet UI.dll`), `start.ps1`, `seed.ps1`, `docker-compose.yml`,
`.env`, `db/` (esquema y backups), `README.md`, `INSTALL.md` y el `.sha256` que
verifica la integridad del ZIP. La instalación manual paso a paso está en
`INSTALL.md`.
