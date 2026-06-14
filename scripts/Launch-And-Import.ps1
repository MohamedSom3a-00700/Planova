<#
.SYNOPSIS
    Launches Planova, selects a project, opens Primavera Studio, and imports an XER file via UI automation.
#>
param(
    [string]$ProjectName = "",
    [string]$XerPath = "",
    [string]$AppPath = ""
)

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# User32 mouse click support
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

if (-not $XerPath) {
    $XerPath = Resolve-Path "tests\Silver Sand  - Land Scape - Update 07- jun- 2026.xer" -ErrorAction Stop
}
$XerPath = Resolve-Path $XerPath -ErrorAction Stop

function Write-ErrorMsg {
    param([string]$Context, [string]$Detail)
    Write-Host "  [ERROR] $Context" -ForegroundColor Red
    if ($Detail) { Write-Host "    $Detail" -ForegroundColor DarkRed }
}

function Dump-Elements {
    param([string]$Label = "", [int]$Max = 15)
    Write-Host "  [DUMP] Elements $Label" -ForegroundColor DarkGray
    $all = $mainWindow.FindAll([System.Windows.Automation.TreeScope]::Subtree,
        [System.Windows.Automation.Condition]::TrueCondition)
    $c = 0
    for ($i = 0; $i -lt $all.Count -and $c -lt $Max; $i++) {
        $e = $all[$i]
        $n = $e.Current.Name
        $t = $e.Current.ControlTypeName
        $r = $e.Current.BoundingRectangle
        if ($n) {
            $hasI = $false; try { $null = $e.GetSupportedPatterns(); if ($e.GetSupportedPatterns() -contains [System.Windows.Automation.InvokePattern]::Pattern) { $hasI = $true } } catch {}
            Write-Host "    [$t] '$($n.Substring(0,[Math]::Min($n.Length,60)))' invoke=$hasI rect=$($r.Width)x$($r.Height)" -ForegroundColor DarkGray
            $c++
        }
    }
}

function Click-MouseAt {
    param([System.Windows.Automation.AutomationElement]$Element)
    $bbox = $Element.Current.BoundingRectangle
    if ($bbox.Width -le 0 -or $bbox.Height -le 0) { Write-ErrorMsg "Click" "Element has no bounding rect"; return $false }
    $x = [int]($bbox.Left + $bbox.Width / 2)
    $y = [int]($bbox.Top + $bbox.Height / 2)
    Write-Host "  Mouse click at ($x, $y)" -ForegroundColor DarkGray
    [Mouse]::Click($x, $y)
    Start-Sleep -Milliseconds 300
    return $true
}

function Click-ElementByText {
    param([string]$Text, [int]$TimeoutSeconds = 15)
    for ($t = 0; $t -lt $TimeoutSeconds; $t++) {
        $all = $mainWindow.FindAll([System.Windows.Automation.TreeScope]::Subtree,
            [System.Windows.Automation.Condition]::TrueCondition)
        for ($i = 0; $i -lt $all.Count; $i++) {
            if ($all[$i].Current.Name -eq $Text) {
                # 1) Try InvokePattern directly on the matched element (it might be a Button)
                try {
                    $invoke = $all[$i].GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                    $invoke.Invoke(); return $true
                }
                catch {}
                # 2) Try mouse click on the element directly
                $bbox = $all[$i].Current.BoundingRectangle
                if ($bbox.Width -gt 0 -and $bbox.Height -gt 0) {
                    $x = [int]($bbox.Left + $bbox.Width / 2)
                    $y = [int]($bbox.Top + $bbox.Height / 2)
                    [Mouse]::Click($x, $y)
                    Start-Sleep -Milliseconds 300
                    return $true
                }
                # 3) Walk up parent chain to find clickable parent
                $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
                $cur = $walker.GetParent($all[$i])
                $depth = 0
                while ($cur -and $depth -lt 20) {
                    try {
                        $invoke = $cur.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                        $invoke.Invoke(); return $true
                    }
                    catch {}
                    $bbox = $cur.Current.BoundingRectangle
                    if ($bbox.Width -gt 0 -and $bbox.Height -gt 0) {
                        $x = [int]($bbox.Left + $bbox.Width / 2)
                        $y = [int]($bbox.Top + $bbox.Height / 2)
                        [Mouse]::Click($x, $y)
                        Start-Sleep -Milliseconds 300
                        return $true
                    }
                    $cur = $walker.GetParent($cur)
                    $depth++
                }
            }
        }
        Write-Host "  Waiting for '$Text'... (${t}s)" -ForegroundColor DarkGray
        Start-Sleep -Seconds 1
    }
    Write-ErrorMsg "Click-ElementByText" "'$Text' not found/clickable after ${TimeoutSeconds}s"
    return $false
}

