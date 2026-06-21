param(
    [switch]$NoBuild,
    [ValidateSet('browse','new','edit','delete')]
    [string]$Action = 'browse'
)

$root = Split-Path -Parent $PSScriptRoot
$exe = "$root\Planova.UI\bin\Debug\net8.0-windows\Planova.UI.exe"

if (-not $NoBuild) {
    Write-Host "Building Planova..." -ForegroundColor Cyan
    dotnet build "$root\Planova.slnx" 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed." -ForegroundColor Red
        exit 1
    }
    Write-Host "Build succeeded." -ForegroundColor Green
}

$actionLabel = @{
    browse  = 'browsing projects'
    new     = 'creating a new project'
    edit    = 'editing the first project'
    delete  = 'deleting the first project'
}

Write-Host "Launching Planova → $($actionLabel[$Action])..." -ForegroundColor Cyan
Start-Process -FilePath "$exe" -ArgumentList "--navigate projects --action $Action"
