using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Interfaces;

namespace Planova.Primavera.Application.Services;

public class PrimaveraValidationService : IPrimaveraValidationService
{
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private readonly IPrimaveraValidationRepository _repository;

    public PrimaveraValidationService(
        IPrimaveraWorkspaceService workspaceService,
        IPrimaveraValidationRepository repository)
    {
        _workspaceService = workspaceService;
        _repository = repository;
    }

    public async Task<List<PrimaveraValidationIssueDto>> ValidateAsync(int projectId, CancellationToken ct = default)
    {
        var issues = new List<PrimaveraValidationIssueDto>();
        var activities = await _workspaceService.GetActivitiesAsync(projectId, ct);
        var relationships = await _workspaceService.GetRelationshipsAsync(projectId, ct);
        var calendars = await _workspaceService.GetCalendarsAsync(projectId, ct);

        var activityIds = activities.Select(a => a.TaskId).ToHashSet();

        CheckMissingCalendars(issues, activities, calendars);
        CheckBrokenRelationships(issues, relationships, activityIds, activities);
        CheckZeroDurationActivities(issues, activities);
        CheckCircularLogic(issues, relationships);
        CheckDanglingActivities(issues, activities, relationships);

        await _repository.ClearIssuesAsync(projectId, ct);

        var entities = issues.Select(i => new Primavera.Domain.Entities.PrimaveraValidationIssue
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Severity = Enum.Parse<Primavera.Domain.Enums.PrimaveraValidationSeverity>(i.Severity),
            EntityType = Primavera.Domain.Enums.PrimaveraEntityType.Activity,
            Description = i.Description,
            SuggestedFix = i.SuggestedFix,
            DetectedAt = DateTime.UtcNow
        }).ToList();

        if (entities.Count > 0)
            await _repository.AddIssuesAsync(entities, ct);

