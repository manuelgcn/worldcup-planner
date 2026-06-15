# Demo en 4 pasos

Abrí Docker Desktop. Después, PowerShell en la carpeta del repo y:

1. `scripts\build-release.ps1 -Version v2.0.0-entrega2`
2. `Expand-Archive .\artifacts\WorldCupPlanner-v2.0.0-entrega2-win-x64.zip .\artifacts\demo`
3. `cd .\artifacts\demo` y después: `.\start.ps1` → Ctrl+C → `.\seed.ps1` → `.\start.ps1`
4. Abrí http://localhost:5000 y entrá con admin1@copa.com / Admin@123
