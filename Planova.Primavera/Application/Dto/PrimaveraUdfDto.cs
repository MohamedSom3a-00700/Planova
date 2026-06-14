namespace Planova.Primavera.Application.Dto;

public class PrimaveraUdfDto
{
    public Guid Id { get; set; }
    public string UdfTypeId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
}
