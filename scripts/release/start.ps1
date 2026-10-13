$ErrorActionPreference = 'Stop'

$releaseRoot = $PSScriptRoot
$composeFile = Join-Path $releaseRoot 'docker-compose.yml'
$envFile = Join-Path $releaseRoot '.env'

if (-not (Test-Path -LiteralPath $composeFile)) {
    $composeFile = Join-Path (Split-Path $releaseRoot -Parent) 'docker-compose.yml'
}

if (-not (Test-Path -LiteralPath $envFile)) {
    $envFile = Join-Path (Split-Path $releaseRoot -Parent) '.env'
}

$appPath = Join-Path $releaseRoot 'win-x64/WorldCupPlanner.UI.exe'

if (-not (Test-Path -LiteralPath $composeFile -PathType Leaf)) {
    throw 'No se encontro docker-compose.yml junto al release.'
}

if (-not (Test-Path -LiteralPath $envFile -PathType Leaf)) {
    throw 'No se encontro .env junto al release.'
}

if (-not (Test-Path -LiteralPath $appPath -PathType Leaf)) {
    throw 'No se encontro win-x64/WorldCupPlanner.UI.exe. Extraiga el ZIP completo antes de iniciar.'
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker no esta instalado o no esta disponible en PATH.'
}

docker info | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw 'Docker Desktop no esta iniciado.'
}

$existingContainer = docker ps -a --filter 'name=^/worldcup-sqledge$' --format '{{.Names}}'

if ($existingContainer -eq 'worldcup-sqledge') {
    Write-Host 'Reutilizando el contenedor worldcup-sqledge existente...'
    docker start worldcup-sqledge | Out-Null
}
else {
    Write-Host 'Levantando Azure SQL Edge...'
    docker compose --file $composeFile --env-file $envFile up -d sqlserver
}

if ($LASTEXITCODE -ne 0) {
    throw 'No se pudo iniciar Azure SQL Edge.'
}

$passwordLine = Get-Content -LiteralPath $envFile |
    Where-Object { $_ -match '^MSSQL_SA_PASSWORD=' } |
    Select-Object -First 1

if (-not $passwordLine) {
    throw 'Falta MSSQL_SA_PASSWORD en .env.'
}

$password = $passwordLine.Substring('MSSQL_SA_PASSWORD='.Length)
$deadline = (Get-Date).AddMinutes(2)
$databaseReady = $false

Write-Host 'Esperando que SQL Server acepte conexiones...'
while ((Get-Date) -lt $deadline) {
    docker exec worldcup-sqledge /opt/mssql-tools/bin/sqlcmd `
        -S localhost -U sa -P $password -C -Q 'SELECT 1' 2>$null | Out-Null

    if ($LASTEXITCODE -eq 0) {
        $databaseReady = $true
        break
    }

    Start-Sleep -Seconds 3
}

if (-not $databaseReady) {
    throw 'SQL Server no quedo listo dentro de 2 minutos. Revise: docker logs worldcup-sqledge'
}

Write-Host 'Iniciando WorldCupPlanner en http://localhost:5000'
Write-Host 'Presione Ctrl+C para detener la aplicacion.'

Push-Location (Split-Path $appPath -Parent)
try {
    & $appPath --urls http://localhost:5000
}
finally {
    Pop-Location
}
