namespace Planova.Primavera.Application.Dto;

public enum DcmaStatus
{
    Pass,
    Warning,
    Fail
}

public class DcmaAssessmentPointDto
{
    public int PointNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DcmaStatus Status { get; set; }
    public int IssueCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class DcmaAssessmentResultDto
{
    public double OverallScore { get; set; }
    public List<DcmaAssessmentPointDto> Points { get; set; } = new();
}
