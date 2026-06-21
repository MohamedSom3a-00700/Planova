param([switch]$SkipBuild)

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms, System.Drawing

Add-Type @"
using System.Runtime.InteropServices;
public class Mouse {
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    public static void Click(int x, int y) { SetCursorPos(x, y); mouse_event(0x02|0x04, 0, 0, 0, 0); }
}
"@

$ScopeDescendants = [System.Windows.Automation.TreeScope]::Subtree

if (-not $SkipBuild) {
    dotnet build "$PSScriptRoot\..\Planova.UI\Planova.UI.csproj" -nologo -clp:NoSummary 2>$null
}
$exe = "$PSScriptRoot\..\Planova.UI\bin\Debug\net8.0-windows\Planova.UI.exe"
$proc = Start-Process $exe -PassThru

try {
    $main = $null
    for ($i = 0; $i -lt 60; $i++) {
        $main = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst(
            [System.Windows.Automation.TreeScope]::Children,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Planova'))
        if ($main) { break }
        Start-Sleep -Seconds 1
    }
    if (-not $main) { "Window not found"; exit 1 }

# Wait for app to initialize
Start-Sleep -Seconds 5

# Get ALL elements and dump Schedule Compare related ones
Write-Host "`n--- ELEMENTS ---" -ForegroundColor Cyan
$all = $main.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
$all | ForEach-Object {
    $ct = ''; try { $ct = $_.Current.ControlType.ProgrammaticName } catch { $ct = 'ERR' }
    $r = $_.Current.BoundingRectangle
    if ($_.Current.Name -eq 'Schedule Compare' -or $ct -match 'Button$' -and $r.Left -gt 320 -and $r.Left -lt 450 -and $r.Width -gt 100 -and $r.Top -gt 600) {
        $en = ''; try { $en = $_.Current.IsEnabled } catch { $en = 'ERR' }
        Write-Host "$ct Name='$($_.Current.Name)' Top=$([int]$r.Top) Left=$([int]$r.Left) Enabled=$en" -ForegroundColor Yellow
    }
}

Write-Host "`n--- TRYING TO CLICK EVERY 'Schedule Compare' TEXT ---" -ForegroundColor Cyan
$allTexts = $main.FindAll($ScopeDescendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Schedule Compare'))
Write-Host "Found $($allTexts.Count) elements with Name='Schedule Compare'" -ForegroundColor Cyan
foreach ($t in $allTexts) {
    $r = $t.Current.BoundingRectangle
    Write-Host "  Top=$([int]$r.Top) Left=$([int]$r.Left) W=$([int]$r.Width) H=$([int]$r.Height)" -ForegroundColor Cyan
}

# Also click on a known simple element like a nav item and check
# Use keyboard approach: Tab to find nav, then navigate
Write-Host "`n--- KEYBOARD NAVIGATION TEST ---" -ForegroundColor Cyan
# Find the first nav item and click it to focus
$firstNavText = $main.FindFirst($ScopeDescendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Dashboard'))
if ($firstNavText) {
    $r = $firstNavText.Current.BoundingRectangle
    Write-Host "Dashboard at Top=$([int]$r.Top)" -ForegroundColor Cyan
    [Mouse]::Click([int]($r.Left+$r.Width/2), [int]($r.Top+$r.Height/2))
    Start-Sleep -Milliseconds 500
    Write-Host "Clicked Dashboard" -ForegroundColor Cyan
    Start-Sleep -Seconds 2
    # Check if we navigated to Dashboard (we're already on it)
    # Now try clicking the Dashboard button again - it should at least be clickable
}

# Use keyboard: try to focus the nav area and arrow down
# First find any Text element that's a nav item (left between 320-450)
$navTexts = $all | Where-Object {
    try { $r = $_.Current.BoundingRectangle; $_.Current.Name -ne '' -and $r.Left -gt 320 -and $r.Left -lt 450 -and $r.Top -gt 600 -and $r.Top -lt 850 } catch { $false }
}
Write-Host "`nNav text elements in visible range:" -ForegroundColor Cyan
foreach ($nt in $navTexts) {
    Write-Host "  '$($nt.Current.Name)' Top=$([int]$nt.Current.BoundingRectangle.Top)" -ForegroundColor Cyan
}

# Now try to click on one of these nav items
$scText = $null
foreach ($nt in $navTexts) {
    if ($nt.Current.Name -eq 'Schedule Compare') { $scText = $nt; break }
}
if ($scText) {
    $r = $scText.Current.BoundingRectangle
    Write-Host "Clicking Schedule Compare at $([int]($r.Left+$r.Width/2)),$([int]($r.Top+$r.Height/2))" -ForegroundColor Green
    [Mouse]::Click([int]($r.Left+$r.Width/2), [int]($r.Top+$r.Height/2))
} else {
    Write-Host "No visible Schedule Compare found in nav" -ForegroundColor Red
    # Try the first visible nav item
    $first = $navTexts | Select-Object -First 1
    if ($first) {
        $r = $first.Current.BoundingRectangle
        Write-Host "Clicking first nav item '$($first.Current.Name)'" -ForegroundColor Yellow
        [Mouse]::Click([int]($r.Left+$r.Width/2), [int]($r.Top+$r.Height/2))
    }
}

Start-Sleep -Seconds 3
# Check what view we're on
$hdr = $main.FindFirst($ScopeDescendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Schedule Comparison'))
if ($hdr) { Write-Host "`nNAVIGATION SUCCESSFUL: Schedule Comparison visible!" -ForegroundColor Green }
else { 
    Write-Host "`nStill on Dashboard" -ForegroundColor Red
    
    # One more attempt: use InvokePattern directly on Buttons
    Write-Host "`nTrying InvokePattern on all nav buttons..." -ForegroundColor Cyan
    $navButtons = $all | Where-Object {
        try { $_.Current.ControlType.ProgrammaticName -match 'Button$' -and $_.Current.BoundingRectangle.Left -gt 320 -and $_.Current.BoundingRectangle.Left -lt 450 -and $_.Current.BoundingRectangle.Width -gt 100 -and $_.Current.BoundingRectangle.Top -gt 600 -and $_.Current.BoundingRectangle.Top -lt 800 } catch { $false }
    }
    Write-Host "Found $($navButtons.Count) nav buttons" -ForegroundColor Cyan
    foreach ($nb in $navButtons) {
        $r = $nb.Current.BoundingRectangle
        # Check children for text
        $kids = $nb.FindAll($ScopeDescendants, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($k in $kids) {
            try {
                if ($k.Current.Name -eq 'Schedule Compare') {
                    Write-Host "Found Schedule Compare button at Top=$([int]$r.Top)" -ForegroundColor Green
                    Write-Host "IsEnabled=$($nb.Current.IsEnabled)" -ForegroundColor Green
                    Write-Host "HasKeyboardFocus=$($nb.Current.HasKeyboardFocus)" -ForegroundColor Green
                    Write-Host "Current.Name='$($nb.Current.Name)'" -ForegroundColor Green
                    try {
                        $invoke = $nb.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
                        Write-Host "InvokePattern supported, calling Invoke..." -ForegroundColor Green
                        $invoke.Invoke()
                        Write-Host "Invoke succeeded!" -ForegroundColor Green
                        break
                    } catch { Write-Host "InvokePattern.Invoke() failed: $_" -ForegroundColor Red }
                }
            } catch {}
        }
    }
}

Start-Sleep -Seconds 3
$hdr2 = $main.FindFirst($ScopeDescendants, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Schedule Comparison'))
if ($hdr2) { Write-Host "`nNAVIGATION SUCCESSFUL after InvokePattern!" -ForegroundColor Green }
else { Write-Host "`nStill on Dashboard after all attempts" -ForegroundColor Red; exit 1 }
}
finally {
    if ($proc -and $proc.Id) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
}