        return issues;
    }

    private static void CheckMissingCalendars(List<PrimaveraValidationIssueDto> issues,
        List<PrimaveraActivityDto> activities, List<PrimaveraCalendarDto> calendars)
    {
        var calendarIds = calendars.Select(c => c.CalendarId).ToHashSet();
        foreach (var activity in activities.Where(a => a.CalendarId != null && !calendarIds.Contains(a.CalendarId)))
        {
            issues.Add(new PrimaveraValidationIssueDto
            {
                Severity = "Error",
                EntityType = "Activity",
                Description = $"Activity '{activity.Name}' references missing calendar '{activity.CalendarId}'.",
                SuggestedFix = "Assign a valid calendar to the activity."
            });
        }
    }

    private static void CheckBrokenRelationships(List<PrimaveraValidationIssueDto> issues,
        List<PrimaveraRelationshipDto> relationships, HashSet<string> activityIds,
        List<PrimaveraActivityDto> activities)
    {
        foreach (var rel in relationships)
        {
            if (!activityIds.Contains(rel.PredTaskId))
            {
                issues.Add(new PrimaveraValidationIssueDto
                {
                    Severity = "Error",
                    EntityType = "Relationship",
                    Description = $"Relationship references missing predecessor task '{rel.PredTaskId}'.",
                    SuggestedFix = "Remove or recreate the relationship."
                });
            }
            if (!activityIds.Contains(rel.SuccTaskId))
            {
                issues.Add(new PrimaveraValidationIssueDto
                {
                    Severity = "Error",
                    EntityType = "Relationship",
                    Description = $"Relationship references missing successor task '{rel.SuccTaskId}'.",
                    SuggestedFix = "Remove or recreate the relationship."
                });
            }
        }
    }

    private static void CheckZeroDurationActivities(List<PrimaveraValidationIssueDto> issues,
        List<PrimaveraActivityDto> activities)
    {
        foreach (var activity in activities.Where(a => a.Duration <= 0 && a.Status != "Status_Completed"))
        {
            issues.Add(new PrimaveraValidationIssueDto
            {
                Severity = "Warning",
                EntityType = "Activity",
                Description = $"Activity '{activity.Name}' has zero duration.",
                SuggestedFix = "Set a positive duration for the activity."
            });
        }
    }

    private static void CheckCircularLogic(List<PrimaveraValidationIssueDto> issues,
        List<PrimaveraRelationshipDto> relationships)
    {
        var graph = new Dictionary<string, List<string>>();
        foreach (var rel in relationships)
        {
            if (!graph.ContainsKey(rel.SuccTaskId))
                graph[rel.SuccTaskId] = new();
            graph[rel.SuccTaskId].Add(rel.PredTaskId);
        }

        foreach (var node in graph.Keys)
        {
            var visited = new HashSet<string>();
            var stack = new Stack<string>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current)) continue;

                if (graph.TryGetValue(current, out var predecessors))
                {
                    foreach (var pred in predecessors)
                    {
                        if (pred == node)
                        {
                            issues.Add(new PrimaveraValidationIssueDto
                            {
                                Severity = "Error",
                                EntityType = "Relationship",
                                Description = $"Circular logic detected involving task '{node}'.",
                                SuggestedFix = "Review and break the circular dependency chain."
                            });
                            return;
                        }
                        stack.Push(pred);
                    }
                }
            }
        }
    }

    private static void CheckDanglingActivities(List<PrimaveraValidationIssueDto> issues,
        List<PrimaveraActivityDto> activities, List<PrimaveraRelationshipDto> relationships)
    {
        var hasPred = relationships.Select(r => r.SuccTaskId).ToHashSet();
        var hasSucc = relationships.Select(r => r.PredTaskId).ToHashSet();

        foreach (var activity in activities)
        {
            bool noPred = !hasPred.Contains(activity.TaskId);
            bool noSucc = !hasSucc.Contains(activity.TaskId);

            if (noPred && noSucc && relationships.Count > 0)
            {
                issues.Add(new PrimaveraValidationIssueDto
                {
                    Severity = "Warning",
                    EntityType = "Activity",
                    Description = $"Activity '{activity.Name}' ({activity.TaskId}) is dangling: has no predecessors and no successors.",
                    SuggestedFix = "Add logic ties (predecessors and successors) to integrate this activity into the schedule network."
                });
            }
        }
    }

    public async Task<DcmaAssessmentResultDto> AssessDcma14PointAsync(int projectId, CancellationToken ct = default)
    {
        var activities = await _workspaceService.GetActivitiesAsync(projectId, ct);
        var relationships = await _workspaceService.GetRelationshipsAsync(projectId, ct);
        var calendars = await _workspaceService.GetCalendarsAsync(projectId, ct);
        var assignments = await _workspaceService.GetResourceAssignmentsAsync(projectId, ct);

        var incompleteActivities = activities.Where(a => a.Status != "Status_Completed").ToList();
        var incompleteCount = incompleteActivities.Count;
        var totalActivities = activities.Count;
        var totalRelationships = relationships.Count;

        var activityMap = activities.ToDictionary(a => a.TaskId, a => a);
        var predMap = relationships.GroupBy(r => r.SuccTaskId).ToDictionary(g => g.Key, g => g.ToList());
        var succMap = relationships.GroupBy(r => r.PredTaskId).ToDictionary(g => g.Key, g => g.ToList());

        var points = new List<DcmaAssessmentPointDto>();
        int passed = 0;

        // ---- 1. Missing Logic: ≤ 5% of incomplete activities ----
        var noPred = incompleteActivities.Where(a => !predMap.ContainsKey(a.TaskId) && totalRelationships > 0).ToList();
        var noSucc = incompleteActivities.Where(a => !succMap.ContainsKey(a.TaskId) && totalRelationships > 0).ToList();
        var missingLogic = noPred.Concat(noSucc).DistinctBy(a => a.TaskId).ToList();
        var mlPct = incompleteCount > 0 ? Math.Round(missingLogic.Count * 100.0 / incompleteCount, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 1,
            Name = "Missing Logic",
            Description = "Activities with missing predecessors or successors (≤ 5% of incomplete activities)",
            Status = missingLogic.Count == 0 ? DcmaStatus.Pass : mlPct > 5 ? DcmaStatus.Fail : DcmaStatus.Warning,
            IssueCount = missingLogic.Count,
            TotalCount = incompleteCount,
            Percentage = mlPct,
            Details = missingLogic.Count == 0
                ? "All incomplete activities have proper logic ties."
                : $"{missingLogic.Count} of {incompleteCount} incomplete activities ({mlPct}%). {noPred.Count} no pred, {noSucc.Count} no succ."
        });
        if (mlPct <= 5) passed++;

        // ---- 2. Leads: 0% allowed ----
        var leads = relationships.Where(r => r.LagDuration < 0).ToList();
        var ldsPct = totalRelationships > 0 ? Math.Round(leads.Count * 100.0 / totalRelationships, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 2,
            Name = "Leads",
            Description = "Relationships with negative lag (0% allowed)",
            Status = leads.Count == 0 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = leads.Count,
            TotalCount = totalRelationships,
            Percentage = ldsPct,
            Details = leads.Count == 0
                ? "No leads found."
                : $"{leads.Count} relationship(s) have negative lag ({ldsPct}%)."
        });
        if (leads.Count == 0) passed++;

        // ---- 3. Lags: ≤ 5% of relationships ----
        var positiveLags = relationships.Where(r => r.LagDuration > 0).ToList();
        var lagPct = totalRelationships > 0 ? Math.Round(positiveLags.Count * 100.0 / totalRelationships, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 3,
            Name = "Lags",
            Description = "Relationships with positive lag (≤ 5% of relationships)",
            Status = positiveLags.Count == 0 ? DcmaStatus.Pass : lagPct > 5 ? DcmaStatus.Fail : DcmaStatus.Warning,
            IssueCount = positiveLags.Count,
            TotalCount = totalRelationships,
            Percentage = lagPct,
            Details = positiveLags.Count == 0
                ? "No lagged relationships found."
                : $"{positiveLags.Count} of {totalRelationships} relationships ({lagPct}%) have positive lag."
        });
        if (lagPct <= 5) passed++;

        // ---- 4. Relationship Types: ≥ 90% FS ----
        var fsCount = relationships.Count(r => r.Type is "FS" or "Finish-to-Start");
        var fsPct = totalRelationships > 0 ? Math.Round(fsCount * 100.0 / totalRelationships, 1) : 100;
        var nonFsCount = totalRelationships - fsCount;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 4,
            Name = "Relationship Types",
            Description = "Finish-to-Start (FS) relationships (≥ 90%)",
            Status = fsPct >= 90 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = nonFsCount,
            TotalCount = totalRelationships,
            Percentage = fsPct,
            Details = fsPct >= 90
                ? $"{fsPct}% FS relationships ({fsCount} of {totalRelationships})."
                : $"{fsPct}% FS — below 90% threshold. {nonFsCount} non-FS relationships found."
        });
        if (fsPct >= 90) passed++;

        // ---- 5. Hard Constraints: ≤ 5% of incomplete activities ----
        var constraintStatuses = new[] { "Status_MandatoryStart", "Status_MandatoryFinish", "Start_On", "Finish_On" };
        var hardConstrained = incompleteActivities.Where(a => constraintStatuses.Any(s => a.Status.Contains(s))).ToList();
        var hcPct = incompleteCount > 0 ? Math.Round(hardConstrained.Count * 100.0 / incompleteCount, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 5,
            Name = "Hard Constraints",
            Description = "Activities with mandatory start/finish constraints (≤ 5% of incomplete)",
            Status = hardConstrained.Count == 0 ? DcmaStatus.Pass : hcPct > 5 ? DcmaStatus.Fail : DcmaStatus.Warning,
            IssueCount = hardConstrained.Count,
            TotalCount = incompleteCount,
            Percentage = hcPct,
            Details = hardConstrained.Count == 0
                ? "No hard constraints detected."
                : $"{hardConstrained.Count} of {incompleteCount} incomplete activities ({hcPct}%) have hard constraints."
        });
        if (hcPct <= 5) passed++;

        // ---- 6. High Float: ≤ 5% of incomplete activities with TF ≥ 44 working days ----
        var highFloat = incompleteActivities
            .Where(a => a.RemainingDuration >= 44)
            .ToList();
        var hfPct = incompleteCount > 0 ? Math.Round(highFloat.Count * 100.0 / incompleteCount, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 6,
            Name = "High Float",
            Description = "Incomplete activities with Total Float ≥ 44 working days (≤ 5%)",
            Status = highFloat.Count == 0 ? DcmaStatus.Pass : hfPct > 5 ? DcmaStatus.Fail : DcmaStatus.Warning,
            IssueCount = highFloat.Count,
            TotalCount = incompleteCount,
            Percentage = hfPct,
            Details = highFloat.Count == 0
                ? "No high-float incomplete activities."
                : $"{highFloat.Count} of {incompleteCount} incomplete activities ({hfPct}%) have remaining duration ≥ 44."
        });
        if (hfPct <= 5) passed++;

        // ---- 7. Negative Float: 0% allowed ----
        var negFloat = incompleteActivities.Where(a => a.RemainingDuration < 0).ToList();
        var nfPct = incompleteCount > 0 ? Math.Round(negFloat.Count * 100.0 / incompleteCount, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 7,
            Name = "Negative Float",
            Description = "Activities with negative float (0% allowed)",
            Status = negFloat.Count == 0 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = negFloat.Count,
            TotalCount = incompleteCount,
            Percentage = nfPct,
            Details = negFloat.Count == 0
                ? "No negative float detected."
                : $"{negFloat.Count} of {incompleteCount} incomplete activities ({nfPct}%) have negative remaining duration."
        });
        if (negFloat.Count == 0) passed++;

        // ---- 8. High Duration: ≤ 5% of incomplete activities with duration > 44 working days ----
        var highDur = incompleteActivities.Where(a => a.Duration > 44).ToList();
        var hdPct = incompleteCount > 0 ? Math.Round(highDur.Count * 100.0 / incompleteCount, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 8,
            Name = "High Duration",
            Description = "Incomplete activities with duration > 44 working days (≤ 5%)",
            Status = highDur.Count == 0 ? DcmaStatus.Pass : hdPct > 5 ? DcmaStatus.Fail : DcmaStatus.Warning,
            IssueCount = highDur.Count,
            TotalCount = incompleteCount,
            Percentage = hdPct,
            Details = highDur.Count == 0
                ? "No activities exceed 44 days."
                : $"{highDur.Count} of {incompleteCount} incomplete activities ({hdPct}%) exceed 44 days."
        });
        if (hdPct <= 5) passed++;

        // ---- 9. Invalid Dates: 0% allowed ----
        var invalidDates = activities.Where(a => a.StartDate.HasValue && a.EndDate.HasValue && a.EndDate < a.StartDate).ToList();
        var idPct = totalActivities > 0 ? Math.Round(invalidDates.Count * 100.0 / totalActivities, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 9,
            Name = "Invalid Dates",
            Description = "Activities with finish date before start date (0% allowed)",
            Status = invalidDates.Count == 0 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = invalidDates.Count,
            TotalCount = totalActivities,
            Percentage = idPct,
            Details = invalidDates.Count == 0
                ? "All activities have valid date ranges."
                : $"{invalidDates.Count} of {totalActivities} activities ({idPct}%) have inverted dates."
        });
        if (invalidDates.Count == 0) passed++;

        // ---- 10. Resources: ≥ 90% of activities should be resource-loaded ----
        var activitiesWithResources = assignments.Select(a => a.TaskId).Distinct().ToHashSet();
        var resourceLoaded = activities.Count(a => activitiesWithResources.Contains(a.TaskId));
        var rlPct = totalActivities > 0 ? Math.Round(resourceLoaded * 100.0 / totalActivities, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 10,
            Name = "Resources",
            Description = "Activities with resource assignments (≥ 90%)",
            Status = rlPct >= 90 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = totalActivities - resourceLoaded,
            TotalCount = totalActivities,
            Percentage = rlPct,
            Details = rlPct >= 90
                ? $"{rlPct}% of activities are resource-loaded ({resourceLoaded} of {totalActivities})."
                : $"{rlPct}% — below 90% threshold. {totalActivities - resourceLoaded} activities without resources."
        });
        if (rlPct >= 90) passed++;

        // ---- 11. Missed Tasks: ≤ 5% ----
        var now = DateTime.UtcNow;
        var missedTasks = incompleteActivities.Where(a =>
            a.EndDate.HasValue && a.EndDate.Value < now).ToList();
        var mtPct = incompleteCount > 0 ? Math.Round(missedTasks.Count * 100.0 / incompleteCount, 1) : 0;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 11,
            Name = "Missed Tasks",
            Description = "Incomplete activities past their finish date (≤ 5%)",
            Status = missedTasks.Count == 0 ? DcmaStatus.Pass : mtPct > 5 ? DcmaStatus.Fail : DcmaStatus.Warning,
            IssueCount = missedTasks.Count,
            TotalCount = incompleteCount,
            Percentage = mtPct,
            Details = missedTasks.Count == 0
                ? "No missed tasks."
                : $"{missedTasks.Count} of {incompleteCount} incomplete activities ({mtPct}%) are past their finish date."
        });
        if (mtPct <= 5) passed++;

        // ---- 12. Critical Path Test: Must pass 100% ----
        var cpIssues = ComputeCriticalPathIssues(activities, relationships);
        var cpPct = cpIssues.total > 0 ? Math.Round(cpIssues.passed * 100.0 / cpIssues.total, 1) : 100;
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 12,
            Name = "Critical Path Test",
            Description = "Activities on critical path with proper logic (must pass 100%)",
            Status = cpIssues.failedCount == 0 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = cpIssues.failedCount,
            TotalCount = cpIssues.total,
            Percentage = cpPct,
            Details = cpIssues.failedCount == 0
                ? $"Critical path intact. {cpIssues.total} activities on longest path."
                : $"{cpIssues.failedCount} critical path issue(s). {cpPct}% pass rate."
        });
        if (cpIssues.failedCount == 0) passed++;

        // ---- 13. CPLI: ≥ 0.95 ----
        double cpli = ComputeCpli(activities);
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 13,
            Name = "CPLI",
            Description = "Critical Path Length Index (≥ 0.95)",
            Status = cpli >= 0.95 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = cpli >= 0.95 ? 0 : 1,
            TotalCount = 1,
            Percentage = Math.Round(cpli * 100, 1),
            Details = cpli >= 0.95
                ? $"CPLI = {cpli:F2} — meets threshold."
                : $"CPLI = {cpli:F2} — below 0.95 threshold."
        });
        if (cpli >= 0.95) passed++;

        // ---- 14. BEI: ≥ 0.95 ----
        double bei = ComputeBei(activities);
        points.Add(new DcmaAssessmentPointDto
        {
            PointNumber = 14,
            Name = "BEI",
            Description = "Baseline Execution Index (≥ 0.95)",
            Status = bei >= 0.95 ? DcmaStatus.Pass : DcmaStatus.Fail,
            IssueCount = bei >= 0.95 ? 0 : 1,
            TotalCount = 1,
            Percentage = Math.Round(bei * 100, 1),
            Details = bei >= 0.95
                ? $"BEI = {bei:F2} — meets threshold."
                : $"BEI = {bei:F2} — below 0.95 threshold."
        });
        if (bei >= 0.95) passed++;

        return new DcmaAssessmentResultDto
        {
            OverallScore = Math.Round(passed / 14.0 * 100, 1),
            Points = points
        };
    }

    private static (int total, int passed, int failedCount) ComputeCriticalPathIssues(
        List<PrimaveraActivityDto> activities, List<PrimaveraRelationshipDto> relationships)
    {
        if (activities.Count == 0) return (0, 0, 0);

        var es = new Dictionary<string, double>();
        var ef = new Dictionary<string, double>();
        var dur = new Dictionary<string, double>();
        var preds = new Dictionary<string, List<string>>();
        var succs = new Dictionary<string, List<string>>();

        foreach (var a in activities)
        {
            dur[a.TaskId] = Math.Max(a.Duration, 0);
            es[a.TaskId] = 0;
            ef[a.TaskId] = 0;
            preds[a.TaskId] = new();
            succs[a.TaskId] = new();
        }

        foreach (var r in relationships)
        {
            if (preds.ContainsKey(r.SuccTaskId))
                preds[r.SuccTaskId].Add(r.PredTaskId);
            if (succs.ContainsKey(r.PredTaskId))
                succs[r.PredTaskId].Add(r.SuccTaskId);
        }

        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var kv in dur)
            {
                var id = kv.Key;
                var predecessors = preds[id];
                double maxPredEF = 0;
                foreach (var p in predecessors)
                {
                    if (ef.TryGetValue(p, out var pEF) && pEF > maxPredEF)
                        maxPredEF = pEF;
                }
                var newES = maxPredEF;
                var newEF = newES + dur[id];
                if (Math.Abs(es[id] - newES) > 0.001)
                {
                    es[id] = newES;
                    ef[id] = newEF;
                    changed = true;
                }
            }
        }

        double projectFinish = ef.Values.Count > 0 ? ef.Values.Max() : 0;

        var ls = new Dictionary<string, double>();
        var lf = new Dictionary<string, double>();
        foreach (var kv in dur)
        {
            lf[kv.Key] = projectFinish;
            ls[kv.Key] = projectFinish - kv.Value;
        }

        changed = true;
        while (changed)
        {
            changed = false;
            foreach (var kv in dur)
            {
                var id = kv.Key;
                var successors = succs[id];
                double minSuccLS = projectFinish;
                foreach (var s in successors)
                {
                    if (ls.TryGetValue(s, out var sLS) && sLS < minSuccLS)
                        minSuccLS = sLS;
                }
                var newLF = minSuccLS;
                var newLS = newLF - dur[id];
                if (Math.Abs(lf[id] - newLF) > 0.001)
                {
                    lf[id] = newLF;
                    ls[id] = newLS;
                    changed = true;
                }
            }
        }

        var tf = new Dictionary<string, double>();
        foreach (var kv in dur)
        {
            tf[kv.Key] = ls[kv.Key] - es[kv.Key];
        }

        var criticalPath = tf.Where(kv => Math.Abs(kv.Value) < 0.001).Select(kv => kv.Key).ToHashSet();
        int total = criticalPath.Count;
        int failedCount = 0;

        foreach (var cpId in criticalPath)
        {
            var cpPreds = preds.GetValueOrDefault(cpId, new());
            if (cpPreds.Count == 0) continue;

            bool hasCriticalPred = cpPreds.Any(p => criticalPath.Contains(p));
            if (!hasCriticalPred)
                failedCount++;
        }

        int passedCount = total - failedCount;
        return (total, passedCount, failedCount);
    }

    private static double ComputeCpli(List<PrimaveraActivityDto> activities)
    {
        if (activities.Count == 0) return 1;

        double totalOriginalDuration = activities.Sum(a => Math.Max(a.Duration, 0));
        if (totalOriginalDuration <= 0) return 1;

        double earnedDuration = activities
            .Where(a => a.Status == "Status_Completed")
            .Sum(a => Math.Max(a.Duration, 0));

        double plannedRemaining = totalOriginalDuration - earnedDuration;
        double actualRemaining = activities
            .Where(a => a.Status != "Status_Completed")
            .Sum(a => Math.Max(a.RemainingDuration, 0));

        if (plannedRemaining <= 0) return 1;

        double cpli = actualRemaining > 0
            ? plannedRemaining / actualRemaining
            : 1;

        return Math.Min(cpli, 2.0);
    }

    private static double ComputeBei(List<PrimaveraActivityDto> activities)
    {
        if (activities.Count == 0) return 1;

        double totalDuration = activities.Sum(a => Math.Max(a.Duration, 0));
        if (totalDuration <= 0) return 1;

        double earnedDuration = activities
            .Sum(a => Math.Max(a.Duration, 0) * a.PercentComplete / 100.0);

        return Math.Min(earnedDuration / totalDuration * 2.0, 2.0);
    }
}
