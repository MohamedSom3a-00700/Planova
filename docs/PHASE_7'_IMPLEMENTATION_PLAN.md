# Phase 8: Project Page Enhancement + Studio Document Integration

## Context

The current Project page is minimal (code, name, dates, currency, client/contractor links). Users need a full document management system per project — upload drawings, BOQ, specs, contracts; scan a local folder; pick documents in studios instead of re-browsing; see location on a map; auto-generate a QR code for location; upload a project logo; and see richer project cards in the list. Studios need to validate that required documents exist before allowing work.

---

## 1. Domain Layer — new entities & fields

### 1a. Add fields to `Planova.Domain/Entities/Project.cs`

```csharp
public string? LogoPath { get; set; }           // path to copied logo file
public string? DocumentsFolder { get; set; }    // base folder path (manual or scanned)
public double? Latitude { get; set; }
public double? Longitude { get; set; }
public string? QrCodePath { get; set; }         // auto-generated QR image path
```

### 1b. New entity `Planova.Domain/Entities/ProjectDocument.cs`

```csharp
public class ProjectDocument
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;  // relative to base docs folder
    public string DocumentType { get; set; } = string.Empty;  // Boq/Spec/Contract/Drawing/Other
    public string FileExtension { get; set; } = string.Empty; // .pdf/.xlsx/.dwg/etc
    public long FileSizeBytes { get; set; }
    public string? Notes { get; set; }
    public DateTime UploadedAt { get; set; }
    public Project Project { get; set; } = null!;
}
```

Allowed extensions: `.pdf`, `.xlsx`, `.xls`, `.xlsm`, `.dwg`, `.dxf`, `.doc`, `.docx`

DocumentType values: `Boq`, `Spec`, `Contract`, `Drawing`, `Other`

---

## 2. Application Layer

### 2a. New DTOs in `Planova.Application/Dto/`

- **`ProjectDocumentDto.cs`** — record with all ProjectDocument fields + full absolute path
- **`AddProjectDocumentDto.cs`** — `(int ProjectId, string SourceFilePath, string DocumentType, string? Notes)`
- **`ScanFolderDto.cs`** — `(int ProjectId, string FolderPath)`
- **Update `CreateProjectDto.cs`** — add `string? LogoSourcePath, string? DocumentsFolder, double? Latitude, double? Longitude`
- **Update `UpdateProjectDto.cs`** — same additions as CreateProjectDto
- **Update `ProjectDetailDto.cs`** — add `string? LogoPath, string? DocumentsFolder, double? Latitude, double? Longitude, string? QrCodePath, List<ProjectDocumentDto> Documents`
- **Update `ProjectSummaryDto.cs`** — add `string? ContractorName, int DocumentCount, string? LogoPath`

### 2b. New service interface `Planova.Application/Services/IProjectDocumentService.cs`

```csharp
Task<IEnumerable<ProjectDocumentDto>> GetByProjectAsync(int projectId, CancellationToken ct = default);
Task<IEnumerable<ProjectDocumentDto>> GetByTypeAsync(int projectId, string documentType, CancellationToken ct = default);
Task<ProjectDocumentDto> AddAsync(AddProjectDocumentDto dto, CancellationToken ct = default);
Task DeleteAsync(int documentId, CancellationToken ct = default);
Task<IEnumerable<ProjectDocumentDto>> ScanFolderAsync(ScanFolderDto dto, CancellationToken ct = default);
bool IsExtensionAllowed(string extension);
```

### 2c. New repository interface `Planova.Application/Repositories/IProjectDocumentRepository.cs`

```csharp
Task<IEnumerable<ProjectDocument>> GetByProjectIdAsync(int projectId, CancellationToken ct = default);
Task<IEnumerable<ProjectDocument>> GetByProjectIdAndTypeAsync(int projectId, string documentType, CancellationToken ct = default);
Task<ProjectDocument?> GetByIdAsync(int id, CancellationToken ct = default);
Task<ProjectDocument> AddAsync(ProjectDocument document, CancellationToken ct = default);
Task DeleteAsync(ProjectDocument document, CancellationToken ct = default);
Task<bool> PathExistsAsync(int projectId, string relativePath, CancellationToken ct = default);
```

### 2d. Update `Planova.Application/Mappings/MappingProfile.cs`

- Update `ToSummaryDto()` for Project — include ContractorName, DocumentCount (from Documents count), LogoPath
- Update `ToDetailDto()` for Project — include all new fields + map Documents collection
- Add `ToDto()` extension for `ProjectDocument`

---

## 3. Persistence Layer

### 3a. New `Planova.Persistence/EntityConfigurations/ProjectDocumentConfiguration.cs`

