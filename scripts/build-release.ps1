param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^v[0-9]+\.[0-9]+\.[0-9]+[-A-Za-z0-9.]*$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$artifactsRoot = Join-Path $repoRoot 'artifacts'
$packageName = "WorldCupPlanner-$Version-win-x64"
$stagingRoot = Join-Path $artifactsRoot $packageName
$zipPath = Join-Path $artifactsRoot "$packageName.zip"
$checksumPath = "$zipPath.sha256"

function Remove-ReleasePath([string]$Path) {
    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $fullArtifactsRoot = [System.IO.Path]::GetFullPath($artifactsRoot) + [System.IO.Path]::DirectorySeparatorChar

    if (-not $fullPath.StartsWith($fullArtifactsRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Se rechazo eliminar una ruta fuera de artifacts: $fullPath"
    }

    if (Test-Path -LiteralPath $fullPath) {
        Remove-Item -LiteralPath $fullPath -Recurse -Force
    }
}

New-Item -ItemType Directory -Path $artifactsRoot -Force | Out-Null
Remove-ReleasePath $stagingRoot
Remove-ReleasePath $zipPath
Remove-ReleasePath $checksumPath

$publishDirectory = Join-Path $stagingRoot 'win-x64'
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null

dotnet publish (Join-Path $repoRoot 'src/WorldCupPlanner.UI/WorldCupPlanner.UI.csproj') `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDirectory

if ($LASTEXITCODE -ne 0) {
    throw 'dotnet publish fallo.'
}

Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts/release/README.md') -Destination (Join-Path $stagingRoot 'README.md')
Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts/release/start.ps1') -Destination (Join-Path $stagingRoot 'start.ps1')
Copy-Item -LiteralPath (Join-Path $repoRoot 'scripts/release/seed.ps1') -Destination (Join-Path $stagingRoot 'seed.ps1')
Copy-Item -LiteralPath (Join-Path $repoRoot 'docker-compose.yml') -Destination (Join-Path $stagingRoot 'docker-compose.yml')
Copy-Item -LiteralPath (Join-Path $repoRoot '.env.example') -Destination (Join-Path $stagingRoot '.env')
Copy-Item -LiteralPath (Join-Path $repoRoot 'db') -Destination (Join-Path $stagingRoot 'db') -Recurse

$requiredFiles = @(
    'win-x64/WorldCupPlanner.UI.exe',
    'win-x64/appsettings.json',
    'README.md',
    'start.ps1',
    'seed.ps1',
    'docker-compose.yml',
    '.env',
    'db/scripts/seed-empty.sql',
    'db/scripts/seed-full.sql',
    'db/backups/seed-empty.bak',
    'db/backups/seed-full.bak'
)

foreach ($relativePath in $requiredFiles) {
    $fullPath = Join-Path $stagingRoot $relativePath
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "Falta un archivo obligatorio en el paquete: $relativePath"
    }
}

Compress-Archive -Path (Join-Path $stagingRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal

$checksum = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
"$checksum  $([System.IO.Path]::GetFileName($zipPath))" | Set-Content -LiteralPath $checksumPath -Encoding ascii

Write-Host "Release generado:"
Write-Host "  $zipPath"
Write-Host "  $checksumPath"
