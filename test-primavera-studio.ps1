param([switch]$SkipBuild)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# ═══════════════════════════════════════════════════
# BUILD
# ═══════════════════════════════════════════════════
if (-not $SkipBuild) {
    Write-Host "Building UI..." -ForegroundColor Cyan
    dotnet build "$ProjectRoot\Planova.UI\Planova.UI.csproj" -v q /nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
}

# ═══════════════════════════════════════════════════
# SEED demo project into DB if none exists
# ═══════════════════════════════════════════════════
Write-Host "Seeding demo project (if needed)..." -ForegroundColor Cyan
$seedCsproj = "$env:TEMP\PlanovaSeed\PlanovaSeed.csproj"
if (-not (Test-Path $seedCsproj)) {
    New-Item -ItemType Directory -Path "$env:TEMP\PlanovaSeed" -Force | Out-Null
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$ProjectRoot\Planova.Persistence\Planova.Persistence.csproj" />
    <ProjectReference Include="$ProjectRoot\Planova.Domain\Planova.Domain.csproj" />
    <ProjectReference Include="$ProjectRoot\Planova.Shared\Planova.Shared.csproj" />
  </ItemGroup>
</Project>
"@ | Set-Content $seedCsproj
@"
using Microsoft.EntityFrameworkCore;
using Planova.Persistence.DbContext;
var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Planova","planova.db");
var ob = new DbContextOptionsBuilder<PlanovaDbContext>(); ob.UseSqlite($"Data Source={dbPath}");
using var ctx = new PlanovaDbContext(ob.Options);
if (await ctx.Set<Planova.Domain.Entities.Project>().AnyAsync()) { Console.Write("EXISTS"); return; }
ctx.Set<Planova.Domain.Entities.Project>().Add(new Planova.Domain.Entities.Project{Name="Demo Project",Code="DEMO-001",Status="Active",StartDate=DateTime.UtcNow,FinishDate=DateTime.UtcNow.AddMonths(6),CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow});
await ctx.SaveChangesAsync(); Console.Write("SEEDED");
"@ | Set-Content "$env:TEMP\PlanovaSeed\Program.cs"
    dotnet build $seedCsproj -v q /nologo 2>&1 | Out-String | Out-Null
}
$seedResult = dotnet run --project $seedCsproj --no-build 2>&1
Write-Host "  Seed result: $seedResult" -ForegroundColor DarkGray

# ═══════════════════════════════════════════════════
# KILL old instances & LAUNCH
# ═══════════════════════════════════════════════════
Get-Process -Name "Planova.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -like "*Planova.UI*" } | Stop-Process -Force
Start-Sleep -Seconds 2

