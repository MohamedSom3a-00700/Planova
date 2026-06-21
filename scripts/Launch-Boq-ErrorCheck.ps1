<#
.SYNOPSIS
    Launches Planova, selects a project, opens BOQ Studio, then scans for any errors.
.DESCRIPTION
    Automates the full workflow: build → launch → select project → navigate to BOQ Studio →
    capture console/log errors and UI error indicators. Exits with code 1 if errors found.
.PARAMETER ProjectName
    Partial name of the project to select (first matching item). Default: picks first project.
.PARAMETER SkipBuild
    Skip the dotnet build step if the app is already built.
#>
param(
    [string]$ProjectName = "",
    [switch]$SkipBuild
)

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms

Add-Type @"
using System.Runtime.InteropServices;
public class Mouse {
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    public const uint LEFTDOWN = 0x02;
    public const uint LEFTUP = 0x04;
    public static void Click(int x, int y) {
        SetCursorPos(x, y);
        mouse_event(LEFTDOWN, 0, 0, 0, 0);
        System.Threading.Thread.Sleep(50);
        mouse_event(LEFTUP, 0, 0, 0, 0);
    }
}
"@

$ScopeDescendants = [System.Windows.Automation.TreeScope]::Subtree
$NameProp = [System.Windows.Automation.AutomationElement]::NameProperty

$global:AnyError = $false
$global:Errors = @()

function Step($text)  { Write-Host "`n>>> $text" -ForegroundColor Cyan }
function Pass($text) { Write-Host "  [PASS] $text" -ForegroundColor Green }
function Fail($text) { $global:AnyError = $true; Write-Host "  [FAIL] $text" -ForegroundColor Red }
function Info($text) { Write-Host "  [INFO] $text" -ForegroundColor DarkGray }

function Wait-Window($title, $timeout = 60) {
    for ($i = 0; $i -lt $timeout; $i++) {
        $w = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            [System.Windows.Automation.PropertyCondition]::new($NameProp, $title))
        if ($w) { return $w }
        Start-Sleep -Seconds 1
    }
    return $null
}

function Find-Text($parent, $text) {
    return $parent.FindFirst($ScopeDescendants, [System.Windows.Automation.PropertyCondition]::new($NameProp, $text))
}

function Wait-Text($parent, $text, $timeout = 15) {
    for ($i = 0; $i -lt $timeout; $i++) {
        $e = Find-Text $parent $text
        if ($e) { return $e }
        Start-Sleep -Seconds 1
    }
    return $null
}

function Find-Text-Partial($parent, $partialText) {
    $all = $parent.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
    return $all | Where-Object { $_.Current.Name -like "*$partialText*" } | Select-Object -First 1
}

function Click-Elem($elem) {
    $r = $elem.Current.BoundingRectangle
    if ($r.Width -le 0 -or $r.Height -le 0) { return $false }
    [Mouse]::Click([int]($r.Left + $r.Width / 2), [int]($r.Top + $r.Height / 2))
    Start-Sleep -Milliseconds 400
    return $true
}

function Click-ElementByText($parent, [string]$Text, [int]$TimeoutSeconds = 15) {
    for ($t = 0; $t -lt $TimeoutSeconds; $t++) {
        $all = $parent.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
        for ($i = 0; $i -lt $all.Count; $i++) {
            if ($all[$i].Current.Name -eq $Text) {
                try {
                    $invoke = $all[$i].GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                    $invoke.Invoke(); return $true
                } catch {}
                $bbox = $all[$i].Current.BoundingRectangle
                if ($bbox.Width -gt 0 -and $bbox.Height -gt 0) {
                    [Mouse]::Click([int]($bbox.Left + $bbox.Width / 2), [int]($bbox.Top + $bbox.Height / 2))
                    Start-Sleep -Milliseconds 400
                    return $true
                }
                $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
                $cur = $walker.GetParent($all[$i])
                $depth = 0
                while ($cur -and $depth -lt 20) {
                    try {
                        $invoke = $cur.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                        $invoke.Invoke(); return $true
                    } catch {}
                    $bbox = $cur.Current.BoundingRectangle
                    if ($bbox.Width -gt 0 -and $bbox.Height -gt 0) {
                        [Mouse]::Click([int]($bbox.Left + $bbox.Width / 2), [int]($bbox.Top + $bbox.Height / 2))
                        Start-Sleep -Milliseconds 400
                        return $true
                    }
                    $cur = $walker.GetParent($cur)
                    $depth++
                }
            }
        }
        Start-Sleep -Seconds 1
    }
    return $false
}

