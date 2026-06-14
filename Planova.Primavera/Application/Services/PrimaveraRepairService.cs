using Planova.Primavera.Application.Dto;
using Planova.Primavera.Domain.Entities;
using Planova.Primavera.Domain.Enums;
using Planova.Primavera.Domain.Interfaces;
using Planova.Shared.Abstractions;

namespace Planova.Primavera.Application.Services;

public class PrimaveraRepairService : IPrimaveraRepairService
{
    private readonly IPrimaveraValidationService _validationService;
    private readonly IPrimaveraRepairRepository _repository;
    private readonly IPrimaveraWorkspaceService _workspaceService;
    private readonly ILoggingService _logger;

    public PrimaveraRepairService(
        IPrimaveraValidationService validationService,
        IPrimaveraRepairRepository repository,
        IPrimaveraWorkspaceService workspaceService,
        ILoggingService logger)
    {
        _validationService = validationService;
        _repository = repository;
        _workspaceService = workspaceService;
        _logger = logger;
    }

    public async Task<List<PrimaveraRepairActionDto>> GetSuggestedFixesAsync(int projectId, CancellationToken ct = default)
    {
        var issues = await _validationService.ValidateAsync(projectId, ct);
        var fixes = new List<PrimaveraRepairActionDto>();

        foreach (var issue in issues.Where(i => i.SuggestedFix != null))
        {
            var action = new PrimaveraRepairAction
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                IssueId = Guid.Empty,
                Description = issue.SuggestedFix!,
                TargetEntityType = Enum.Parse<PrimaveraEntityType>(issue.EntityType),
                AppliedBy = "System",
                AppliedAt = DateTime.UtcNow,
                UndoAvailable = true
            };
            await _repository.AddActionAsync(action, ct);

            fixes.Add(new PrimaveraRepairActionDto
            {
                Id = action.Id,
                Description = action.Description,
                TargetEntityType = action.TargetEntityType.ToString(),
                AppliedBy = action.AppliedBy,
                AppliedAt = action.AppliedAt,
                UndoAvailable = action.UndoAvailable
            });
        }

        return fixes;
    }

    public async Task<bool> ApplyFixAsync(Guid actionId, CancellationToken ct = default)
    {
        var actions = await _repository.GetActionsAsync(0, ct);
        var action = actions.FirstOrDefault(a => a.Id == actionId);
        if (action == null) return false;

        action.UndoAvailable = false;
        await _repository.UpdateActionAsync(action, ct);
        return true;
    }

    public async Task<bool> ApplyAllFixesAsync(int projectId, CancellationToken ct = default)
    {
        var fixes = await GetSuggestedFixesAsync(projectId, ct);
        bool allApplied = true;
        int appliedCount = 0;

        foreach (var fix in fixes)
        {
            if (await ApplyFixAsync(fix.Id, ct))
                appliedCount++;
            else
                allApplied = false;
        }

        _logger.Info("Repair batch applied: {AppliedCount}/{TotalCount} fixes for project {ProjectId} at {Time}",
            appliedCount, fixes.Count, projectId, DateTime.UtcNow);

        return allApplied;
    }
}