```csharp
builder.ToTable("ProjectDocuments");
builder.HasKey(e => e.Id);
builder.Property(e => e.FileName).IsRequired().HasMaxLength(260);
builder.Property(e => e.RelativePath).IsRequired().HasMaxLength(1000);
builder.Property(e => e.DocumentType).IsRequired().HasMaxLength(30);
builder.Property(e => e.FileExtension).IsRequired().HasMaxLength(10);
builder.Property(e => e.UploadedAt).HasDefaultValueSql("datetime('now')");
builder.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ProjectDocuments_ProjectId");
builder.HasIndex(new[] { "ProjectId", "DocumentType" }).HasDatabaseName("IX_ProjectDocuments_ProjectId_Type");
builder.HasOne(e => e.Project).WithMany(p => p.Documents)
    .HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
```

### 3b. Update `Planova.Persistence/DbContext/PlanovaDbContext.cs`

- Add `public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();`
- Add `modelBuilder.ApplyConfiguration(new ProjectDocumentConfiguration());` in `OnModelCreating`

### 3c. Update `Planova.Persistence/EntityConfigurations/ProjectConfiguration.cs`

- Add new column configs: `LogoPath` (MaxLength 1000), `DocumentsFolder` (MaxLength 1000), `Latitude`, `Longitude`, `QrCodePath` (MaxLength 1000)
- Add `builder.HasMany(e => e.Documents).WithOne(d => d.Project).HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Cascade);`

### 3d. Run migration

```
cd Planova.UI
dotnet ef migrations add AddProjectDocumentsAndLocation --project ..\Planova.Persistence
dotnet ef database update --project ..\Planova.Persistence
```

### 3e. New `Planova.Persistence/Repositories/ProjectDocumentRepository.cs`

Implements `IProjectDocumentRepository` using `PlanovaDbContext`.

### 3f. New `Planova.Persistence/Services/ProjectDocumentService.cs`

Implements `IProjectDocumentService`. Key logic:

**AddAsync**:
1. Validate extension is in allowed list — throw `ValidationException` if not
2. Resolve destination: `{Environment.GetFolderPath(SpecialFolder.ApplicationData)}/Planova/Projects/{projectId}/Documents/{DocumentType}/`
3. `Directory.CreateDirectory(dest)` (idempotent)
4. Copy file with `File.Copy(sourcePath, destPath, overwrite: false)` — if name collision append `_1`, `_2`
5. Insert `ProjectDocument` record with `RelativePath` relative to the project folder root

**ScanFolderAsync**:
1. `Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)`
2. Filter to allowed extensions only
3. Auto-detect `DocumentType` by: first check parent subfolder name matches type keywords (`boq`,`spec`,`contract`,`draw`); then check filename keywords
4. For each file: call `PathExistsAsync` to skip duplicates; insert new `ProjectDocument` record with absolute path stored as RelativePath (scanned files stay in place — not copied)
5. Update `Project.DocumentsFolder = folderPath`

### 3g. Update `Planova.Persistence/Extensions/ServiceCollectionExtensions.cs`

```csharp
services.AddScoped<IProjectDocumentRepository, ProjectDocumentRepository>();
services.AddScoped<IProjectDocumentService, ProjectDocumentService>();
```

---

## 4. UI — Project Page Enhancements

### 4a. Update `Planova.Application/Services/ProjectService.cs`

In `CreateAsync` and `UpdateAsync`: if `dto.LogoSourcePath` is provided, copy logo to project folder and set `project.LogoPath`. Handle Latitude/Longitude/DocumentsFolder passthrough.

### 4b. New `Planova.UI/Services/QrCodeService.cs`

Add `QRCoder` 1.6.0 NuGet to `Planova.UI.csproj`.

```csharp
public string GenerateLocationQr(int projectId, double latitude, double longitude)
// Generates geo:{lat},{lon} QR PNG
// Saves to {AppData}/Planova/Projects/{projectId}/qr_location.png
// Returns saved path
```

### 4c. New `Planova.UI/Services/MapHtmlService.cs`

```csharp
public string GenerateMapHtml(double latitude, double longitude, string projectName)
// Returns self-contained HTML using Leaflet.js CDN
// Includes a marker at the given coordinates with a popup showing projectName
// Caller writes to temp file and loads into WebBrowser
```

### 4d. Update `Planova.UI/ViewModels/ProjectsWorkspaceViewModel.cs`

Inject `IProjectDocumentService` into constructor.

**New observable properties**:
```csharp
[ObservableProperty] string? _editLogoSourcePath;
[ObservableProperty] string? _editDocumentsFolder;
[ObservableProperty] double? _editLatitude;
[ObservableProperty] double? _editLongitude;
[ObservableProperty] string? _logoPreviewPath;
[ObservableProperty] string? _qrCodePath;
[ObservableProperty] string? _mapHtmlPath;
[ObservableProperty] string _selectedDocumentTypeFilter = "All";
[ObservableProperty] ProjectDocumentDto? _selectedDocument;
```

