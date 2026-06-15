# Probar la Release — guía rápida

Cómo levantar y probar la demo en **5 minutos**. Para detalle, ver `INSTALL.md`.

## Requisitos
- **Docker Desktop instalado y abierto** (el botón debe estar en verde / "Running").
- Nada más: la app es self-contained (no hace falta instalar .NET).

> Todos los comandos se ejecutan **dentro de la carpeta `release-build/`**, en PowerShell.

---

## A. Probar la app con base vacía (solo usuarios básicos)

```powershell
cd release-build
Set-ExecutionPolicy -Scope Process Bypass   # solo una vez por ventana
.\start.ps1
```

`start.ps1` levanta la base, espera a que acepte conexiones y arranca la app.
Cuando veas **"Iniciando WorldCupPlanner en http://localhost:5000"**, abrí esa URL.

Login: `admin@copa.com` / `Admin@123`

Para detener: **Ctrl + C** (la base queda levantada).

---

## B. Probar la demo completa (48 equipos + fixture simulado)

El restore de la demo **necesita que la app NO esté conectada**, por eso el orden importa:

```powershell
# 1. Levantá todo
.\start.ps1
# 2. Cortá SOLO la app con Ctrl + C  (el contenedor de base sigue corriendo)
# 3. Cargá los datos de demo
.\seed.ps1
# 4. Volvé a arrancar la app
.\start.ps1
```

Abrí `http://localhost:5000` y entrá con:

| Rol | Email | Password |
|---|---|---|
| Admin | `admin1@copa.com` | `Admin@123` |
| Editor | `editor1@copa.com` | `Editor@123` |
| Periodista | `periodista1@copa.com` | `Prensa@123` |

Para volver a base vacía: `.\seed.ps1 empty` (con la app detenida).

---

## Qué revisar en la demo (checklist de defensa)

- [ ] Login con los 3 roles y que cada uno vea lo que corresponde.
- [ ] **Equipos**: 48 cargados, con bandera; generador automático funciona.
- [ ] **Estadios**: ABM (solo admin).
- [ ] **Fixture**: 12 grupos visibles, navegación a un grupo, 6 partidos por grupo.
- [ ] **Cruces fase 2**: generar y avanzar el bracket hasta la Final.
- [ ] **Partidos**: filtros por fecha/estadio/grupo/fase y edición de un partido.
- [ ] **Log de auditoría**: aparecen altas, generación de fixture y simulación.
- [ ] **Importar CSV** (`teams-wc2026-48.csv`) y **exportar**.

---

## Si algo falla

| Síntoma | Causa / solución |
|---|---|
| "Docker Desktop no esta iniciado" | Abrí Docker Desktop y esperá a que diga "Running". |
| `seed.ps1` falla en el RESTORE | La app sigue conectada: cortala con Ctrl+C y reintentá. |
| La app cierra con error de conexión | Arrancaste `UI.exe` antes que la base. Usá siempre `start.ps1`. |
| El puerto 5000 está ocupado | Cerrá la instancia previa o cambiá `--urls` en `start.ps1`. |
| Primer arranque muy lento | Está descargando la imagen de Azure SQL Edge (necesita Internet una vez). |
