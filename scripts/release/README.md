# WorldCupPlanner — Release package (Windows x64)

Self-contained build: **no .NET installation needed**. The database runs on Azure SQL Edge,
so **Docker must be installed and running** (the first start downloads the image).

Run the commands below in PowerShell, inside this folder.

## Start

```powershell
Set-ExecutionPolicy -Scope Process Bypass   # once per window
.\start.ps1
```

Open `http://localhost:5000` and sign in with `admin@copa.com` / `Admin@123`.
Stop with **Ctrl + C** (the database keeps running).

## Load the demo (48 teams + simulated group stage)

The restore needs the app disconnected, hence this order:

```powershell
.\start.ps1      # 1. start everything
# 2. Ctrl + C to stop ONLY the app (the database stays up)
.\seed.ps1       # 3. load the demo
.\start.ps1      # 4. start the app again
```

Demo login: `admin1@copa.com` / `Admin@123`.

## Scripts

| Script | What it does |
|---|---|
| `start.ps1` | Checks Docker, starts Azure SQL Edge, waits until it accepts connections and runs the app on `http://localhost:5000`. On first run it creates the database, applies migrations and creates the default users. |
| `seed.ps1` | Restores the full demo from `db/backups/seed-full.bak`. `.\seed.ps1 empty` resets to an empty schema. The app must be stopped. |

## Troubleshooting

- **"Docker Desktop no esta iniciado"** → open Docker Desktop and wait until it shows *Running*.
- **`seed.ps1` fails on RESTORE** → the app is still connected: stop it with Ctrl + C and retry.
- **The app exits with a connection error** → `UI.exe` was started before the database. Always use `start.ps1`.

## Package contents

`win-x64/` (the app), `start.ps1`, `seed.ps1`, `docker-compose.yml`, `.env`, `db/` (schema
scripts and backups), this `README.md`, and a `.sha256` file to verify the ZIP. To build
from source, see the repository README.
