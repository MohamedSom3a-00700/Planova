using FluentAssertions;
using Planova.Primavera.Application.Parsers;
using Planova.Primavera.Application.Writers;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Tests.Integration;

public class ImportExportRoundTripTests
{
    private readonly XerParser _parser = new();
    private readonly XerWriter _writer = new();

    [Fact]
    public async Task ParseThenWrite_RoundTrip_PreservesRawTables()
    {
        var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Fixtures", "small.xer");

        if (!File.Exists(sourcePath))
            return;

        var parseResult = await _parser.ParseAsync(sourcePath);
        parseResult.Should().NotBeNull();

        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            await _writer.WriteAsync(outputPath,
                parseResult.Calendars,
                null,
                parseResult.Activities,
                parseResult.Relationships,
                parseResult.ResourceAssignments,
                parseResult.Codes,
                parseResult.Baselines,
                parseResult.Udfs,
                parseResult.RawTables);

            File.Exists(outputPath).Should().BeTrue();
            var fileSize = new FileInfo(outputPath).Length;
            fileSize.Should().BeGreaterThan(0);

            var reParsed = await _parser.ParseAsync(outputPath);
            reParsed.Should().NotBeNull();
            reParsed.Errors.Should().BeEmpty();

            if (parseResult.RawTables.Count > 0)
            {
                reParsed.RawTables.Count.Should().Be(parseResult.RawTables.Count,
                    "raw tables should be preserved across round-trip");
            }
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ParseThenWrite_ProducesValidXerHeader()
    {
        var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "Fixtures", "small.xer");

        if (!File.Exists(sourcePath))
            return;

        var parseResult = await _parser.ParseAsync(sourcePath);
        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            await _writer.WriteAsync(outputPath,
                parseResult.Calendars, null,
                parseResult.Activities, parseResult.Relationships,
                parseResult.ResourceAssignments, parseResult.Codes,
                parseResult.Baselines, parseResult.Udfs, parseResult.RawTables);

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().StartWith("ERMHDR");
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public void Enums_AreConsistent()
    {
        var imported = PrimaveraSourceType.Imported;
        var manualEdit = PrimaveraSourceType.ManualEdit;
        var repair = PrimaveraSourceType.Repair;
        var export = PrimaveraSourceType.Export;

        ((int)imported).Should().Be(0);
        ((int)manualEdit).Should().Be(1);
        ((int)repair).Should().Be(2);
        ((int)export).Should().Be(3);
    }
}
