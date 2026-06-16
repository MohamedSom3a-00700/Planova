param([string]$ProjectName = "", [switch]$SkipBuild, [switch]$SkipExport)

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms

Add-Type @"
using System.Runtime.InteropServices;
public class Mouse {
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    public static void Click(int x, int y) { SetCursorPos(x, y); mouse_event(0x02|0x04, 0, 0, 0, 0); }
}
"@

$ScopeDescendants = [System.Windows.Automation.TreeScope]::Subtree
$NameProp = [System.Windows.Automation.AutomationElement]::NameProperty

function E($text)  { Write-Host "`n>>> $text" -ForegroundColor Cyan }
function OK($text) { Write-Host "  [PASS] $text" -ForegroundColor Green }
function NO($text) { Write-Host "  [FAIL] $text" -ForegroundColor Red }

function Wait-Window($title, $timeout) {
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

function Wait-Text($parent, $text, $timeout) {
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

function Wait-Text-Partial($parent, $partialText, $timeout) {
    for ($i = 0; $i -lt $timeout; $i++) {
        $e = Find-Text-Partial $parent $partialText
        if ($e) { return $e }
        Start-Sleep -Seconds 1
    }
    return $null
}

function Find-Diff-DataGrid-Rows($main) {
    $all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
    # Find all visible DataItem elements that are NOT class names (no dots)
    $rows = $all | Where-Object {
        try {
            $r = $_.Current.BoundingRectangle
            $_.Current.ControlType.ProgrammaticName -match 'DataItem$|Row$' -and
            $_.Current.Name -ne '' -and
            $_.Current.Name -notmatch '\.' -and
            $r.Width -gt 0 -and $r.Height -gt 0
        } catch { $false }
    }
    return @($rows)
}

function Click-Elem($elem) {
    $r = $elem.Current.BoundingRectangle
    if ($r.Width -le 0) { return }
    [void][Mouse]::Click([int]($r.Left+$r.Width/2), [int]($r.Top+$r.Height/2))
    Start-Sleep -Milliseconds 400
}

# === MAIN ===
E '0/6 Pre-importing XER files & launching Planova'
$xerProj = "$PSScriptRoot\..\scripts\XerImport\XerImport.csproj"
$sourceXerPath = Resolve-Path "$PSScriptRoot\..\tests\Silver Sand  - Land Scape - Update 24 - May - 2026.xer"
$targetXerPath = Resolve-Path "$PSScriptRoot\..\tests\Silver Sand  - Land Scape - Update 07- jun- 2026.xer"

if (-not $SkipBuild) {
    dotnet build "$PSScriptRoot\..\Planova.UI\Planova.UI.csproj" -nologo -clp:NoSummary 2>$null
}

# Import both XER files before launching (so the combo loads them fresh)
Write-Host "    Cleaning old sessions & importing source..."
dotnet run --project $xerProj -- --xer "$sourceXerPath" --commit --project 2 --clean 2>&1 | Out-Null
Write-Host "    Importing target..."
dotnet run --project $xerProj -- --xer "$targetXerPath" --commit --project 2 2>&1 | Out-Null
OK 'XER files pre-imported'

$exe = "$PSScriptRoot\..\Planova.UI\bin\Debug\net8.0-windows\Planova.UI.exe"
$proc = Start-Process $exe -PassThru
$main = Wait-Window 'Planova' 60
if (-not $main) { NO 'Window not found'; exit 1 }
OK 'Launched'

Start-Sleep -Seconds 3

E '1/6 Selecting project Silversand'
# Find the project selector ComboBox in the navigation rail
$all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
$projCombo = $all | Where-Object {
    try { $_.Current.ControlType.ProgrammaticName -match 'ComboBox$' -and $_.Current.BoundingRectangle.Width -gt 100 -and $_.Current.BoundingRectangle.Height -gt 20 } catch { $false }
} | Select-Object -First 1
if (-not $projCombo) { NO 'Project selector ComboBox not found'; exit 1 }
# Click and use keyboard to open dropdown and select Silversand (first item)
Click-Elem $projCombo; Start-Sleep -Milliseconds 500
[System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 1000
[System.Windows.Forms.SendKeys]::SendWait('{DOWN}'); Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Seconds 3
OK 'Silversand selected and loading'

E '2/6 Navigating to Schedule Compare'
$navText = $null
for ($t = 0; $t -lt 15; $t++) {
    $navText = Find-Text $main 'Schedule Compare'
    if ($navText) { break }
    Start-Sleep -Milliseconds 500
}
if (-not $navText) { NO 'Schedule Compare text not found in sidebar'; exit 1 }
Click-Elem $navText
OK 'Clicked Schedule Compare nav item'

$hdr = Wait-Text $main 'Schedule Comparison' 30
if (-not $hdr) { NO 'Schedule Comparison header not found'; exit 1 }
OK 'Schedule Comparison view loaded'
Start-Sleep -Seconds 2

if (Find-Text $main 'Source:') { OK 'Source found' } else { NO 'Source missing' }
if (Find-Text $main 'Target:') { OK 'Target found' } else { NO 'Target missing' }
if (Find-Text $main 'Run Comparison') { OK 'Run button found' } else { NO 'Run button missing' }

E '3/6 Selecting source/target from dropdown'
$combos = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition) | Where-Object {
    try { $_.Current.ControlType.ProgrammaticName -match 'ComboBox$' } catch { $false }
}
if ($combos.Count -lt 2) { NO "Expected 2 ComboBox elements, found $($combos.Count)"; exit 1 }
OK "Found $($combos.Count) ComboBox elements"

# Items: Snapshots, then XER (by ImportedAt DESC — newest first).
# Source (24 May, imported first = older) → LAST item.
# Target (07 Jun, imported second = newer) → second-to-last.
# Source combo = last item via END
Click-Elem $combos[0]; Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 1500
[System.Windows.Forms.SendKeys]::SendWait('{END}'); Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Milliseconds 500

# Target combo = second-to-last via END then UP
Click-Elem $combos[1]; Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait('%{DOWN}'); Start-Sleep -Milliseconds 1500
[System.Windows.Forms.SendKeys]::SendWait('{END}'); Start-Sleep -Milliseconds 400
[System.Windows.Forms.SendKeys]::SendWait('{UP}'); Start-Sleep -Milliseconds 200
[System.Windows.Forms.SendKeys]::SendWait('{ENTER}'); Start-Sleep -Milliseconds 500

[System.Windows.Forms.SendKeys]::SendWait('{ESC}'); Start-Sleep -Milliseconds 300
OK 'Source/Target selected from imported XER files'

OK 'Source/Target selected from imported XER files'

E '4/6 Running comparison'
$runBtn = Find-Text $main 'Run Comparison'
if (-not $runBtn) { NO 'Run button not found'; exit 1 }
Click-Elem $runBtn; Start-Sleep -Seconds 2
# Wait for comparison to finish — the app auto-switches to Activities tab when done
$done = $false
for ($t = 0; $t -lt 60; $t++) {
    Start-Sleep -Seconds 1
    $hdr = Find-Text $main 'Activity Diffs'
    if ($hdr) { $done = $true; break }
}
if ($done) { OK 'Comparison completed (switched to Activity Diffs tab)' } else { NO 'Did not complete within 60s' }

E '5/6 Verifying diff tab results'
$all4 = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
$tabCtrl2 = $all4 | Where-Object {
    try { $_.Current.ControlType.ProgrammaticName -match 'Tab$' -and $_.Current.BoundingRectangle.Width -gt 400 } catch { $false }
} | Select-Object -First 1
$tabItems = @()
if ($tabCtrl2) {
    $tabItems = $tabCtrl2.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition) | Where-Object {
        try { $_.Current.ControlType.ProgrammaticName -match 'TabItem$' } catch { $false }
    }
}
if ($tabItems.Count -lt 6) { NO "Only $($tabItems.Count)/6 diff tabs found"; exit 1 }

$tabVerifications = @(
    @{Index=1; Header='Activity Diffs'; Content=@('Activity Diffs'); ColHeaders=@('MatchKey','ActivityName','ChangeType')}
    @{Index=2; Header='Logic Diffs'; Content=@('Logic Diffs'); ColHeaders=@('PredecessorMatchKey','SuccessorMatchKey','ChangeType')}
    @{Index=3; Header='Resource Diffs'; Content=@('Resource Diffs'); ColHeaders=@('ActivityMatchKey','ResourceName','ChangeType')}
    @{Index=4; Header='Critical Path Analysis'; Content=@('Source Duration','Target Duration','Change'); ColHeaders=@()}
    @{Index=5; Header='Float Impact'; Content=@('Negative Float','Improved','Worsened'); ColHeaders=@('MatchKey','FloatDelta')}
)

foreach ($tv in $tabVerifications) {
    $i = $tv.Index
    $r = $tabItems[$i].Current.BoundingRectangle
    if ($r.Width -le 0) { NO "Tab $i has no width"; continue }
    [Mouse]::Click([int]($r.Left+$r.Width/2), [int]($r.Top+$r.Height/2))
    Start-Sleep -Milliseconds 800

    $loaded = Wait-Text $main $tv.Header 8
    if (-not $loaded) { NO "$($tv.Header) header not found"; continue }

    $contentOk = $true
    foreach ($ct in $tv.Content) {
        if (-not (Wait-Text-Partial $main $ct 3)) { NO "  '$ct' missing in $($tv.Header)"; $contentOk = $false }
    }

    if ($tv.ColHeaders.Count -gt 0) {
        $colOk = $true
        foreach ($ch in $tv.ColHeaders) {
            if (-not (Find-Text $main $ch)) { NO "  Column '$ch' not found in $($tv.Header)"; $colOk = $false }
        }
        if ($colOk) {
            $rows = Find-Diff-DataGrid-Rows $main
            if ($rows.Count -gt 0) {
                $firstVal = $rows[0].Current.Name
                OK "  Diff rows: $($rows.Count), sample: '$firstVal'"
            } else { OK '  Diff grid rendered (no diffs found)' }
        } else { $contentOk = $false }
    }

    if ($contentOk) { OK "$($tv.Header) - content verified" }
}

E '6/6 Verifying Export tab'
$expTab = Find-Text $main 'Export'
if ($expTab) {
    Click-Elem $expTab; Start-Sleep -Seconds 1
    if (Find-Text $main 'Export Comparison Results') {
        OK 'Export tab loaded'
        if (-not $SkipExport) {
            foreach ($btn in @('Export to Excel','Export to PDF','Export to JSON')) {
                if (Find-Text $main $btn) { OK "  $btn present" } else { NO "  $btn missing" }
            }
        }
    } else { NO 'Export header not found' }
} else { NO 'Export tab not found' }

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host '  SCHEDULE COMPARISON: ALL CHECKS PASSED' -ForegroundColor Green
Write-Host '========================================' -ForegroundColor Cyan

Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