Write-Host "Launching Planova..." -ForegroundColor Cyan
$proc = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$ProjectRoot\Planova.UI\Planova.UI.csproj`"" -PassThru
Write-Host "Waiting for app..." -ForegroundColor Cyan
Start-Sleep -Seconds 12

# ═══════════════════════════════════════════════════
# UI AUTOMATION SETUP
# ═══════════════════════════════════════════════════
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, WindowsBase, System.Windows.Forms

$ScopeDescendants = [System.Windows.Automation.TreeScope]::Descendants
$InvokePattern   = [System.Windows.Automation.InvokePattern]::Pattern
$ExpandCollapse  = [System.Windows.Automation.ExpandCollapsePattern]::Pattern
$SelectionItem   = [System.Windows.Automation.SelectionItemPattern]::Pattern
$ValuePattern    = [System.Windows.Automation.ValuePattern]::Pattern
$NameProp        = [System.Windows.Automation.AutomationElement]::NameProperty
$CtrlTypeProp    = [System.Windows.Automation.AutomationElement]::ControlTypeProperty
$IsEnabledProp   = [System.Windows.Automation.AutomationElement]::IsEnabledProperty
$CtrlType_Window     = [System.Windows.Automation.ControlType]::Window
$CtrlType_ComboBox   = [System.Windows.Automation.ControlType]::ComboBox
$CtrlType_ListItem   = [System.Windows.Automation.ControlType]::ListItem
$CtrlType_Button     = [System.Windows.Automation.ControlType]::Button
$CtrlType_RadioButton = [System.Windows.Automation.ControlType]::RadioButton
$CtrlType_Text       = [System.Windows.Automation.ControlType]::Text
$CtrlType_PopupMenu  = [System.Windows.Automation.ControlType]::PopupMenu
$CtrlType_List       = [System.Windows.Automation.ControlType]::List
$CtrlType_TabItem    = [System.Windows.Automation.ControlType]::TabItem
$CtrlType_Tab        = [System.Windows.Automation.ControlType]::Tab

function Find-Window {
    param([string]$Title)
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $cond = [System.Windows.Automation.PropertyCondition]::new($CtrlTypeProp, $CtrlType_Window)
    $windows = $root.FindAll($ScopeDescendants, $cond)
    foreach ($w in $windows) { if ($w.Current.Name -eq $Title) { return $w } }
    return $null
}

function Find-Element {
    param([System.Windows.Automation.AutomationElement]$Parent, [string]$Name)
    $cond = [System.Windows.Automation.PropertyCondition]::new($NameProp, $Name)
    return $Parent.FindFirst($ScopeDescendants, $cond)
}

function Find-ElementsByType {
    param([System.Windows.Automation.AutomationElement]$Parent, $CtrlType)
    $cond = [System.Windows.Automation.PropertyCondition]::new($CtrlTypeProp, $CtrlType)
    return $Parent.FindAll($ScopeDescendants, $cond)
}

function Click-Element {
    param([System.Windows.Automation.AutomationElement]$elem)
    # 1) InvokePattern
    try { $invoke = $elem.GetCurrentPattern($InvokePattern) -as [System.Windows.Automation.InvokePattern]
          if ($invoke) { $invoke.Invoke(); return $true } } catch {}
    # 2) LegacyIAccessible
    try { $legacy = $elem.GetCurrentPattern([System.Windows.Automation.LegacyIAccessiblePattern]::Pattern) -as [System.Windows.Automation.LegacyIAccessiblePattern]
          if ($legacy) { $legacy.DoDefaultAction(); return $true } } catch {}
    # 3) TogglePattern
    try { $togg = $elem.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern) -as [System.Windows.Automation.TogglePattern]
          if ($togg) { $togg.Toggle(); return $true } } catch {}
    # 4) SelectionItemPattern
    try { $sel = $elem.GetCurrentPattern($SelectionItem) -as [System.Windows.Automation.SelectionItemPattern]
          if ($sel) { $sel.Select(); return $true } } catch {}
    # 5) Mouse click at center
    try {
        $rect = $elem.Current.BoundingRectangle
        if ($rect.Width -gt 0 -and $rect.Height -gt 0) {
            $sig = @'
using System.Runtime.InteropServices;
public class MouseClicker {
    [DllImport("user32.dll")] public static extern void SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
    public static void Click(int x, int y) {
        SetCursorPos(x, y);
        mouse_event(0x02|0x04, 0, 0, 0, 0);
    }
}
'@
            Add-Type -TypeDefinition $sig -ErrorAction SilentlyContinue
            $x = [int]($rect.Left + $rect.Width / 2)
            $y = [int]($rect.Top + $rect.Height / 2)
            [MouseClicker]::Click($x, $y)
            return $true
        }
    } catch {}
    return $false
}

function Select-ComboBoxItem {
    param([System.Windows.Automation.AutomationElement]$ComboBox, [int]$Index = 0)
    # Expand
    try {
        $ecp = $ComboBox.GetCurrentPattern($ExpandCollapse) -as [System.Windows.Automation.ExpandCollapsePattern]
        if ($ecp) { $ecp.Expand(); Start-Sleep -Milliseconds 1000 }
    } catch {}

    # Find list items - try multiple approaches
    Start-Sleep -Milliseconds 500

    # Approach 1: Find List descendant
    $list = $ComboBox.FindFirst($ScopeDescendants,
        [System.Windows.Automation.PropertyCondition]::new($CtrlTypeProp, $CtrlType_List))
    if ($list) {
        $items = $list.FindAll($ScopeDescendants,
            [System.Windows.Automation.PropertyCondition]::new($CtrlTypeProp, $CtrlType_ListItem))
        if ($items -and $items.Count -gt $Index) {
            $item = $items[$Index]
            Write-Host "  Selecting item: $($item.Current.Name)" -ForegroundColor DarkGray
            $ok = Click-Element -elem $item
            if ($ok) { return $true }
            # Try SelectionItemPattern
            try { $sip = $item.GetCurrentPattern($SelectionItem) -as [System.Windows.Automation.SelectionItemPattern]
                  if ($sip) { $sip.Select(); return $true } } catch {}
        }
    }

    # Approach 2: Find all ListItems directly under ComboBox
    $items2 = $ComboBox.FindAll($ScopeDescendants,
        [System.Windows.Automation.PropertyCondition]::new($CtrlTypeProp, $CtrlType_ListItem))
    if ($items2 -and $items2.Count -gt $Index) {
        $ok = Click-Element -elem $items2[$Index]
        if ($ok) { return $true }
    }

    # Approach 3: Keyboard - type Down Arrow then Enter
    $ComboBox.SetFocus()
    Start-Sleep -Milliseconds 200
    [System.Windows.Forms.SendKeys]::SendWait("{DOWN}")
    Start-Sleep -Milliseconds 200
    [System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
    Start-Sleep -Milliseconds 500
    return $true
}

$results = @{}; $allPass = $true
function Test-Section {
    param([string]$Label, [scriptblock]$Action)
    Write-Host "`n>>> $Label" -ForegroundColor Yellow
    try { & $Action; Write-Host "  [PASS] $Label" -ForegroundColor Green; $results[$Label] = "PASS" }
    catch { Write-Host "  [FAIL] $Label - $_" -ForegroundColor Red; $results[$Label] = "FAIL"; $script:allPass = $false }
}

