namespace Planova.Cost.Domain.Configuration;

public class CostAiOptions
{
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
}
