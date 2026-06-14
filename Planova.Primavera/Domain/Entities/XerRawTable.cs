namespace Planova.Primavera.Domain.Entities;

public class XerRawTable
{
    public Guid Id { get; set; }
    public int ProjectId { get; set; }
    public Guid ImportSessionId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string? ColumnHeaders { get; set; }
    public string? Rows { get; set; }
    public int SortOrder { get; set; }
}
