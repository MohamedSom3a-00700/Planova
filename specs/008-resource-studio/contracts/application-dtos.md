# Application DTOs — Resource Studio

## Resource DTOs

### ResourceDto

```csharp
public record ResourceDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public ResourceScope Scope { get; init; }
    public int? ProjectId { get; init; }
    public ResourceStatus Status { get; init; }
    public decimal DefaultRate { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? MaxQuantity { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public string? Trade { get; init; }
    public string? SkillLevel { get; init; }
    public string? EquipmentType { get; init; }
    public string? Capacity { get; init; }
    public decimal? OperatingCost { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? WastagePercent { get; init; }
    public string? Company { get; init; }
    public decimal? ContractValue { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
    public decimal? EffectiveRate { get; init; } // resolved at query time
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

### CreateResourceRequest

```csharp
public record CreateResourceRequest
{
    public string Name { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public ResourceScope Scope { get; init; }
    public int? ProjectId { get; init; }
    public decimal DefaultRate { get; init; }
    public string UnitOfMeasure { get; init; } = "hr";
    public decimal? MaxQuantity { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    // Type-specific fields
    public string? Trade { get; init; }
    public string? SkillLevel { get; init; }
    public string? EquipmentType { get; init; }
    public string? Capacity { get; init; }
    public decimal? OperatingCost { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? WastagePercent { get; init; }
    public string? Company { get; init; }
    public decimal? ContractValue { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
}
```

### UpdateResourceRequest

```csharp
public record UpdateResourceRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal DefaultRate { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal? MaxQuantity { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    // Type-specific mutable fields
    public string? Trade { get; init; }
    public string? SkillLevel { get; init; }
    public string? EquipmentType { get; init; }
    public string? Capacity { get; init; }
    public decimal? OperatingCost { get; init; }
    public decimal? UnitPrice { get; init; }
    public decimal? WastagePercent { get; init; }
    public string? Company { get; init; }
    public decimal? ContractValue { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
}
```

### ResourceFilter

```csharp
public record ResourceFilter
{
    public string? SearchQuery { get; init; }
    public ResourceType? Type { get; init; }
    public ResourceScope? Scope { get; init; }
    public int? ProjectId { get; init; }
    public ResourceStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
```

### ResourceDuplicateCheckResult

```csharp
public record ResourceDuplicateCheckResult
{
    public bool HasDuplicate { get; init; }
    public ResourceDto? MatchingResource { get; init; }
    public string? WarningMessage { get; init; }
}
```

---

## ResourceRate DTOs

### ResourceRateDto

```csharp
public record ResourceRateDto
{
    public Guid Id { get; init; }
    public Guid ResourceId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

### CreateRateRequest

```csharp
public record CreateRateRequest
{
    public Guid ResourceId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = "USD";
    public string UnitOfMeasure { get; init; } = "hr";
    public bool IsDefault { get; init; }
    public string? Notes { get; init; }
}
```

---

## Crew DTOs

### CrewDto

```csharp
public record CrewDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ProjectId { get; init; }
    public CrewStatus Status { get; init; }
    public string? Category { get; init; }
    public decimal BlendedRate { get; init; } // computed on read
    public int ResourceCount { get; init; }
    public List<CrewResourceDto> Resources { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

### CrewResourceDto

```csharp
public record CrewResourceDto
{
    public Guid Id { get; init; }
    public Guid CrewId { get; init; }
    public Guid ResourceId { get; init; }
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public decimal Quantity { get; init; }
    public bool IsLead { get; init; }
    public int SortOrder { get; init; }
    public decimal EffectiveRate { get; init; }
    public decimal LineTotal { get; init; } // Quantity × EffectiveRate
}
```

### CreateCrewRequest

```csharp
public record CreateCrewRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? ProjectId { get; init; }
    public string? Category { get; init; }
    public List<CrewResourceInput> Resources { get; init; } = [];
}
```

### CrewResourceInput

```csharp
public record CrewResourceInput
{
    public Guid ResourceId { get; init; }
    public decimal Quantity { get; init; }
    public bool IsLead { get; init; }
}
```

---

## ResourceAssignment DTOs

### ResourceAssignmentDto

```csharp
public record ResourceAssignmentDto
{
    public Guid Id { get; init; }
    public int ProjectId { get; init; }
    public Guid ActivityId { get; init; }
    public string ActivityName { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public Guid? CrewId { get; init; }
    public string? CrewName { get; init; }
    public decimal Quantity { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal TotalCost { get; init; }
    public decimal? DurationDays { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
```

### CreateAssignmentRequest

```csharp
public record CreateAssignmentRequest
{
    public int ProjectId { get; init; }
    public Guid ActivityId { get; init; }
    public Guid ResourceId { get; init; }
    public Guid? CrewId { get; init; }
    public decimal Quantity { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = "USD";
    public string UnitOfMeasure { get; init; } = "hr";
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
}
```

### UpdateAssignmentRequest

```csharp
public record UpdateAssignmentRequest
{
    public Guid Id { get; init; }
    public decimal Quantity { get; init; }
    public decimal Rate { get; init; }
    public string Currency { get; init; } = "USD";
    public string UnitOfMeasure { get; init; } = "hr";
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Notes { get; init; }
}
```

---

## Histogram DTOs

### ResourceHistogramDto

```csharp
public record ResourceHistogramDto
{
    public List<HistogramDayDto> DailyData { get; init; } = [];
    public List<HistogramResourceSummary> ResourceSummaries { get; init; } = [];
    public DateTime ProjectStart { get; init; }
    public DateTime ProjectEnd { get; init; }
    public int TotalDays { get; init; }
    public HistogramFilter AppliedFilter { get; init; }
}
```

### HistogramDayDto

```csharp
public record HistogramDayDto
{
    public DateTime Date { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal? AvailableQuantity { get; init; }
    public bool IsOverallocated { get; init; }
    public ResourceType? ResourceType { get; init; }
    public List<HistogramBreakdownItem> Breakdown { get; init; } = [];
}
```

### HistogramFilter

```csharp
public record HistogramFilter
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public ResourceType? ResourceType { get; init; }
    public Guid? ResourceId { get; init; }
    public HistogramAggregation Aggregation { get; init; } = HistogramAggregation.Sum;
}

public enum HistogramAggregation { Sum, Average, Peak }
```

---

## AI Estimation DTOs

### AiSuggestionDto

```csharp
public record AiSuggestionDto
{
    public string ResourceCode { get; init; } = string.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public ResourceType ResourceType { get; init; }
    public decimal SuggestedQuantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public decimal ConfidenceScore { get; init; } // 0.0 - 1.0
    public string? Reasoning { get; init; }
}
```

### AcceptedSuggestionDto

```csharp
public record AcceptedSuggestionDto
{
    public string ResourceCode { get; init; } = string.Empty;
    public decimal Quantity { get; init; } // may be adjusted by user
}
```

---

## Report DTOs

### ResourceUsageReportDto

```csharp
public record ResourceUsageReportDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<ActivityResourceSection> Activities { get; init; } = [];
    public decimal TotalCost { get; init; }
}

public record ActivityResourceSection
{
    public string ActivityCode { get; init; } = string.Empty;
    public string ActivityName { get; init; } = string.Empty;
    public List<ResourceAssignmentDto> Assignments { get; init; } = [];
    public decimal ActivityTotalCost { get; init; }
}
```

### ResourceCostReportDto

```csharp
public record ResourceCostReportDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public List<CostSummarySection> CostSections { get; init; } = [];
    public decimal GrandTotal { get; init; }
}

public record CostSummarySection
{
    public string SectionName { get; init; } = string.Empty; // "By Type", "By Crew", "By Activity"
    public List<CostLineItem> LineItems { get; init; } = [];
}

public record CostLineItem
{
    public string Label { get; init; } = string.Empty;
    public decimal TotalCost { get; init; }
    public string Currency { get; init; } = "USD";
    public int AssignmentCount { get; init; }
}
```

---

## Import DTOs

### ImportPreviewDto

```csharp
public record ImportPreviewDto
{
    public string FileName { get; init; } = string.Empty;
    public int TotalRows { get; init; }
    public int ValidRows { get; init; }
    public int ErrorRows { get; init; }
    public List<ImportRowDto> Rows { get; init; } = [];
    public List<ImportDuplicateDto> Duplicates { get; init; } = [];
}

public record ImportRowDto
{
    public int RowNumber { get; init; }
    public string? Name { get; init; }
    public string? ResourceType { get; init; }
    public string? Code { get; init; }
    public decimal? DefaultRate { get; init; }
    public string? UnitOfMeasure { get; init; }
    public List<string> ValidationErrors { get; init; } = [];
    public bool IsValid { get; init; }
}

public record ImportDuplicateDto
{
    public int RowNumber { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string ExistingResourceCode { get; init; } = string.Empty;
    public string ExistingResourceName { get; init; } = string.Empty;
}
```

### ImportRequest

```csharp
public record ImportRequest
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public int? ProjectId { get; init; }
    public ImportDuplicateHandling DuplicateHandling { get; init; }
    public List<int>? SelectedRowNumbers { get; init; } // null = import all valid
}

public enum ImportDuplicateHandling { Skip, Overwrite, Rename }
```

### ImportResultDto

```csharp
public record ImportResultDto
{
    public int TotalProcessed { get; init; }
    public int SuccessCount { get; init; }
    public int SkippedCount { get; init; }
    public int ErrorCount { get; init; }
    public List<string> Errors { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}
```
