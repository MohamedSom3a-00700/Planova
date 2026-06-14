using FluentAssertions;
using Planova.Primavera.Application.Writers;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;

namespace Planova.Primavera.Tests.Application;

public class XerWriterTests
{
    private readonly XerWriter _writer = new();

    [Fact]
    public async Task WriteAsync_ShouldCreateValidXerFile()
    {
        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            var calendars = new List<PrimaveraCalendar>
            {
                new() { CalendarId = "1", Name = "Standard 5-day", IsBaseCalendar = true, SourceType = PrimaveraSourceType.Imported }
            };

            await _writer.WriteAsync(outputPath, calendars, null,
                new List<PrimaveraActivity>(),
                new List<PrimaveraRelationship>(),
                new List<PrimaveraResourceAssignment>(),
                new List<PrimaveraCode>(),
                new List<PrimaveraBaseline>(),
                new List<PrimaveraUdf>(),
                new List<XerRawTable>());

            File.Exists(outputPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("ERMHDR");
            content.Should().Contain("%T|CALENDAR");
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task WriteAsync_WithActivities_IncludesTaskTable()
    {
        var outputPath = Path.GetTempFileName() + ".xer";

        try
        {
            var activities = new List<PrimaveraActivity>
            {
                new() { TaskId = "T1", Name = "Task 1", Duration = 5.0, Status = "Status_NotStart", SourceType = PrimaveraSourceType.Imported },
                new() { TaskId = "T2", Name = "Task 2", Duration = 3.0, Status = "Status_InProgress", SourceType = PrimaveraSourceType.Imported }
            };

            await _writer.WriteAsync(outputPath, new List<PrimaveraCalendar>(), null,
                activities, new List<PrimaveraRelationship>(),
                new List<PrimaveraResourceAssignment>(),
                new List<PrimaveraCode>(),
                new List<PrimaveraBaseline>(),
                new List<PrimaveraUdf>(),
                new List<XerRawTable>());

            var content = await File.ReadAllTextAsync(outputPath);
            content.Should().Contain("%T|TASK");
            content.Should().Contain("T1");
            content.Should().Contain("T2");
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }
}
