using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;

namespace Planova.ScheduleComparison.Tests.Application.Comparers;

public class ComparerPerformanceBenchmarks
{
    private static ScheduleData LoadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ScheduleData>(json) ?? new ScheduleData();
    }

    [Fact]
    public void ActivityComparer_ProcessesModerateSchedule_UnderThreshold()
    {
        var source = LoadFixture("moderate-schedule.json");
        var target = LoadFixture("small-schedule.json");
        var comparer = new ActivityComparer();

        var sw = Stopwatch.StartNew();
        var result = comparer.Compare(source, target);
        sw.Stop();

        result.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void LogicComparer_ProcessesModerateSchedule_UnderThreshold()
    {
        var source = LoadFixture("moderate-schedule.json");
        var target = LoadFixture("small-schedule.json");
        var comparer = new LogicComparer();

        var sw = Stopwatch.StartNew();
        var result = comparer.Compare(source, target);
        sw.Stop();

        result.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void ResourceComparer_ProcessesModerateSchedule_UnderThreshold()
    {
        var source = LoadFixture("moderate-schedule.json");
        var target = LoadFixture("small-schedule.json");
        var comparer = new ResourceComparer();

        var sw = Stopwatch.StartNew();
        var result = comparer.Compare(source, target);
        sw.Stop();

        result.Should().NotBeNull();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void CriticalPathComparer_ProcessesModerateSchedule_UnderThreshold()
    {
        var source = LoadFixture("moderate-schedule.json");
        var target = LoadFixture("small-schedule.json");
        var comparer = new CriticalPathComparer();

        var sw = Stopwatch.StartNew();
        var result = comparer.Compare(source, target);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void FloatComparer_ProcessesModerateSchedule_UnderThreshold()
    {
        var source = LoadFixture("moderate-schedule.json");
        var target = LoadFixture("small-schedule.json");
        var comparer = new FloatComparer();

        var sw = Stopwatch.StartNew();
        var result = comparer.Compare(source, target);
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }
}