**New collections**:
```csharp
public ObservableCollection<ProjectDocumentDto> Documents { get; } = new();
public ObservableCollection<string> DocumentTypeFilters { get; } = new()
    { "All", "Boq", "Drawing", "Spec", "Contract", "Other" };
```

**New commands**:
- `BrowseLogoCommand` — `OpenFileDialog` filter `*.png;*.jpg;*.jpeg;*.bmp`, sets `EditLogoSourcePath` + `LogoPreviewPath`
- `BrowseDocumentsFolderCommand` — `FolderBrowserDialog`, sets `EditDocumentsFolder`
- `AddDocumentsCommand` — `OpenFileDialog` multi-select, all allowed extensions; calls `IProjectDocumentService.AddAsync` per file; reloads Documents
- `DeleteDocumentCommand(int id)` — confirmation, calls `IProjectDocumentService.DeleteAsync`; reloads
- `ScanFolderCommand` — calls `IProjectDocumentService.ScanFolderAsync(new ScanFolderDto(projectId, EditDocumentsFolder))`; reloads
- `OpenDocumentCommand(ProjectDocumentDto doc)` — `Process.Start(new ProcessStartInfo(doc.AbsolutePath) { UseShellExecute = true })`
- `SetLocationCommand` — saves lat/lon via `IProjectService.UpdateAsync`; calls `QrCodeService.GenerateLocationQr`; calls `MapHtmlService.GenerateMapHtml`; writes HTML to temp file; sets `MapHtmlPath` and `QrCodePath`

**Update `SelectProjectAsync`**: after loading project, call `LoadDocumentsAsync`; set `MapHtmlPath` if lat/lon exist.

**Update `NewProject` / `EditProject` / `CancelEdit`**: include new fields.

**Update `SaveAsync`**: pass new fields in `CreateProjectDto` / `UpdateProjectDto`.

**Add `partial void OnSelectedDocumentTypeFilterChanged`**: filter `Documents` by type.

### 4e. Redesign `Planova.UI/Views/Projects/ProjectsWorkspaceView.xaml`

**Left column — richer project list**: Replace `ListView/GridView` with `ListBox` + `DataTemplate` card:
```xml
<!-- Card shows: Logo thumbnail (32x32), Project Name (bold), Code badge,
     Status badge (color per status), Client name, date range, doc count chip -->
```

**Right column — TabControl with 3 tabs** (replaces the single ScrollViewer):

**Tab 1 — Info**: All existing edit fields + new fields:
- Logo: `Image` thumbnail (64x64) + "Browse Logo" button
- Documents Folder: `TextBox` + "Browse" button (shown in view mode too)

**Tab 2 — Documents**:
```
[Filter: All | Boq | Drawing | Spec | Contract | Other]        [+ Add Files]  [Scan Folder]
[Scan folder path TextBox ________________________________________________]  [Scan]

ListView:
  FileName | Type | Ext | Size | Date | Notes | [Open] [Delete]

Validation summary: "Missing: BOQ document" (red text if applicable)
```

**Tab 3 — Location**:
```
Latitude: [TextBox]   Longitude: [TextBox]   [Set Location & Generate QR]

[WebBrowser showing map HTML — fills available space]

QR Code: [Image 128x128]   [Copy path button]
```

---

## 5. UI — Studio Integration

### 5a. New `Planova.UI/ViewModels/Shared/DocumentValidationBannerViewModel.cs`

```csharp
public partial class DocumentValidationBannerViewModel : ObservableObject
{
    private readonly IProjectDocumentService _docService;
    private readonly ICurrentProjectService _currentProject;

    [ObservableProperty] bool _hasWarning;
    [ObservableProperty] string _warningMessage = string.Empty;

    public async Task CheckAsync(string[] requiredTypes, CancellationToken ct = default)
    // Loads docs for current project; checks each required type has at least one doc
    // Sets HasWarning and builds WarningMessage listing missing types
}
```

### 5b. New `Planova.UI/Views/Shared/DocumentValidationBannerView.xaml`

```xml
<!-- Yellow Border, visible only when HasWarning=True -->
<!-- Icon (Warning24) + WarningMessage text + "Go to Project Documents" button -->
<!-- Height ~40px, does not consume space when hidden -->
```

### 5c. Per-studio changes

Each studio view gains the banner in its header `StackPanel`. Each studio ViewModel gets a `DocumentValidationBanner` property. `CheckAsync` is called when the studio loads and when `ICurrentProjectService.CurrentProjectChanged` fires.

