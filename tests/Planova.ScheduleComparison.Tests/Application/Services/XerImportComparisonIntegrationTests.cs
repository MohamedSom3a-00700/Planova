using System.Text.Json;
using FluentAssertions;
using Planova.Primavera.Application.Parsers;
using Planova.Primavera.Application.Services;
using Planova.Primavera.Domain.Entities;
using Planova.ScheduleComparison.Application.Comparers;
using Planova.ScheduleComparison.Application.Models;

namespace Planova.ScheduleComparison.Tests.Application.Services;

public class XerImportComparisonIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly string TestDataDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "..", "..", "tests");

    [Fact]
    public async Task Compare_TwoXerFiles_DetectsActivityDateDifferences()
    {
        var xer1 = Path.Combine(TestDataDir, "Silver Sand  - Land Scape - Update 24 - May - 2026.xer");
        var xer2 = Path.Combine(TestDataDir, "Silver Sand  - Land Scape - Update 07- jun- 2026.xer");

        if (!File.Exists(xer1) || !File.Exists(xer2))
            return;

        var parser = new XerParser();

        var result1 = await parser.ParseAsync(xer1);
        var result2 = await parser.ParseAsync(xer2);

        result1.Errors.Should().BeEmpty();
        result2.Errors.Should().BeEmpty();

        var stored1 = ToStoredData(result1);
        var stored2 = ToStoredData(result2);

        stored1.Activities.Count.Should().Be(2392);
        stored2.Activities.Count.Should().Be(2392);
        stored1.Relationships.Count.Should().Be(7935);
        stored2.Relationships.Count.Should().Be(7935);
        stored1.ResourceAssignments.Count.Should().Be(4820);
        stored2.ResourceAssignments.Count.Should().Be(4820);

        var data1 = ResolveScheduleData(stored1);
        var data2 = ResolveScheduleData(stored2);

        data1.Activities.Count.Should().Be(2392);
        data2.Activities.Count.Should().Be(2392);

        Console.WriteLine("=== SAMPLE ACTIVITIES (File1) ===");
        foreach (var a in data1.Activities.Where(a => a.ActivityId.StartsWith("CP09-TR1")).Take(3))
        {
            Console.WriteLine($"  {a.ActivityId}: Name={a.Name} Start={a.Start?.ToString("O")} Finish={a.Finish?.ToString("O")} Dur={a.Duration} Float={a.TotalFloat}");
        }
        Console.WriteLine("=== SAMPLE ACTIVITIES (File2) ===");
        foreach (var a in data2.Activities.Where(a => a.ActivityId.StartsWith("CP09-TR1")).Take(3))
        {
            Console.WriteLine($"  {a.ActivityId}: Name={a.Name} Start={a.Start?.ToString("O")} Finish={a.Finish?.ToString("O")} Dur={a.Duration} Float={a.TotalFloat}");
        }
        Console.WriteLine($"Sample from file1 TaskCode: {stored1.Activities.First().TaskCode}");
        Console.WriteLine($"Sample from file1 TaskId: {stored1.Activities.First().TaskId}");
        Console.WriteLine($"Sample from file1 StartDate: {stored1.Activities.First().StartDate?.ToString("O")}");
        Console.WriteLine($"Sample from file1 TotalFloat: {stored1.Activities.First().TotalFloat}");

        var activityDiffs = new ActivityComparer().Compare(data1, data2);
        var logicDiffs = new LogicComparer().Compare(data1, data2);
        var resourceDiffs = new ResourceComparer().Compare(data1, data2);

        var modified = activityDiffs.Count(d => d.ChangeType == "Modified");
        var added = activityDiffs.Count(d => d.ChangeType == "Added");
        var removed = activityDiffs.Count(d => d.ChangeType == "Removed");

        Console.WriteLine($"=== COMPARISON RESULTS ===");
        Console.WriteLine($"Activity diffs: total={activityDiffs.Count} modified={modified} added={added} removed={removed}");
        Console.WriteLine($"Logic diffs: {logicDiffs.Count}");
        Console.WriteLine($"Resource diffs: {resourceDiffs.Count}");

        if (activityDiffs.Count > 0)
        {
            var sampleDiffs = activityDiffs.Where(d => d.ChangeType == "Modified").Take(5).ToList();
            foreach (var d in sampleDiffs)
            {
                Console.WriteLine($"  {d.MatchKey}: {d.FieldName} = '{d.OldValue}' -> '{d.NewValue}'");
            }
        }

        modified.Should().BeGreaterThan(0, "979 activities have date/float differences between the two files");
        added.Should().Be(0, "all 2392 task_codes match between files");
        removed.Should().Be(0, "all 2392 task_codes match between files");
    }

    private static XerStoredData ToStoredData(XerParserResult result)
    {
        return new XerStoredData
        {
            Activities = result.Activities.Select(a => new XerStoredActivity
            {
                TaskId = a.TaskId,
                TaskCode = a.ActivityCode,
                WbsId = a.WbsId,
                Name = a.Name,
                Status = a.Status,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Duration = a.Duration,
                OriginalDuration = a.OriginalDuration,
                RemainingDuration = a.RemainingDuration,
                PercentComplete = a.PercentComplete,
                ActualStartDate = a.ActualStartDate,
                ActualEndDate = a.ActualEndDate,
                EarlyStartDate = a.EarlyStartDate,
                EarlyEndDate = a.EarlyEndDate,
                LateStartDate = a.LateStartDate,
                LateEndDate = a.LateEndDate,
                TotalFloat = a.TotalFloat,
                FreeFloat = a.FreeFloat,
                CalendarId = a.CalendarId
            }).ToList(),
            Relationships = result.Relationships.Select(r => new XerStoredRelationship
            {
                PredTaskId = r.PredTaskId,
                SuccTaskId = r.SuccTaskId,
                Type = r.Type,
                LagDuration = r.LagDuration
            }).ToList(),
            ResourceAssignments = result.ResourceAssignments.Select(ra => new XerStoredResourceAssignment
            {
                TaskId = ra.TaskId,
                ResourceId = ra.ResourceId,
                Units = ra.Units,
                CostPerUnit = ra.CostPerUnit
            }).ToList(),
            ProjectId = result.Project?.ProjectId,
            ProjectName = result.Project?.Name,
            LastRecalcDate = result.Project?.LastRecalcDate,
            PlanStartDate = result.Project?.PlanStartDate,
            PlanEndDate = result.Project?.PlanEndDate,
            SchedEndDate = result.Project?.SchedEndDate,
            AddDate = result.Project?.AddDate,
            LastTasksumDate = result.Project?.LastTasksumDate,
            LastScheduleDate = result.Project?.LastScheduleDate
        };
    }

    private static ScheduleData ResolveScheduleData(XerStoredData storedData)
    {
        var data = new ScheduleData();

        var taskIdToCode = storedData.Activities?
            .Where(a => !string.IsNullOrEmpty(a.TaskCode))
            .ToDictionary(a => a.TaskId, a => a.TaskCode!, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (storedData.Activities != null)
        {
            foreach (var a in storedData.Activities)
            {
                var stableId = taskIdToCode.GetValueOrDefault(a.TaskId, a.TaskId);
                data.Activities.Add(new ScheduleActivity
                {
                    ActivityId = stableId,
                    ActivityCode = a.TaskCode,
                    WbsCode = a.WbsId,
                    Name = a.Name,
                    Status = a.Status,
                    Start = a.StartDate,
                    Finish = a.EndDate,
                    Duration = a.Duration,
                    OriginalDuration = a.OriginalDuration,
                    RemainingDuration = a.RemainingDuration,
                    PercentComplete = a.PercentComplete,
                    ActualStart = a.ActualStartDate,
                    ActualFinish = a.ActualEndDate,
                    EarlyStart = a.EarlyStartDate,
                    EarlyFinish = a.EarlyEndDate,
                    LateStart = a.LateStartDate,
                    LateFinish = a.LateEndDate,
                    TotalFloat = a.TotalFloat,
                    FreeFloat = a.FreeFloat,
                    CalendarId = a.CalendarId
                });
            }
        }

        if (storedData.Relationships != null)
        {
            foreach (var r in storedData.Relationships)
            {
                data.Relationships.Add(new ScheduleRelationship
                {
                    PredecessorActivityId = taskIdToCode.GetValueOrDefault(r.PredTaskId, r.PredTaskId),
                    SuccessorActivityId = taskIdToCode.GetValueOrDefault(r.SuccTaskId, r.SuccTaskId),
                    RelationshipType = r.Type,
                    Lag = r.LagDuration
                });
            }
        }

        if (storedData.ResourceAssignments != null)
        {
            foreach (var ra in storedData.ResourceAssignments)
            {
                var stableActivityId = taskIdToCode.GetValueOrDefault(ra.TaskId, ra.TaskId);
                data.ResourceAssignments.Add(new ScheduleResourceAssignment
                {
                    ActivityProvenanceId = ra.TaskId,
                    ActivityMatchKey = stableActivityId,
                    ResourceId = ra.ResourceId,
                    Units = ra.Units,
                    Cost = ra.CostPerUnit
                });
            }
        }

        return data;
    }
}
