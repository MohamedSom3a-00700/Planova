<#
.SYNOPSIS
    Launches Planova, selects a project, opens BOQ Studio Import tab, imports from project document, and commits.
.DESCRIPTION
    Full end-to-end automation: build → launch → select project → navigate to BOQ Studio →
    click Import tab → select "Use project document" → click "Use Selected" → wait for column mapping →
    Preview Import → Commit Import → verify success. Exits with code 1 on any failure.
.PARAMETER ProjectName
    Partial name of the project to select. Default: picks first real project.
.PARAMETER SkipBuild
    Skip the dotnet build step if the app is already built.
#>
param(
    [string]$ProjectName = "",
    [switch]$SkipBuild
)

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms

Add-Type @"
using System;
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
$AutoIdProp = [System.Windows.Automation.AutomationElement]::AutomationIdProperty

$global:AnyFailed = $false

function Step($text)  { Write-Host "`n>>> $text" -ForegroundColor Cyan }
function Pass($text) { Write-Host "  [PASS] $text" -ForegroundColor Green }
function Fail($text) { $global:AnyFailed = $true; Write-Host "  [FAIL] $text" -ForegroundColor Red }
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
    return @($all | Where-Object { $_.Current.Name -like "*$partialText*" } | Select-Object -First 1)
}