| Studio ViewModel | Required Types |
|-----------------|----------------|
| `BoqStudioViewModel` | `["Boq"]` |
| `WbsStudioViewModel` | `["Boq", "Spec"]` (warning if neither exists) |
| `ActivityStudioViewModel` | `["Boq", "Spec"]` |
| `ResourceStudioViewModel` | `["Spec", "Boq"]` |
| `CostStudioViewModel` | `["Boq", "Contract"]` |

XAML change pattern for each studio (e.g. `BoqStudioView.xaml`):
```xml
<!-- Replace header StackPanel with: -->
<StackPanel Grid.Row="0" Margin="12,12,12,0">
    <TextBlock Text="BOQ Studio" .../>
    <shared:DocumentValidationBannerView DataContext="{Binding DocumentValidationBanner}"/>
</StackPanel>
```

### 5d. BOQ Import Wizard upgrade

In `BoqImportViewModel.cs` + `BoqImportWizardView.xaml`:
- Add `ProjectBoqDocuments` (`ObservableCollection<ProjectDocumentDto>`)
- Add `SelectedProjectDocument` property
- On load: populate from `IProjectDocumentService.GetByTypeAsync(projectId, "Boq")`
- Add a "Pick from project" section above the "Select File" button:
  ```xml
  <GroupBox Header="Pick from project documents" Visibility="{Binding HasProjectDocuments, ...}">
      <ComboBox ItemsSource="{Binding ProjectBoqDocuments}" DisplayMemberPath="FileName"
                SelectedItem="{Binding SelectedProjectDocument}"/>
      <Button Content="Use Selected" Command="{Binding UseProjectDocumentCommand}"/>
  </GroupBox>
  ```
- `UseProjectDocumentCommand` sets `SelectedFilePath` to the selected document's path and triggers detection logic

---

## 6. New NuGet Package

Add to `Planova.UI.csproj`:
```xml
<PackageReference Include="QRCoder" Version="1.6.0" />
```

No map library needed — `WebBrowser` control + self-generated Leaflet.js HTML is zero-dependency.

---

## 7. Implementation Order

1. `Planova.Domain`: Edit `Project.cs` + add `ProjectDocument.cs`
2. `Planova.Application`: Add `ProjectDocumentDto`, `AddProjectDocumentDto`, `ScanFolderDto`; update `CreateProjectDto`, `UpdateProjectDto`, `ProjectDetailDto`, `ProjectSummaryDto`; add `IProjectDocumentRepository`, `IProjectDocumentService`; update `MappingProfile`
3. `Planova.Persistence`: Add `ProjectDocumentConfiguration`; update `PlanovaDbContext`; update `ProjectConfiguration`; add `ProjectDocumentRepository`, `ProjectDocumentService`; update `ServiceCollectionExtensions`
4. **Run migration**: `AddProjectDocumentsAndLocation`
5. `Planova.Application/Services/ProjectService.cs`: Handle new logo/location fields
6. `Planova.UI`: Add `QrCodeService`, `MapHtmlService` in new `Services/` folder; add `QRCoder` NuGet
7. `Planova.UI`: Update `ProjectsWorkspaceViewModel` — new properties + commands
8. `Planova.UI`: Redesign `ProjectsWorkspaceView.xaml` — cards list + 3-tab detail
9. `Planova.UI`: Add `DocumentValidationBannerView/ViewModel` in `Views/Shared` + `ViewModels/Shared`
10. `Planova.UI`: Update each studio ViewModel to add `DocumentValidationBanner` + subscribe to project changes
11. `Planova.UI`: Add banner XAML to each studio view header
12. `Planova.UI`: Upgrade `BoqImportViewModel` + `BoqImportWizardView` with project-docs picker
13. `App.xaml.cs` / DI: Register `QrCodeService`, `MapHtmlService`, `DocumentValidationBannerViewModel`

---

## 8. Verification Steps

1. **Build** — zero errors
2. **Create project** — new fields (logo, folder, lat/lon) save and reload correctly
3. **Upload PDF** → file copied to `{AppData}/Planova/Projects/{id}/Documents/Other/`; appears in Documents tab
4. **Upload Excel tagged as Boq** → type shows as `Boq`; BOQ Studio banner disappears
5. **Scan folder** → all allowed-extension files registered; unsupported extensions skipped; `DocumentsFolder` saved on project
6. **Location** → enter lat/lon → map renders in WebBrowser → QR code image appears
7. **Logo** → browse + save → thumbnail shows in Info tab
8. **Project list** → cards show name, code, status color, client, dates, doc count
9. **BOQ Studio** (no BOQ doc) → yellow warning banner visible
10. **BOQ Import wizard** → "Pick from project documents" section shows uploaded BOQ Excel
11. **WBS, Activity, Resource, Cost Studio** → respective banners show/hide correctly
12. **Delete document** → file record removed from DB (file stays on disk — soft reference only for scanned files)
