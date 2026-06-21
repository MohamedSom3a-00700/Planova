# Cross-Module Interface Contracts: Overall Enhancement Plan

## Purpose

Define the contract interfaces that cross module boundaries between `Planova.Boq`, `Planova.Wbs`, `Planova.Persistence`, `Planova.UI`, and `Planova.Excel`. These contracts follow Clean Architecture — interfaces are defined in the Domain layer of each module, implementations in Application/Services, and DI registration in Extensions.

## 1. BOQ Import Contracts

### IBoqImportService (Planova.Boq.Domain.Interfaces)

```csharp
public interface IBoqImportService
{
    /// <summary>Scans an Excel file and detects BOQ worksheets.</summary>
    Task<IReadOnlyList<WorksheetInfo>> ScanAsync(string filePath, CancellationToken ct);

    /// <summary>Performs a dry-run preview of the import.</summary>
    Task<BoqImportPreview> PreviewAsync(
        string filePath,
        Guid projectId,
        ImportMode mode,
        IReadOnlyList<WorksheetSelection> selections,
        CancellationToken ct);

    /// <summary>Executes the import and persists BOQ data.</summary>
    Task<BoqImportResult> ImportAsync(
        string filePath,
        Guid projectId,
        ImportMode mode,
        IReadOnlyList<WorksheetSelection> selections,
        int userId,
        CancellationToken ct);
}

public record WorksheetInfo(string Name, int Index, int RowCount, bool IsBoqSheet, decimal MatchConfidence);

public record WorksheetSelection(int WorksheetIndex, ImportAction Action, ColumnMapping? OverrideMapping);

public enum ImportAction { Skip, Import, Merge }

public record BoqImportPreview(
    int WorksheetsDetected,
    int WorksheetsToImport,
    int EstimatedRows,
    IReadOnlyList<WorksheetPreview> Previews);

public record BoqImportResult(
    Guid SessionId,
    int SheetsImported,
    int SectionsCreated,
    int ItemsImported,
    decimal TotalAmount);
```

### IBoqColumnMappingService (Planova.Boq.Domain.Interfaces)

```csharp
public interface IBoqColumnMappingService
{
    /// <summary>Detects column headers and maps them to known BOQ fields using heuristics.</summary>
    Task<ColumnMapping> DetectMappingAsync(string filePath, int worksheetIndex, CancellationToken ct);

    /// <summary>Validates a manual/overridden column mapping.</summary>
    Task<MappingValidation> ValidateMappingAsync(ColumnMapping mapping, CancellationToken ct);
}

public record ColumnMapping(
    int? CodeColumn,
    int? DescriptionColumn,
    int? UnitColumn,
    int? QuantityColumn,
    int? RateColumn,
    int? AmountColumn,
    decimal Confidence);

public record MappingValidation(bool IsValid, IReadOnlyList<string> Warnings);
```

## 2. WBS Mapping Contracts

### IWbsMappingService (Planova.Wbs.Domain.Interfaces)

```csharp
public interface IWbsMappingService
{
    /// <summary>Generates a preview WBS tree from a BOQ using the specified mapping method.</summary>
    Task<WbsMappingPreview> PreviewMappingAsync(
        Guid boqId, WbsMappingMethod method, CancellationToken ct);

    /// <summary>Creates a WBS from a BOQ mapping.</summary>
    Task<WbsMappingResult> CreateWbsFromMappingAsync(
        Guid boqId, WbsMappingMethod method, string wbsName, int userId, CancellationToken ct);
}

public enum WbsMappingMethod { BySection, ByCsi, ByCostCode, ByTrade, ByDiscipline }

public record WbsMappingPreview(
    IReadOnlyList<WbsMappingNode> Nodes,
    int TotalItems,
    int Depth);

public record WbsMappingNode(string Code, string Name, decimal Weight, IReadOnlyList<WbsMappingNode> Children);

public record WbsMappingResult(Guid WbsId, int ItemsCreated);
```

