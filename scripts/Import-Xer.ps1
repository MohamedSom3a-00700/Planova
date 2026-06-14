<#
.SYNOPSIS
    Imports a Primavera XER file into a Planova project.

.DESCRIPTION
    This script builds and runs a console importer that:
    1. Connects to the Planova database
    2. Lists available projects and lets you pick one (or create new)
    3. Parses the XER file
    4. Shows parsed data statistics
    5. Optionally commits to the database

.PARAMETER XerPath
    Path to the .xer file to import. If omitted, you will be prompted.

.PARAMETER Commit
    Automatically commit to database without prompting.

.PARAMETER ProjectId
    Select a project by ID automatically (skips the project list prompt).

.EXAMPLE
    .\scripts\Import-Xer.ps1 -XerPath "tests\Silver Sand  - Land Scape - Update 07- jun- 2026.xer"

.EXAMPLE
    .\scripts\Import-Xer.ps1 -XerPath "tests\Silver Sand  - Land Scape - Update 07- jun- 2026.xer" -Commit -ProjectId 1
#>
param(
    [string]$XerPath,
    [switch]$Commit,
    [int]$ProjectId = -1
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$consoleDir = "$scriptDir\XerImport"

if (-not (Test-Path $consoleDir)) {
    Write-Host "Error: Console project not found at $consoleDir" -ForegroundColor Red
    exit 1
}

$args = @()
if ($XerPath)   { $args += "--xer", (Resolve-Path $XerPath -ErrorAction Stop) }
if ($Commit)    { $args += "--commit" }
if ($ProjectId -ge 0) { $args += "--project", $ProjectId }

Write-Host "Building XerImport console..." -ForegroundColor Cyan
dotnet run --project "$consoleDir" -- $args 2>&1 | Where-Object { $_ -notmatch "warning NU190[34]" }
