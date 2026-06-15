# WorldCupPlanner 2026 - Release de Entrega 2

Paquete ejecutable para Windows x64. La aplicacion es self-contained: no
requiere instalar .NET 8. La persistencia usa Azure SQL Edge, por lo que
**Docker Desktop debe estar instalado e iniciado**. El primer arranque necesita
acceso a Internet si la imagen `mcr.microsoft.com/azure-sql-edge:latest` aun no
esta descargada.

## Inicio recomendado

1. Extraer el ZIP completo.
2. Abrir PowerShell en la carpeta extraida.
3. Ejecutar:

   ```powershell
   Set-ExecutionPolicy -Scope Process Bypass
   .\start.ps1
   ```

4. Abrir `http://localhost:5000`.

`start.ps1` valida Docker, levanta Azure SQL Edge, espera que acepte conexiones
y ejecuta `win-x64/UI.exe`. En el primer arranque, la aplicacion crea la base,
aplica todas las migraciones y crea usuarios basicos.

## Credenciales del primer arranque

| Rol | Email | Password |
|---|---|---|
| Admin | `admin@copa.com` | `Admin@123` |
| Editor | `editor@copa.com` | `Editor@123` |
| Periodista | `periodista@copa.com` | `Periodista@123` |

## Cargar datos de demo

Con la app ya corriendo (`start.ps1` dejo el contenedor levantado), en otra
ventana de PowerShell:

```powershell
.\seed.ps1          # carga la demo completa (login: admin1@copa.com / Admin@123)
.\seed.ps1 empty    # reinicia la base a vacia (solo esquema)
```

`seed.ps1` restaura `db/backups/seed-full.bak` (o `seed-empty.bak`) en un solo
paso. El RESTORE requiere que no haya conexiones activas: si falla, deten la app
con Ctrl+C, corre `seed.ps1` y volve a iniciar. Alternativa manual con
`sqlcmd`/`seed-full.sql` documentada en `INSTALL.md`.

## Inicio manual

```powershell
docker compose up -d sqlserver
.\win-x64\UI.exe --urls http://localhost:5000
```

No ejecutar `UI.exe` antes de que SQL Edge este listo: la aplicacion aplica
migraciones al arrancar y terminara con error si no puede conectarse.

## Contenido del asset formal

- `win-x64/`: aplicacion self-contained.
- `start.ps1`: inicio verificado de base + aplicacion.
- `INSTALL.md`: instalacion manual, restauracion y datos de prueba.
- `docker-compose.yml` y `.env`: configuracion reproducible de SQL Edge.
- `db/scripts/`: scripts de esquema y datos.
- `db/backups/`: backups vacio y con datos de prueba.
- Archivo `.sha256`: integridad del ZIP publicado en GitHub Releases.