## 3. WBS AI Generation Contracts

### IWbsAiGenerationService (Planova.Wbs.Domain.Interfaces)

```csharp
public interface IWbsAiGenerationService
{
    /// <summary>Generates a WBS hierarchy from BOQ data and/or project documents using AI.</summary>
    Task<WbsAiPreview> GeneratePreviewAsync(
        WbsAiRequest request, CancellationToken ct);

    /// <summary>Accepts an AI-generated preview and creates the WBS.</summary>
    Task<WbsAiResult> AcceptGenerationAsync(
        WbsAiRequest request, Guid previewId, string wbsName, int userId, CancellationToken ct);
}

public enum WbsAiSource { BoqOnly, DocumentsOnly, BoqAndDocuments }

public record WbsAiRequest(
    Guid ProjectId,
    WbsAiSource Source,
    Guid? BoqId,
    IReadOnlyList<Guid>? DocumentIds);

public record WbsAiPreview(
    Guid PreviewId,
    IReadOnlyList<WbsMappingNode> Nodes,
    int Confidence);

public record WbsAiResult(Guid WbsId, int ItemsCreated);
```

## 4. WBS Edit Lock Contracts

### IWbsLockService (Planova.Wbs.Domain.Interfaces)

```csharp
public interface IWbsLockService
{
    /// <summary>Attempts to acquire an edit lock on a WBS.</summary>
    Task<LockResult> AcquireLockAsync(Guid wbsId, int userId, CancellationToken ct);

    /// <summary>Releases an edit lock.</summary>
    Task ReleaseLockAsync(Guid wbsId, int userId, CancellationToken ct);

    /// <summary>Returns current lock info (null if unlocked).</summary>
    Task<LockInfo?> GetLockInfoAsync(Guid wbsId, CancellationToken ct);

    /// <summary>Extends the lock expiration (called on user activity).</summary>
    Task RefreshLockAsync(Guid wbsId, int userId, CancellationToken ct);
}

public record LockResult(bool Acquired, LockInfo? CurrentLock, string? DeniedReason);

public record LockInfo(Guid WbsId, string LockedByUserName, DateTime LockedAt, DateTime ExpiresAt);
```

## 5. WBS Code Generation Contract

### IWbsCodeGenerationService (Planova.Wbs.Domain.Interfaces)

```csharp
public interface IWbsCodeGenerationService
{
    /// <summary>Regenerates codes for all items in a WBS based on tree structure.</summary>
    Task RegenerateCodesAsync(Guid wbsId, CancellationToken ct);

    /// <summary>Validates that current codes are consistent with tree structure.</summary>
    Task<bool> ValidateCodesAsync(Guid wbsId, CancellationToken ct);
}
```

## 6. Project Folder Contract

### IProjectFolderService (Planova.Application.Interfaces)

```csharp
public interface IProjectFolderService
{
    /// <summary>Creates the standard subfolder structure under the given root path.</summary>
    Task<IReadOnlyList<string>> CreateFolderStructureAsync(
        string rootPath, CancellationToken ct);

    /// <summary>Returns the list of standard subfolder names.</summary>
    IReadOnlyList<string> GetStandardFolderNames();
}
```

## 7. Party Contracts

### IPartyService (Planova.Application.Interfaces)

```csharp
public interface IPartyService
{
    Task<IReadOnlyList<PartyDto>> GetPartiesByProjectAsync(int projectId, CancellationToken ct);
    Task<PartyDto> CreatePartyAsync(CreatePartyRequest request, CancellationToken ct);
    Task<PartyDto> UpdatePartyAsync(Guid partyId, UpdatePartyRequest request, CancellationToken ct);
    Task DeletePartyAsync(Guid partyId, CancellationToken ct);
    Task LinkPartyToProjectAsync(Guid partyId, int projectId, string role, CancellationToken ct);
    Task UnlinkPartyFromProjectAsync(Guid partyId, int projectId, CancellationToken ct);
}
```
