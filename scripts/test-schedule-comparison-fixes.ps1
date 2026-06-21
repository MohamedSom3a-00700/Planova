<#
.SYNOPSIS
    Validates Schedule Comparison fixes.
    Runs backend unit tests + launches app for manual UI checks.
#>

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Schedule Comparison Fix Validation" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# ── Step 1: Build ──────────────────────────────────────
Write-Host "[1/4] Building solution..." -ForegroundColor Yellow
dotnet build "$RepoRoot\Planova.UI\Planova.UI.csproj" --no-restore | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Build failed" }
dotnet build "$RepoRoot\tests\Planova.ScheduleComparison.Tests\Planova.ScheduleComparison.Tests.csproj" --no-restore | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Test project build failed" }
Write-Host "  PASS`tBuild succeeded" -ForegroundColor Green

# ── Step 2: Unit tests ─────────────────────────────────
Write-Host "[2/4] Running unit tests..." -ForegroundColor Yellow
$testOut = dotnet test "$RepoRoot\tests\Planova.ScheduleComparison.Tests\Planova.ScheduleComparison.Tests.csproj" 2>&1
$testOut | Select-String "Passed!|Failed"
if ($LASTEXITCODE -ne 0) { throw "Unit tests failed" }
Write-Host "  PASS`tAll unit tests passed" -ForegroundColor Green

# ── Step 3: Pipeline round-trip tests ──────────────────
Write-Host "[3/4] Verifying result-pipeline round-trip..." -ForegroundColor Yellow
$pipeOut = dotnet test "$RepoRoot\tests\Planova.ScheduleComparison.Tests\Planova.ScheduleComparison.Tests.csproj" --filter "ScheduleComparisonResultPipeline" 2>&1
$pipeOut | Select-String "Passed!|Failed"
if ($LASTEXITCODE -ne 0) { throw "Pipeline tests failed" }
Write-Host "  PASS`t5 pipeline round-trip tests passed" -ForegroundColor Green

# ── Step 4: Launch app ─────────────────────────────────
Write-Host "[4/4] Launching application..." -ForegroundColor Yellow
Start-Process -FilePath "dotnet" -ArgumentList "run --no-build" -WorkingDirectory "$RepoRoot\Planova.UI"
Write-Host "  DONE`tApplication launched" -ForegroundColor Green

# ── Manual checklist ───────────────────────────────────
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MANUAL UI VERIFICATION" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Fix 1 - Results appear in all tabs" -ForegroundColor Yellow
Write-Host "  (after clicking 'Run Comparison' successfully)"
Write-Host "  __ Activities tab       DataGrid shows diffs (rows with data)"
Write-Host "  __ Logic tab            DataGrid shows relationship diffs"
Write-Host "  __ Resources tab        DataGrid shows resource diffs"
Write-Host "  __ Critical Path tab    Duration values are populated"
Write-Host "  __ Float tab            DataGrid has rows + counts shown above"
Write-Host "  __ Export tab           SessionId is set (buttons work)"
Write-Host ""
Write-Host "Fix 2 - History checkboxes" -ForegroundColor Yellow
Write-Host "  __ Click a checkbox in the History grid"
Write-Host "     -> toggles checked/unchecked (was broken before)"
Write-Host "  __ Click multiple checkboxes independently"
Write-Host "  __ 'Load Selected' loads the checked session"
Write-Host "  __ 'Delete Selected' deletes checked sessions"
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Backend validation:  PASS" -ForegroundColor Green
Write-Host "UI validation:      needs manual check (above)" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Cyan