function Collect-LogErrors {
    $logDir = Join-Path $env:APPDATA "Planova\logs"
    $fatalFile = Join-Path $logDir "fatal-error.txt"
    $foundErrors = @()

    if (Test-Path $fatalFile) {
        $content = Get-Content $fatalFile -Raw -ErrorAction SilentlyContinue
        if ($content) {
            $foundErrors += "[FATAL] fatal-error.txt exists: $content"
            Info "Fatal error file found"
        }
    }

    $logFiles = Get-ChildItem -LiteralPath $logDir -Filter "planova-*.log" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1

    if ($logFiles) {
        Info "Reading latest log: $($logFiles.Name)"
        $lines = Get-Content $logFiles.FullName -ErrorAction SilentlyContinue
        foreach ($line in $lines) {
            if ($line -match '\[ERR\]|\[ERROR\]|Exception|Fatal|Unhandled') {
                $foundErrors += "[LOG] $line"
            }
        }
    }

    return $foundErrors
}

function Scan-UiErrors($parent) {
    $all = $parent.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
    $uiErrors = @()

    $errorKeywords = @('error', 'exception', 'failed', 'failure', 'crash', 'fatal', 'unhandled', 'warning')
    foreach ($el in $all) {
        $name = $el.Current.Name
        if (-not $name) { continue }
        foreach ($kw in $errorKeywords) {
            if ($name -match $kw) {
                $ct = ''; try { $ct = $el.Current.ControlType.ProgrammaticName } catch { $ct = '?' }
                $uiErrors += "[UI] $ct '$name'"
                break
            }
        }
    }

    return $uiErrors
}

# === MAIN ===

Step "0/4 Building & launching Planova"

if (-not $SkipBuild) {
    Write-Host "    Building..." -ForegroundColor DarkGray
    dotnet build "$PSScriptRoot\..\Planova.UI\Planova.UI.csproj" -nologo -clp:NoSummary 2>$null
    if ($LASTEXITCODE -ne 0) { Fail "Build failed"; exit 1 }
    Pass "Build succeeded"
}

$exe = "$PSScriptRoot\..\Planova.UI\bin\Debug\net8.0-windows\Planova.UI.exe"
if (-not (Test-Path $exe)) { Fail "Exe not found at $exe"; exit 1 }

$proc = Start-Process -FilePath $exe -WindowStyle Normal -PassThru
$main = Wait-Window 'Planova' 60
if (-not $main) { Fail "Planova window not found within 60s"; exit 1 }
Pass "Planova launched"

Start-Sleep -Seconds 3

Step "1/4 Selecting project"

$all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
$projCombo = $all | Where-Object {
    try { $_.Current.ControlType.ProgrammaticName -match 'ComboBox$' -and $_.Current.BoundingRectangle.Width -gt 100 -and $_.Current.BoundingRectangle.Height -gt 20 } catch { $false }
} | Select-Object -First 1

