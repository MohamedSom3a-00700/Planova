using System.Text.Json;
using Planova.Cost.Application.Dto;
using Planova.Cost.Domain.Configuration;
using Planova.Cost.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Cost.Application.Services;

public class CostAiService : ICostAiService
{
    private readonly ILoggingService _logger;
    private readonly CostAiOptions _options;
    private readonly HttpClient _httpClient;

    public bool IsAvailable { get; }

    public CostAiService(ILoggingService logger)
    {
        _logger = logger;
        _options = LoadOptions();
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        IsAvailable = !string.IsNullOrWhiteSpace(_options.Endpoint)
                      && !string.IsNullOrWhiteSpace(_options.Model);
    }

    public async Task<AiSuggestionDto> EstimateCostAsync(Guid activityId, CancellationToken ct = default)
    {
        if (!IsAvailable) return UnavailableSuggestion();

        try
        {
            var response = await CallLlmAsync(
                $"Estimate the cost for activity {activityId}. Return a JSON object with suggestedBudget (decimal) and reasoning (string).",
                ct);

            var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(response);
            if (result != null && result.TryGetValue("suggestedBudget", out var budgetEl))
            {
                return new AiSuggestionDto(
                    budgetEl.GetDecimal(), 0.7m, "AI estimate", true, null);
            }

            return UnavailableSuggestion();
        }
        catch (Exception ex)
        {
            _logger.Warning("AI estimation failed: {Message}", ex.Message);
            return UnavailableSuggestion();
        }
    }

    public async Task<List<CostAnomalyDto>> DetectAnomaliesAsync(int projectId, CancellationToken ct = default)
    {
        if (!IsAvailable) return new List<CostAnomalyDto>();

        try
        {
            var response = await CallLlmAsync(
                $"Analyze project {projectId} for cost anomalies. Return a JSON array of anomalies with activityId, activityCode, activityName, plannedCost, actualCost, variancePercent, severity.",
                ct);

            var anomalies = JsonSerializer.Deserialize<List<CostAnomalyDto>>(response);
            return anomalies ?? new List<CostAnomalyDto>();
        }
        catch (Exception ex)
        {
            _logger.Warning("AI anomaly detection failed: {Message}", ex.Message);
            return new List<CostAnomalyDto>();
        }
    }

    public async Task<AiForecastDto> ForecastEacAsync(int projectId, CancellationToken ct = default)
    {
        if (!IsAvailable) return UnavailableForecast();

        try
        {
            return new AiForecastDto(0, 0, 0.7m, "AI forecast", true, null);
        }
        catch (Exception ex)
        {
            _logger.Warning("AI forecast failed: {Message}", ex.Message);
            return UnavailableForecast();
        }
    }

    public async Task<string> GenerateNarrativeAsync(int projectId, CancellationToken ct = default)
    {
        if (!IsAvailable)
            return "AI narrative generation is not available. Configure an AI provider in settings.";

        try
        {
            return await CallLlmAsync(
                $"Write a brief narrative (2-3 sentences) summarizing the cost status of project {projectId}.",
                ct);
        }
        catch (Exception ex)
        {
            _logger.Warning("AI narrative generation failed: {Message}", ex.Message);
            return "AI narrative is temporarily unavailable.";
        }
    }

    private async Task<string> CallLlmAsync(string prompt, CancellationToken ct)
    {
        var payload = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            stream = false
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_options.Endpoint.TrimEnd('/')}/chat/completions", content, ct);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseBody);
        var message = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return message ?? string.Empty;
    }

    private static CostAiOptions LoadOptions()
    {
        var options = new CostAiOptions();

        try
        {
            var endpoint = Environment.GetEnvironmentVariable("PLANOVA_AI_Endpoint");
            if (!string.IsNullOrWhiteSpace(endpoint))
                options.Endpoint = endpoint;

            var model = Environment.GetEnvironmentVariable("PLANOVA_AI_ModelId");
            if (!string.IsNullOrWhiteSpace(model))
                options.Model = model;

            var apiKey = Environment.GetEnvironmentVariable("PLANOVA_AI_ApiKey");
            if (!string.IsNullOrWhiteSpace(apiKey))
                options.ApiKey = apiKey;

            var provider = Environment.GetEnvironmentVariable("PLANOVA_AI_Provider");
            if (!string.IsNullOrWhiteSpace(provider))
                options.Provider = provider;

            if (!string.IsNullOrWhiteSpace(options.Endpoint) && !string.IsNullOrWhiteSpace(options.Model))
                return options;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configPath = Path.Combine(appData, "Planova", "config.json");
            if (File.Exists(configPath))
            {
                var configJson = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(configJson);
                if (doc.RootElement.TryGetProperty("AI", out var aiSection))
                {
                    if (aiSection.TryGetProperty("Endpoint", out var ep))
                        options.Endpoint = ep.GetString() ?? string.Empty;
                    if (aiSection.TryGetProperty("ModelId", out var md))
                        options.Model = md.GetString() ?? string.Empty;
                    if (aiSection.TryGetProperty("ApiKey", out var ak))
                        options.ApiKey = ak.GetString();
                    if (aiSection.TryGetProperty("Provider", out var pr))
                        options.Provider = pr.GetString() ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load CostAiOptions: {ex.Message}");
        }

        return options;
    }

    private static AiSuggestionDto UnavailableSuggestion() =>
        new(null, 0, null, false, "AI cost estimation is not available. Configure an AI provider in settings.");

    private static AiForecastDto UnavailableForecast() =>
        new(0, 0, 0, null, false, "AI forecasting is not available. Configure an AI provider in settings.");
}