# Launch Planova
Write-Host "Launching Planova..." -ForegroundColor Cyan
$appDir = Resolve-Path "$PSScriptRoot\..\Planova.UI\bin\Debug\net8.0-windows"
$exePath = "$appDir\Planova.UI.exe"
if (-not (Test-Path $exePath)) {
    dotnet build "$PSScriptRoot\..\Planova.UI\Planova.UI.csproj" -nologo -clp:NoSummary | Out-Null
}
$proc = Start-Process -FilePath $exePath -WindowStyle Normal -PassThru

# Wait for main window
Write-Host "Waiting for Planova window..." -ForegroundColor Cyan
$mainWindow = $null
for ($t = 0; $t -lt 60; $t++) {
    Start-Sleep -Seconds 1
    try {
        $mainWindow = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            (New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::NameProperty, "Planova")))
        if ($mainWindow) { Write-Host "  Found after ${t}s" -ForegroundColor Green; break }
    }
    catch {}
}
if (-not $mainWindow) { Write-Host "Timed out." -ForegroundColor Red; exit 1 }
Start-Sleep -Seconds 2

# --- Step 1: Select project ---
Write-Host "Selecting project..." -ForegroundColor Yellow
$projectSel = $mainWindow.FindFirst([System.Windows.Automation.TreeScope]::Subtree,
    (New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, "ProjectSelector")))
if ($projectSel) {
    # Expand the dropdown
    try {
        $expand = $projectSel.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        $expand.Expand()
        Start-Sleep -Milliseconds 500
    }
    catch { Write-ErrorMsg "ProjectSelector" "Expand failed: $_" }
    $items = $projectSel.FindAll([System.Windows.Automation.TreeScope]::Subtree,
        [System.Windows.Automation.Condition]::TrueCondition)
    Write-Host "  [DIAG] Project items: $($items.Count)" -ForegroundColor DarkGray
    $selected = $false
    # Try to match by name
    foreach ($item in $items) {
        $n = $item.Current.Name
        if ($ProjectName -and $n -like "*$ProjectName*") {
            Write-Host "  Selecting: $n" -ForegroundColor Green
            Click-MouseAt $item; $selected = $true; break
        }
    }
    if (-not $selected) {
        # Pick first non-DTO name
        foreach ($item in $items) {
            $n = $item.Current.Name
            if ($n.Length -gt 2 -and -not $n.Contains("Dto")) {
                Write-Host "  Selecting: $n" -ForegroundColor Green
                Click-MouseAt $item; $selected = $true; break
            }
        }
    }
    if (-not $selected) { Write-ErrorMsg "ProjectSelector" "No usable project found" }
    # Close dropdown
    [System.Windows.Forms.SendKeys]::SendWait("{ESCAPE}")
}
else { Write-ErrorMsg "ProjectSelector" "Not found" }

Start-Sleep -Seconds 2

# --- Step 2: Open Primavera Studio ---
Write-Host "Opening Primavera Studio..." -ForegroundColor Yellow
# Find the nav button with visible bounding rect
$navItems = $mainWindow.FindFirst([System.Windows.Automation.TreeScope]::Subtree,
    (New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, "NavItems")))
