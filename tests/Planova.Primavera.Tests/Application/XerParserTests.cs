using FluentAssertions;
using Planova.Primavera.Application.Parsers;

namespace Planova.Primavera.Tests.Application;

public class XerParserTests
{
    private readonly XerParser _parser = new();

    [Fact]
    public async Task ParseAsync_WithValidXer_ReturnsParseResult()
    {
        var xerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Fixtures", "small.xer");

        if (!File.Exists(xerPath))
            return;

        var result = await _parser.ParseAsync(xerPath);

        result.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_WithNonexistentFile_ShouldNotThrow()
    {
        var act = () => _parser.ParseAsync("nonexistent.xer");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Parser_ShouldBeInitializable()
    {
        var parser = new XerParser();
        parser.Should().NotBeNull();
    }
}
