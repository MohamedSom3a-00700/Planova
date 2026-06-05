using Planova.Excel.Validation;
using Xunit;

namespace Planova.Excel.Tests.Validation;

public class ValidationServiceTests
{
    private readonly ValidationService _sut = new();

    [Fact]
    public async Task ValidateAsync_WithValidProjectRecords_ReturnsValid()
    {
        var records = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "P001", ["Name"] = "Project Alpha" }
        };
        var mappings = new Dictionary<string, string>
        {
            ["Code"] = "Code",
            ["Name"] = "Name"
        };

        var result = await _sut.ValidateAsync("Project", records, mappings, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(0, result.TotalErrors);
    }

    [Fact]
    public async Task ValidateAsync_WithMissingRequiredFields_ReturnsErrors()
    {
        var records = new List<Dictionary<string, object>>
        {
            new() { ["Code"] = "P001" }
        };
        var mappings = new Dictionary<string, string>
        {
            ["Code"] = "Code",
            ["Name"] = "Name"
        };

        var result = await _sut.ValidateAsync("Project", records, mappings, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.True(result.TotalErrors > 0);
        Assert.Contains(result.Errors, e => e.ErrorType == "Required");
    }

    [Fact]
    public async Task ValidateAsync_WithUnknownEntityType_ReturnsError()
    {
        var records = new List<Dictionary<string, object>>();
        var mappings = new Dictionary<string, string>();

        var result = await _sut.ValidateAsync("Unknown", records, mappings, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal(1, result.TotalErrors);
        Assert.Equal("InvalidEntity", result.Errors[0].ErrorType);
    }

    [Fact]
    public async Task ValidateAsync_Cancellation_Throws()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _sut.ValidateAsync("Project", new List<Dictionary<string, object>>(), new Dictionary<string, string>(), cts.Token));
    }
}