$clicked = $false
if ($navItems) {
    $all = $navItems.FindAll([System.Windows.Automation.TreeScope]::Subtree,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($el in $all) {
        if ($el.Current.Name -eq "Primavera Studio") {
            $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
            $cur = $walker.GetParent($el)
            while ($cur) {
                $bbox = $cur.Current.BoundingRectangle
                if ($bbox.Width -gt 0 -and $bbox.Height -gt 0) {
                    Write-Host "  Clicking Primavera Studio nav item" -ForegroundColor Green
                    [Mouse]::Click([int]($bbox.Left+$bbox.Width/2), [int]($bbox.Top+$bbox.Height/2))
                    Start-Sleep -Milliseconds 500
                    $clicked = $true; break
                }
                $cur = $walker.GetParent($cur)
            }
            if (-not $clicked) {
                # Click the text element directly
                $bbox = $el.Current.BoundingRectangle
                if ($bbox.Width -gt 0) {
                    [Mouse]::Click([int]($bbox.Left+$bbox.Width/2), [int]($bbox.Top+$bbox.Height/2))
                    Start-Sleep -Milliseconds 500; $clicked = $true
                }
            }
            break
        }
    }
}
if (-not $clicked) { Write-ErrorMsg "PrimaveraStudio" "Could not find/click Primavera Studio nav item" }

Start-Sleep -Seconds 3

# --- Step 3: Click Browse and select XER file ---
Write-Host "Browsing for XER file..." -ForegroundColor Yellow

# Wait for Import tab to be visible by checking for "Import XER File" header text
$foundImport = $false
for ($t = 0; $t -lt 15; $t++) {
    $all = $mainWindow.FindAll([System.Windows.Automation.TreeScope]::Subtree,
        [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($el in $all) {
        if ($el.Current.Name -eq "Import XER File") { $foundImport = $true; break }
    }
    if ($foundImport) { Write-Host "  Import view loaded" -ForegroundColor Green; break }
    if ($t -eq 0) { Write-Host "  Waiting for import view..." -ForegroundColor DarkGray }
    Start-Sleep -Seconds 1
}
if (-not $foundImport) { Write-ErrorMsg "ImportView" "Import XER File header not found - Primavera Studio may not have opened" }

# Click Browse — scope search to Import view area
$allElements = $mainWindow.FindAll([System.Windows.Automation.TreeScope]::Subtree,
    [System.Windows.Automation.Condition]::TrueCondition)
$browseBtn = $null
$importHeader = $null
$importViewStart = -1
# Find "Import XER File" header to know where the import view is
for ($i = 0; $i -lt $allElements.Count; $i++) {
    if ($allElements[$i].Current.Name -eq "Import XER File") { $importHeader = $allElements[$i]; $importViewStart = $i }
}
if ($importHeader) {
    # Search from the header onward for the FIRST "Browse" — that's the import view's browse
    for ($i = $importViewStart; $i -lt $allElements.Count; $i++) {
        if ($allElements[$i].Current.Name -eq "Browse") { $browseBtn = $allElements[$i]; break }
    }
}
if (-not $browseBtn) {
    # Fallback: find any "Browse" by looking for the one with "Import" nearby
    for ($i = 0; $i -lt $allElements.Count; $i++) {
        if ($allElements[$i].Current.Name -eq "Browse") {
            # Check if nearby elements contain "Import" 
            $start = [Math]::Max(0, $i - 20)
            $end = [Math]::Min($allElements.Count - 1, $i + 20)
            for ($j = $start; $j -le $end; $j++) {
                if ($allElements[$j].Current.Name -like "*Import*") { $browseBtn = $allElements[$i]; break }
            }
            if ($browseBtn) { break }
        }
    }
}
if ($browseBtn) {
    Write-Host "  Found Browse button in Import view" -ForegroundColor Green
    # Try InvokePattern
    $clicked = $false
    try {
        $invoke = $browseBtn.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        $invoke.Invoke(); $clicked = $true
    }
    catch {}
    if (-not $clicked) {
        $bbox = $browseBtn.Current.BoundingRectangle
        if ($bbox.Width -gt 0) {
            [Mouse]::Click([int]($bbox.Left+$bbox.Width/2), [int]($bbox.Top+$bbox.Height/2))
            $clicked = $true
        }
    }
    if (-not $clicked) {
        Write-ErrorMsg "Browse" "Found Browse but could not click it"
        exit 1
    }
}
else {
    Write-ErrorMsg "Browse" "Browse button not found in Import view"
    exit 1
}
Start-Sleep -Seconds 2

# --- Step 4: Find file dialog and send file path ---
Write-Host "Looking for file dialog..." -ForegroundColor DarkGray
$fileDialog = $null
for ($t = 0; $t -lt 30; $t++) {
    try {
        # Search all top-level windows and descendants
        foreach ($scope in @([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.TreeScope]::Descendants)) {
            $allTop = [System.Windows.Automation.AutomationElement]::RootElement.FindAll($scope,
                [System.Windows.Automation.Condition]::TrueCondition) 2>$null
            foreach ($w in $allTop) {
                $wn = $w.Current.Name; $wt = $w.Current.ControlTypeName
                if ($wt -eq "window" -and ($wn -eq "Select Primavera XER File" -or $wn -like "*XER*" -or $wn -like "*Open*" -or $wn -like "*Select*")) {
                    $fileDialog = $w
                    Write-Host "  Found UIA window: '$wn'" -ForegroundColor Green; break
                }
            }
            if ($fileDialog) { break }
        }
    }
    catch {}
    if ($fileDialog) { break }
    Start-Sleep -Milliseconds 500
}

if (-not $fileDialog) {
    Write-ErrorMsg "FileDialog" "Not found via UIA top-level windows, checking all windows again..."
    # Broader UIA search: look at ALL windows (including owned/popup)
    try {
        $allWindows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
            [System.Windows.Automation.TreeScope]::Descendants,
            [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($w in $allWindows) {
            $wn = $w.Current.Name; $wt = $w.Current.ControlTypeName
            if ($wt -eq "window" -and $wn -eq "Select Primavera XER File") {
                $fileDialog = $w
                Write-Host "  Found exact title: '$wn'" -ForegroundColor Green; break
            }
        }
    }
    catch { Write-ErrorMsg "FileDialog" "Error during broad search: $_" }
}

if (-not $fileDialog) {
    Write-ErrorMsg "FileDialog" "No file dialog found after Browse click"
    Dump-Elements -Label "diagnostic" -Max 40
    exit 1
}

try { $fileDialog.SetFocus() } catch {}
Start-Sleep -Milliseconds 500

Write-Host "  Sending file path..." -ForegroundColor DarkGray
[System.Windows.Forms.SendKeys]::SendWait("^a")
Start-Sleep -Milliseconds 100
[System.Windows.Forms.SendKeys]::SendWait($XerPath)
Start-Sleep -Milliseconds 300
[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Write-Host "  File dialog submitted" -ForegroundColor Green

# --- Step 5: Wait for parse, then Commit ---
Write-Host "Waiting for import to complete..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

$foundCommit = $false
for ($t = 0; $t -lt 60; $t++) {
    if (Click-ElementByText "Commit" -TimeoutSeconds 2) {
        Write-Host "Import committed!" -ForegroundColor Green
        $foundCommit = $true; break
    }
    if ($t % 10 -eq 0) { Write-Host "  Waiting for Commit button... (${t}s)" -ForegroundColor DarkGray }
}

if (-not $foundCommit) {
    Write-ErrorMsg "Commit" "Commit button did not appear within 60s"
}

Write-Host "`nDone. Planova is running with imported data." -ForegroundColor Cyan
