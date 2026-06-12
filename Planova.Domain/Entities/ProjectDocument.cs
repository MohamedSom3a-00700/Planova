namespace Planova.Domain.Entities;

public class ProjectDocument
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Notes { get; set; }
    public DateTime UploadedAt { get; set; }
    public Project Project { get; set; } = null!;
}