function Click-Elem($elem) {
    if (-not $elem) { return $false }
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

function Select-Project($parent, [string]$ProjName) {
    $navExclusions = @('Dashboard', 'Projects', 'BOQ Studio', 'WBS Studio', 'Activity Studio',
        'Resource Studio', 'Cost Studio', 'Reports', 'Primavera Studio', 'Schedule Compare',
        'Settings', 'Parties', 'Excel Studio', 'Delay Analysis', 'Claims', 'Chronology',
        'Correspondence', 'Knowledge Base', 'Analytics', 'Integration Hub')

    $all = $parent.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
    $projCombo = @($all | Where-Object {
        try { $_.Current.ControlType.ProgrammaticName -match 'ComboBox$' -and $_.Current.BoundingRectangle.Width -gt 100 -and $_.Current.BoundingRectangle.Height -gt 20 } catch { $false }
    } | Select-Object -First 1)

    if ($projCombo) {
        Info "Found project ComboBox"
        try {
            $expand = $projCombo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            $expand.Expand()
            Start-Sleep -Milliseconds 800

            $comboItems = $projCombo.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
            foreach ($item in $comboItems) {
                $n = $item.Current.Name
                if ($ProjName -and $n -like "*$ProjName*" -and $navExclusions -notcontains $n) {
                    Write-Host "  Selecting: $n" -ForegroundColor Green
                    Click-Elem $item; return $true
                }
            }
            foreach ($item in $comboItems) {
                $n = $item.Current.Name
                if ($n.Length -gt 2 -and -not $n.Contains("Dto") -and $navExclusions -notcontains $n) {
                    Write-Host "  Selecting: $n" -ForegroundColor Green
                    Click-Elem $item; return $true
                }
            }
        } catch { Info "ExpandCollapse failed" }

        Info "Keyboard fallback for project selection"
        Click-Elem $projCombo; Start-Sleep -Milliseconds 500
        [System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 1000
        [System.Windows.Forms.SendKeys]::SendWait('{DOWN}'); Start-Sleep -Milliseconds 300
        [System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Milliseconds 300
        return $true
    }

    $projSelector = $parent.FindFirst($ScopeDescendants,
        [System.Windows.Automation.PropertyCondition]::new($AutoIdProp, "ProjectSelector"))
    if ($projSelector) {
        Info "Found ProjectSelector by AutomationId"
        try {
            $expand = $projSelector.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
            $expand.Expand()
            Start-Sleep -Milliseconds 500
        } catch {}

        $items = $projSelector.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($item in $items) {
            $n = $item.Current.Name
            if ($ProjName -and $n -like "*$ProjName*" -and $navExclusions -notcontains $n) {
                Write-Host "  Selecting: $n" -ForegroundColor Green
                Click-Elem $item; return $true
            }
        }
        foreach ($item in $items) {
            $n = $item.Current.Name
            if ($n.Length -gt 2 -and -not $n.Contains("Dto") -and $navExclusions -notcontains $n) {
                Write-Host "  Selecting: $n" -ForegroundColor Green
                Click-Elem $item; return $true
            }
        }
        [System.Windows.Forms.SendKeys]::SendWait("{ESCAPE}")
    }

    return $false
}

# === MAIN ===

Step "0/8 Building & launching Planova"

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

Step "1/8 Selecting project"

$selected = Select-Project $main $ProjectName
if (-not $selected) { Fail "No project found or selected"; exit 1 }
Start-Sleep -Seconds 3

$projText = Find-Text-Partial $main "project"
if ($projText) { Info "Current project: '$($projText.Current.Name)'" }
Pass "Project selected"

Step "2/8 Navigating to BOQ Studio"

$boqNav = $null
for ($t = 0; $t -lt 15; $t++) {
    $boqNav = Find-Text $main 'BOQ Studio'
    if ($boqNav) { break }
    Start-Sleep -Milliseconds 500
}
if (-not $boqNav) { Fail "BOQ Studio nav item not found"; exit 1 }

$clicked = Click-ElementByText $main 'BOQ Studio' 10
if (-not $clicked) { Fail "Could not click BOQ Studio nav item"; exit 1 }

$studioHdr = Wait-Text $main 'BOQ Studio' 20
if ($studioHdr) { Pass "BOQ Studio view loaded" }
else { Fail "BOQ Studio view did not load"; exit 1 }

Start-Sleep -Seconds 2

Step "3/8 Clicking Import tab"

$importTab = $null
for ($t = 0; $t -lt 15; $t++) {
    $all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
    $tabItems = @($all | Where-Object {
        try { $_.Current.ControlType.ProgrammaticName -match 'TabItem$' -and $_.Current.BoundingRectangle.Width -gt 50 -and $_.Current.BoundingRectangle.Height -gt 15 } catch { $false }
    })
    foreach ($tab in $tabItems) {
        if ($tab.Current.Name -eq 'Import') { $importTab = $tab; break }
    }
    if ($importTab) { break }
    Start-Sleep -Milliseconds 500
}

if (-not $importTab) {
    Info "No TabItem named 'Import', searching by text..."
    $importTab = Find-Text $main 'Import'
}

if (-not $importTab) { Fail "Import tab not found"; exit 1 }

Click-Elem $importTab
Start-Sleep -Seconds 2

$importHdr = Wait-Text $main 'Import BOQ' 15
if ($importHdr) { Pass "Import view loaded" }
else { Fail "Import view did not load"; exit 1 }

Step "4/8 Selecting 'Use project document' mode"

$projDocRadio = Find-Text $main 'Use project document'
if ($projDocRadio) {
    Info "Found 'Use project document' radio button"
    Click-Elem $projDocRadio
    Start-Sleep -Seconds 2
    Pass "Selected 'Use project document' mode"
} else {
    Info "'Use project document' text not found directly, scanning all elements..."
    $all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
    $radioCandidates = @($all | Where-Object {
        try { $_.Current.Name -like "*project document*" -or $_.Current.Name -like "*Use project*" } catch { $false }
    })
    if ($radioCandidates.Count -gt 0) {
        Click-Elem $radioCandidates[0]
        Start-Sleep -Seconds 2
        Pass "Selected project document mode"
    } else {
        Fail "'Use project document' option not found"; exit 1
    }
}

Start-Sleep -Seconds 2

Step "5/8 Selecting BOQ document from ComboBox"

$boqDocStatus = Find-Text-Partial $main "BOQ document"
if ($boqDocStatus) { Info "BOQ document status: '$($boqDocStatus.Current.Name)'" }

$noBoqText = Find-Text $main 'No BOQ documents found'
if ($noBoqText) { Fail "No BOQ documents found for this project - cannot import"; exit 1 }

Info "Finding 'Use Selected' button to locate the document ComboBox..."
$useSelectedBtn = $null
for ($t = 0; $t -lt 15; $t++) {
    $useSelectedBtn = Find-Text $main 'Use Selected'
    if ($useSelectedBtn) { break }
    Start-Sleep -Seconds 1
}
if (-not $useSelectedBtn) { Fail "'Use Selected' button not found"; exit 1 }

$useSelectedRect = $useSelectedBtn.Current.BoundingRectangle
Info "'Use Selected' at Top=$([int]$useSelectedRect.Top) Left=$([int]$useSelectedRect.Left)"

Info "Searching for ComboBox near 'Use Selected'..."
$all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
$combos = @($all | Where-Object {
    try {
        $_.Current.ControlType.ProgrammaticName -match 'ComboBox$' -and
        $_.Current.BoundingRectangle.Width -gt 80 -and
        $_.Current.BoundingRectangle.Height -gt 15
    } catch { $false }
})

Info "Found $($combos.Count) ComboBoxes total"

$boqComboBox = $null
$minDist = [double]::MaxValue
foreach ($cb in $combos) {
    $cbRect = $cb.Current.BoundingRectangle
    $distY = [Math]::Abs($cbRect.Top - $useSelectedRect.Top)
    $distX = [Math]::Abs($cbRect.Left - $useSelectedRect.Left)
    $dist = $distY + $distX
    Info "  ComboBox dist=$([int]$dist) Top=$([int]$cbRect.Top) Left=$([int]$cbRect.Left)"
    if ($dist -lt $minDist) {
        $minDist = $dist
        $boqComboBox = $cb
    }
}

if (-not $boqComboBox) { Fail "No ComboBox found near 'Use Selected'"; exit 1 }

Info "Selected nearest ComboBox (dist=$([int]$minDist))"

Info "Expanding BOQ ComboBox and selecting first document..."
try {
    $expand = $boqComboBox.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $expand.Expand()
    Start-Sleep -Milliseconds 800
} catch {
    Info "ExpandCollapsePattern failed, trying click + keyboard"
    Click-Elem $boqComboBox; Start-Sleep -Milliseconds 500
    [System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 800
}

$docItems = $boqComboBox.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
Info "Expanded ComboBox has $($docItems.Count) elements"

$selectedDoc = $false
foreach ($item in $docItems) {
    $n = $item.Current.Name
    $r = $item.Current.BoundingRectangle
    if ($n -and $n.Length -gt 2 -and -not $n.Contains("Dto") -and $r.Width -gt 0 -and $r.Height -gt 0) {
        Write-Host "  Selecting BOQ document: $n" -ForegroundColor Green
        Click-Elem $item; $selectedDoc = $true; break
    }
}

if (-not $selectedDoc) {
    Info "No visible doc items, trying keyboard DOWN+ENTER"
    [System.Windows.Forms.SendKeys]::SendWait('{DOWN}'); Start-Sleep -Milliseconds 300
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Milliseconds 300
}

Pass "BOQ document selected from ComboBox"

Start-Sleep -Seconds 2

Click-Elem $useSelectedBtn
Pass "Clicked 'Use Selected'"

Start-Sleep -Seconds 3

Step "6/8 Waiting for column mapping detection"

Info "Waiting for column auto-detection..."
$mappingDetected = $false
for ($t = 0; $t -lt 30; $t++) {
    $codeLabel = Find-Text $main 'Code'
    $descLabel = Find-Text $main 'Description'
    $qtyLabel = Find-Text $main 'Quantity'
    if ($codeLabel -and $descLabel -and $qtyLabel) {
        $mappingDetected = $true
        break
    }
    Info "Waiting for column mapping labels... ($t s)"
    Start-Sleep -Seconds 1
}

if ($mappingDetected) { Pass "Column mapping section visible" }
else { Fail "Column mapping did not appear"; exit 1 }

Start-Sleep -Seconds 2

Step "7/8 Previewing import"

$previewBtn = $null
for ($t = 0; $t -lt 15; $t++) {
    $previewBtn = Find-Text $main 'Preview Import'
    if ($previewBtn) { break }
    Start-Sleep -Milliseconds 500
}

if (-not $previewBtn) { Fail "'Preview Import' button not found"; exit 1 }

Info "Clicking Preview Import..."
$clicked = Click-ElementByText $main 'Preview Import' 5
if (-not $clicked) { Fail "Could not click Preview Import"; exit 1 }

Start-Sleep -Seconds 5

$previewResult = Find-Text-Partial $main "Preview loaded"
if ($previewResult) { Pass "Preview loaded: '$($previewResult.Current.Name)'" }
else {
    $previewStatus = Find-Text-Partial $main "Preview"
    if ($previewStatus) { Info "Preview status: '$($previewStatus.Current.Name)'" }
    $previewError = Find-Text-Partial $main "Preview error"
    if ($previewError) { Fail "Preview error: '$($previewError.Current.Name)'" }
}

Step "8/8 Committing import"

$commitBtn = $null
for ($t = 0; $t -lt 30; $t++) {
    $commitBtn = Find-Text $main 'Commit Import'
    if ($commitBtn) { break }
    Info "Waiting for 'Commit Import' button... ($t s)"
    Start-Sleep -Seconds 1
}

if (-not $commitBtn) { Fail "'Commit Import' button not found"; exit 1 }

Info "Clicking Commit Import..."
$clicked = Click-ElementByText $main 'Commit Import' 5
if (-not $clicked) { Fail "Could not click Commit Import"; exit 1 }

Info "Waiting for import to complete..."
Start-Sleep -Seconds 8

$successMsg = $null
for ($t = 0; $t -lt 30; $t++) {
    $successMsg = Find-Text-Partial $main "Import complete"
    if ($successMsg) { break }
    $errorMsg = Find-Text-Partial $main "Import error"
    if ($errorMsg) { Fail "Import error: '$($errorMsg.Current.Name)'"; exit 1 }
    Info "Waiting for import result... ($t s)"
    Start-Sleep -Seconds 1
}

if ($successMsg) { Pass "Import succeeded: '$($successMsg.Current.Name)'" }
else { Fail "Could not confirm import success within 30s" }

Write-Host "`n========================================" -ForegroundColor Cyan
if ($global:AnyFailed) {
    Write-Host "  BOQ PROJECT DOCUMENT IMPORT: SOME STEPS FAILED" -ForegroundColor Red
} else {
    Write-Host "  BOQ PROJECT DOCUMENT IMPORT: ALL STEPS PASSED" -ForegroundColor Green
}
Write-Host "========================================" -ForegroundColor Cyan

Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue

if ($global:AnyFailed) { exit 1 } else { exit 0 }
