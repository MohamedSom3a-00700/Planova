using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Planova.Wbs.Application.Services;

namespace Planova.Wbs.Tests.Services;

public class WbsAiGenerationServiceTests
{
    private readonly Mock<ILogger<WbsAiGenerationService>> _logger = new();
    private readonly WbsAiGenerationService _sut;

    public WbsAiGenerationServiceTests()
    {
        _sut = new WbsAiGenerationService(_logger.Object);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsResult()
    {
        var result = await _sut.GenerateAsync("Build a bridge", null, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task IsAiAvailableAsync_ReturnsFalseWhenNoEndpoint()
    {
        var result = await _sut.IsAiAvailableAsync(CancellationToken.None);

        result.Should().BeFalse();
    }
}