if (-not $projCombo) {
    $projSelector = $main.FindFirst($ScopeDescendants,
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::AutomationIdProperty, "ProjectSelector"))
    if ($projSelector) {
        Info "Found ProjectSelector by AutomationId"
        try {
            $expand = $projSelector.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            $expand.Expand()
            Start-Sleep -Milliseconds 500
        } catch { Info "Expand failed, trying click" }

        $items = $projSelector.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
        $selected = $false

        if ($ProjectName) {
            foreach ($item in $items) {
                $n = $item.Current.Name
                if ($n -like "*$ProjectName*") {
                    Write-Host "  Selecting: $n" -ForegroundColor Green
                    Click-Elem $item; $selected = $true; break
                }
            }
        }

        if (-not $selected) {
            foreach ($item in $items) {
                $n = $item.Current.Name
                if ($n.Length -gt 2 -and -not $n.Contains("Dto")) {
                    Write-Host "  Selecting: $n" -ForegroundColor Green
                    Click-Elem $item; $selected = $true; break
                }
            }
        }

        if (-not $selected) { Fail "No project found in ProjectSelector" }
        [System.Windows.Forms.SendKeys]::SendWait("{ESCAPE}")
    } else {
        Fail "No project selector found (ComboBox or AutomationId)"
    }
} else {
    Info "Found project ComboBox"
    $selected = $false

    try {
        $expand = $projCombo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        $expand.Expand()
        Start-Sleep -Milliseconds 800

        $comboItems = $projCombo.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
        $navExclusions = @('Dashboard', 'Projects', 'BOQ Studio', 'WBS Studio', 'Activity Studio', 'Resource Studio', 'Cost Studio', 'Reports', 'Primavera Studio', 'Schedule Compare', 'Settings', 'Parties', 'Excel Studio', 'Delay Analysis', 'Claims', 'Chronology', 'Correspondence', 'Knowledge Base', 'Analytics', 'Integration Hub')

        if ($ProjectName) {
            foreach ($item in $comboItems) {
                $n = $item.Current.Name
                if ($n -like "*$ProjectName*" -and $navExclusions -notcontains $n) {
                    Write-Host "  Selecting: $n" -ForegroundColor Green
                    Click-Elem $item; $selected = $true; break
                }
            }
        }

        if (-not $selected) {
            foreach ($item in $comboItems) {
                $n = $item.Current.Name
                if ($n.Length -gt 2 -and -not $n.Contains("Dto") -and $navExclusions -notcontains $n) {
                    Write-Host "  Selecting: $n" -ForegroundColor Green
                    Click-Elem $item; $selected = $true; break
                }
            }
        }

        if (-not $selected) {
            [System.Windows.Forms.SendKeys]::SendWait('{ESCAPE}')
            Start-Sleep -Milliseconds 300
            Info "No valid project in ComboBox dropdown, trying keyboard approach"
            Click-Elem $projCombo; Start-Sleep -Milliseconds 500
            [System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 1000
            [System.Windows.Forms.SendKeys]::SendWait('{DOWN}'); Start-Sleep -Milliseconds 300
            [System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Milliseconds 300
            $selected = $true
        }
    } catch {
        Info "ExpandCollapsePattern failed, using keyboard approach"
        Click-Elem $projCombo; Start-Sleep -Milliseconds 500
        [System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 1000
        [System.Windows.Forms.SendKeys]::SendWait('{DOWN}'); Start-Sleep -Milliseconds 300
        [System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Milliseconds 300
        $selected = $true
    }
}

Start-Sleep -Seconds 3

$projName = Find-Text-Partial $main "project"
if ($projName) { Info "Current project text: '$($projName.Current.Name)'" }

Pass "Project selected"

Step "2/4 Navigating to BOQ Studio"

$boqNav = $null
for ($t = 0; $t -lt 15; $t++) {
    $boqNav = Find-Text $main 'BOQ Studio'
    if ($boqNav) { break }
    Start-Sleep -Milliseconds 500
}

if (-not $boqNav) {
    Fail "BOQ Studio nav item not found in sidebar"
} else {
    $clicked = Click-ElementByText $main 'BOQ Studio' 10
    if (-not $clicked) { Fail "Could not click BOQ Studio nav item" }
}

Start-Sleep -Seconds 3

$boqHeader = Wait-Text $main 'BOQ Studio' 20
if ($boqHeader) { Pass "BOQ Studio view loaded" }
else { Fail "BOQ Studio view did not load" }

Start-Sleep -Seconds 2

Step "3/4 Scanning for errors"

Info "Scanning UI elements for error indicators..."
$uiErrors = Scan-UiErrors $main
if ($uiErrors.Count -gt 0) {
    Fail "Found $($uiErrors.Count) error indicators in UI:"
    foreach ($err in $uiErrors) {
        Write-Host "    $err" -ForegroundColor Red
        $global:Errors += $err
    }
} else {
    Pass "No error indicators found in UI elements"
}

Info "Checking application logs..."
$logErrors = Collect-LogErrors
if ($logErrors.Count -gt 0) {
    Fail "Found $($logErrors.Count) error entries in logs:"
    foreach ($err in $logErrors) {
        Write-Host "    $err" -ForegroundColor Red
        $global:Errors += $err
    }
} else {
    Pass "No error entries found in application logs"
}

Info "Checking stderr from process..."
try {
    $stderr = $proc.StandardError
    if ($stderr) {
        $errOutput = $stderr.ReadToEnd()
        if ($errOutput -and $errOutput.Trim() -ne '') {
            Fail "Process stderr: $errOutput"
            $global:Errors += "[STDERR] $errOutput"
        } else { Pass "No stderr output" }
    } else { Pass "No stderr stream available" }
} catch { Pass "Stderr check skipped" }

Step "4/4 Results"

Write-Host "`n========================================" -ForegroundColor Cyan
if ($global:AnyError) {
    Write-Host "  BOQ ERROR CHECK: ERRORS FOUND" -ForegroundColor Red
    Write-Host "  Total errors: $($global:Errors.Count)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Cyan

    Write-Host "`n  Error details:" -ForegroundColor Yellow
    foreach ($e in $global:Errors) { Write-Host "    $e" -ForegroundColor Red }
} else {
    Write-Host "  BOQ ERROR CHECK: NO ERRORS FOUND" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
}

Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue

if ($global:AnyError) { exit 1 } else { exit 0 }