try {
    # ═══ FIND WINDOW ═══════════════════════════════
    Write-Host "`nLooking for Planova window..." -ForegroundColor Cyan
    $window = $null
    for ($i = 0; $i -lt 30; $i++) {
        $window = Find-Window -Title "Planova"
        if ($window) { break }
        Start-Sleep -Seconds 1
    }
    if (-not $window) { throw "Planova window not found" }
    Write-Host "  [PASS] Window found" -ForegroundColor Green

    # ═══ SELECT PROJECT ════════════════════════════
    Test-Section "Project selection" {
        $comboBox = $null
        for ($i = 0; $i -lt 10; $i++) {
            $comboBox = Find-ElementsByType -Parent $window -CtrlType $CtrlType_ComboBox
            if ($comboBox -and $comboBox.Count -gt 0) { $comboBox = $comboBox[0]; break }
            Start-Sleep -Milliseconds 500
        }
        if (-not $comboBox) {
            # Project may already be selected - check if Primavera Studio is enabled
            $navP = Find-Element -Parent $window -Name "Primavera Studio"
            if ($navP -and $navP.Current.IsEnabled) {
                Write-Host "  Project already active" -ForegroundColor Green; return
            }
            throw "ComboBox not found and no active project"
        }
        Write-Host "  Found ComboBox: $($comboBox.Current.Name)" -ForegroundColor Green
        $ok = Select-ComboBoxItem -ComboBox $comboBox -Index 0
        if (-not $ok) { throw "Could not select project from ComboBox" }
        Start-Sleep -Seconds 2

        # Verify project was selected
        $navP = Find-Element -Parent $window -Name "Primavera Studio"
        if ($navP -and (-not $navP.Current.IsEnabled)) { throw "Project selected but Primavera Studio still disabled" }
        Write-Host "  Project selected successfully" -ForegroundColor Green
    }

    # ═══ OPEN PRIMAVERA STUDIO ════════════════════
    Test-Section "Primavera Studio navigation" {
        $navP = $null
        for ($i = 0; $i -lt 10; $i++) {
            $navP = Find-Element -Parent $window -Name "Primavera Studio"
            if ($navP -and $navP.Current.IsEnabled) { break }
            Start-Sleep -Milliseconds 500
        }
        if (-not $navP) { throw "Primavera Studio nav item not found" }
        # Walk up to find the clickable Button parent
        $walker = [System.Windows.Automation.TreeWalker]::new([System.Windows.Automation.Automation]::RawViewCondition)
        $clickTarget = $navP
        $parent = $walker.GetParent($navP)
        while ($parent -and $parent.Current.ControlType.ProgrammaticName -ne "Button" -and $parent.Current.ControlType.ProgrammaticName -ne "Window") {
            if ($parent.Current.ControlType -eq $CtrlType_Button) { $clickTarget = $parent; break }
            $parent = $walker.GetParent($parent)
        }
        if ($clickTarget -eq $navP) {
            # Try the first Button ancestor
            $buttons = Find-ElementsByType -Parent $window -CtrlType $CtrlType_Button
            foreach ($b in $buttons) {
                if ($b.Current.Name -eq "Primavera Studio" -or $b.Current.Name -eq "") {
                    $texts = $b.FindAll($ScopeDescendants,
                        [System.Windows.Automation.PropertyCondition]::new($CtrlTypeProp, $CtrlType_Text))
                    foreach ($t in $texts) { if ($t.Current.Name -eq "Primavera Studio") { $clickTarget = $b; break } }
                    if ($clickTarget -ne $navP) { break }
                }
            }
        }
        Write-Host "  Click target: $($clickTarget.Current.ControlType.ProgrammaticName) '$($clickTarget.Current.Name)'" -ForegroundColor DarkGray
        $patterns = $clickTarget.GetSupportedPatterns()
        Write-Host "  Patterns: $($patterns | ForEach-Object { $_.ProgrammaticName })" -ForegroundColor DarkGray
        $ok = Click-Element -elem $clickTarget
        if (-not $ok) { throw "Could not click Primavera Studio" }
        Start-Sleep -Seconds 2
        Write-Host "  Primavera Studio opened" -ForegroundColor Green
    }

    # ═══ TEST SECTIONS ══════════════════════════════
    $sections = @(
        @{ Name="Import";    Expected=(@("Import XER File", "Browse", "Commit", "Cancel")) },
        @{ Name="Workspace"; Expected=(@("Save Changes", "Validate")) },
        @{ Name="Validate";  Expected=(@("Schedule Validation", "Run Validation")) },
        @{ Name="Repair";    Expected=(@("Repair Schedule Issues", "Load Suggestions", "Apply All Fixes")) },
        @{ Name="Export";    Expected=(@("Export to XER", "Browse", "Export")) }
    )
    foreach ($section in $sections) {
        $name = $section.Name
        $expectedTexts = $section.Expected
        Test-Section "Section: $name" {
            $tabItem = $null
            for ($i = 0; $i -lt 5; $i++) {
                $tabItems = Find-ElementsByType -Parent $window -CtrlType $CtrlType_TabItem
                if ($tabItems) { $tabItem = $tabItems | Where-Object { $_.Current.Name -eq $name } | Select-Object -First 1; if ($tabItem) { break } }
                Start-Sleep -Milliseconds 300
            }
            if (-not $tabItem) { throw "Tab item '$name' not found" }
            Write-Host "  Found tab item" -ForegroundColor DarkGray

            $ok = Click-Element -elem $tabItem
            if (-not $ok) { throw "Could not click tab '$name'" }
            Start-Sleep -Milliseconds 800
            Write-Host "  Clicked tab '$name'" -ForegroundColor Green

            # Verify content elements are visible
            $foundCount = 0
            foreach ($expected in $expectedTexts) {
                $elem = Find-Element -Parent $window -Name $expected
                if ($elem) {
                    $foundCount++
                    Write-Host "  [CONTENT] Found: '$expected'" -ForegroundColor Green
                } else {
                    Write-Host "  [MISSING] '$expected' not visible"
                }
            }
            if ($foundCount -eq 0) { throw "No expected content elements found for '$name' section" }
        }
    }

    # ═══ RESULTS ═══════════════════════════════════
    Write-Host "`n============================================" -ForegroundColor Cyan
    Write-Host "  Primavera Studio - Test Results" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    foreach ($key in $results.Keys) {
        $color = if ($results[$key] -eq "PASS") { "Green" } else { "Red" }
        Write-Host "  $key`: $($results[$key])" -ForegroundColor $color
    }
    Write-Host "============================================" -ForegroundColor Cyan
    if ($allPass) { Write-Host "`nAll tests PASSED!" -ForegroundColor Green }
    else          { Write-Host "`nSome tests FAILED" -ForegroundColor Red }

    Write-Host "`nApp is running - verify the UI visually." -ForegroundColor Cyan
    Write-Host "Close the app when done, then press Enter..." -ForegroundColor Gray
    Read-Host
}
catch {
    Write-Host "`nERROR: $_" -ForegroundColor Red
    Read-Host "Press Enter to exit..."
}
finally {
    if ($proc -and (-not $proc.HasExited)) { Write-Host "Closing Planova..." -ForegroundColor Cyan; $proc.Kill() }
}
Write-Host "Done." -ForegroundColor Green
