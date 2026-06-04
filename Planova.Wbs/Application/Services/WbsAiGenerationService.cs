using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Planova.Wbs.Application.Dto;
using Planova.Wbs.Domain.Interfaces;

namespace Planova.Wbs.Application.Services;

public class WbsAiGenerationService : IWbsAiGenerationService
{
    private readonly ILogger<WbsAiGenerationService> _logger;
    private IChatCompletionService? _chat;

    public WbsAiGenerationService(ILogger<WbsAiGenerationService> logger)
    {
        _logger = logger;
    }

    public async Task<WbsGenerationResult> GenerateAsync(string projectScope, Guid? referenceBoqId, CancellationToken ct)
    {
        try
        {
            var chat = await GetChatServiceAsync(ct);
            if (chat is null)
                return new WbsGenerationResult(new List<SuggestedItem>(), false);

            var prompt = BuildPrompt(projectScope, referenceBoqId);
            var history = new ChatHistory();
            history.AddUserMessage(prompt);

            var result = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
            var text = result.Content ?? string.Empty;

            var items = ParseSuggestedTree(text);
            return new WbsGenerationResult(items, items.Count > 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI generation failed");
            return new WbsGenerationResult(new List<SuggestedItem>(), false);
        }
    }

    public async Task<bool> IsAiAvailableAsync(CancellationToken ct)
    {
        try
        {
            var chat = await GetChatServiceAsync(ct);
            return chat is not null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<IChatCompletionService?> GetChatServiceAsync(CancellationToken ct)
    {
        if (_chat is not null) return _chat;

        var endpoint = GetSetting("AI:Endpoint", "http://localhost:11434/v1");
        var modelId = GetSetting("AI:ModelId", "llama3.2");
        var apiKey = GetSetting("AI:ApiKey", "");

        try
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(endpoint)
            };

            var builder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0010
            builder.AddOpenAIChatCompletion(modelId, apiKey, httpClient: httpClient);
#pragma warning restore SKEXP0010

            var kernel = builder.Build();
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var ping = new ChatHistory();
            ping.AddUserMessage("respond with exactly: OK");
            var response = await chat.GetChatMessageContentAsync(ping, cancellationToken: ct);
            var content = response.Content?.Trim() ?? string.Empty;

            if (content.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                _chat = chat;
                return _chat;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI endpoint not available at {Endpoint}", endpoint);
            return null;
        }
    }

    private static string BuildPrompt(string projectScope, Guid? referenceBoqId)
    {
        var boqHint = referenceBoqId.HasValue
            ? $"\nReference BOQ ID: {referenceBoqId.Value} (use this to inform the WBS structure)"
            : string.Empty;

        return $"""
You are a WBS (Work Breakdown Structure) generator for construction and engineering projects.
Given the following project scope description, generate a WBS in JSON format.

Project Scope:
{projectScope}
{boqHint}

Return ONLY a valid JSON array of items with no additional text. Each item has:
- "Name": string
- "Description": string or null
- "Level": integer (0 for top-level)
- "SortOrder": integer (position among siblings)
- "WbsLevel": string ("Summary", "ControlAccount", "WorkPackage", "PlanningPackage")
- "Weight": decimal or null
- "Children": array of nested items

Generate 3-5 levels deep with 3-7 items at each level.
""";
    }

    private static List<SuggestedItem> ParseSuggestedTree(string text)
    {
        var json = ExtractJson(text);
        if (string.IsNullOrWhiteSpace(json))
            return new List<SuggestedItem>();

        try
        {
            var items = JsonSerializer.Deserialize<List<SuggestedItemJson>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items is null || items.Count == 0)
                return new List<SuggestedItem>();

            var result = new List<SuggestedItem>();
            foreach (var item in items)
                result.Add(MapFromJson(item, null));

            return result;
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"Failed to parse AI response: {ex.Message}");
            return new List<SuggestedItem>();
        }
    }

    private static SuggestedItem MapFromJson(SuggestedItemJson json, Guid? parentId)
    {
        var id = Guid.NewGuid();
        var children = json.Children?.Select(c => MapFromJson(c, id)).ToList()
                       ?? new List<SuggestedItem>();

        return new SuggestedItem(
            id,
            parentId,
            json.Name ?? "Unnamed",
            json.Description,
            json.Level,
            json.SortOrder,
            json.WbsLevel ?? "WorkPackage",
            json.Weight,
            children
        );
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        if (start >= 0 && end > start)
            return text[start..(end + 1)];

        start = text.IndexOf('{');
        end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
            return text[start..(end + 1)];

        return text.Trim();
    }

    private static string GetSetting(string key, string defaultValue)
    {
        try
        {
            var envKey = $"PLANOVA_{key.Replace(":", "_")}";
            var value = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(value)) return value;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configPath = Path.Combine(appData, "Planova", "config.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                var parts = key.Split(':');
                JsonElement element = doc.RootElement;
                foreach (var part in parts)
                {
                    if (!element.TryGetProperty(part, out element))
                        return defaultValue;
                }
                return element.GetString() ?? defaultValue;
            }
        }
        catch
        {
        }

        return defaultValue;
    }
}

internal class SuggestedItemJson
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
    public string? WbsLevel { get; set; }
    public decimal? Weight { get; set; }
    public List<SuggestedItemJson>? Children { get; set; }
}
