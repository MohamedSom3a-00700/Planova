namespace Planova.Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public int? ClientId { get; set; }
    public int? ContractorId { get; set; }
    public int? SubcontractorId { get; set; }
    public int? ConsultantId { get; set; }
    public string? Currency { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string? LogoPath { get; set; }
    public string? CoverImagePath { get; set; }
    public string? DocumentsFolder { get; set; }
    public string? ProjectFolderPath { get; set; }
    public string? ConnectedXerPath { get; set; }
    public string? ConnectedDatabase { get; set; }
    public string? GoogleMapsLink { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? QrCodePath { get; set; }
    public decimal? ProgressPercentage { get; set; }
    public string HealthIndicator { get; set; } = "Green";
    public DateTime? CurrentDataDate { get; set; }
    public decimal? Budget { get; set; }
    public int ActivityCount { get; set; }
    public DateTime? LastImportDate { get; set; }
    public DateTime? LastExportDate { get; set; }
    public decimal? OriginalBudget { get; set; }
    public decimal? CurrentBudget { get; set; }
    public decimal? ActualCost { get; set; }
    public decimal? EarnedValue { get; set; }
    public decimal? Cpi { get; set; }
    public decimal? Spi { get; set; }
    public decimal? ScheduleHealthPct { get; set; }
    public decimal? CostHealthPct { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public int? CriticalActivityCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Client? Client { get; set; }
    public Contractor? Contractor { get; set; }
    public Subcontractor? Subcontractor { get; set; }
    public Consultant? Consultant { get; set; }
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
}
