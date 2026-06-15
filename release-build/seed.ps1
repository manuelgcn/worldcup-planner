param(
    [ValidateSet('full', 'empty')]
    [string]$Data = 'full'
)

$ErrorActionPreference = 'Stop'

$releaseRoot = $PSScriptRoot
$envFile = Join-Path $releaseRoot '.env'
$dbRoot = Join-Path $releaseRoot 'db'

if (-not (Test-Path -LiteralPath $envFile)) {
    $envFile = Join-Path (Split-Path $releaseRoot -Parent) '.env'
}

if (-not (Test-Path -LiteralPath $dbRoot)) {
    $dbRoot = Join-Path (Split-Path $releaseRoot -Parent) 'db'
}

$backupFile = Join-Path $dbRoot ("backups/seed-$Data.bak")

if (-not (Test-Path -LiteralPath $envFile -PathType Leaf)) {
    throw 'No se encontro .env junto al release.'
}

if (-not (Test-Path -LiteralPath $backupFile -PathType Leaf)) {
    throw "No se encontro el backup: $backupFile"
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker no esta instalado o no esta disponible en PATH.'
}

$container = docker ps --filter 'name=^/worldcup-sqledge$' --format '{{.Names}}'
if ($container -ne 'worldcup-sqledge') {
    throw 'El contenedor worldcup-sqledge no esta corriendo. Inicie primero con start.ps1.'
}

$passwordLine = Get-Content -LiteralPath $envFile |
    Where-Object { $_ -match '^MSSQL_SA_PASSWORD=' } |
    Select-Object -First 1

if (-not $passwordLine) {
    throw 'Falta MSSQL_SA_PASSWORD en .env.'
}

$password = $passwordLine.Substring('MSSQL_SA_PASSWORD='.Length)

Write-Host "Copiando seed-$Data.bak al contenedor..."
docker cp $backupFile "worldcup-sqledge:/var/opt/mssql/data/seed-$Data.bak"
if ($LASTEXITCODE -ne 0) { throw 'Fallo al copiar el backup al contenedor.' }

$restoreQuery = @"
RESTORE DATABASE [WorldCupPlanner]
FROM DISK = N'/var/opt/mssql/data/seed-$Data.bak'
WITH REPLACE,
  MOVE 'WorldCupPlanner'     TO '/var/opt/mssql/data/WorldCupPlanner.mdf',
  MOVE 'WorldCupPlanner_log' TO '/var/opt/mssql/data/WorldCupPlanner_log.ldf';
"@

Write-Host "Restaurando la base ($Data)..."
docker exec worldcup-sqledge /opt/mssql-tools/bin/sqlcmd `
    -S localhost -U sa -P $password -C -Q $restoreQuery

if ($LASTEXITCODE -ne 0) {
    throw 'Fallo el RESTORE. Verifique que no haya conexiones activas (cierre la app con Ctrl+C y reintente).'
}

Write-Host ''
if ($Data -eq 'full') {
    Write-Host 'Datos de demo cargados. Login: admin1@copa.com / Admin@123'
}
else {
    Write-Host 'Base reiniciada a vacia (solo esquema).'
}
