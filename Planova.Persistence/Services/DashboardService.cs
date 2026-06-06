using Microsoft.EntityFrameworkCore;
using Planova.Application.Dto;
using Planova.Application.Services;
using Planova.Persistence.DbContext;

namespace Planova.Persistence.Services;

public class DashboardService : IDashboardService
{
    private readonly PlanovaDbContext _context;

    public DashboardService(PlanovaDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var totalProjects = await _context.Projects.CountAsync(ct);
        var totalClients = await _context.Clients.CountAsync(ct);
        var totalContracts = await _context.Contracts.CountAsync(ct);
        var totalBoqs = await _context.Boqs.CountAsync(ct);
        var totalWbsEntries = await _context.WbsEntries.CountAsync(ct);
        var totalActivities = await _context.Activities.CountAsync(ct);
        var totalResources = await _context.Resources.CountAsync(ct);

        var projectsByStatus = await _context.Projects
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

        var boqs = await _context.Boqs.ToListAsync(ct);
        var boqStatusDistribution = boqs
            .GroupBy(b => b.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var wbsEntries = await _context.WbsEntries.ToListAsync(ct);
        var wbsStatusDistribution = wbsEntries
            .GroupBy(w => w.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var activities = await _context.Activities.ToListAsync(ct);
        var activitiesByStatus = activities
            .GroupBy(a => a.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var resources = await _context.Resources.ToListAsync(ct);
        var resourceTypeDistribution = resources
            .GroupBy(r => r.ResourceType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var recent = await _context.Projects
            .OrderByDescending(p => p.UpdatedAt)
            .Take(5)
            .Select(p => new RecentActivityItem("Project", p.Name, "Updated", p.UpdatedAt))
            .ToListAsync(ct);

        return new DashboardSummaryDto(
            totalProjects,
            totalClients,
            totalContracts,
            totalBoqs,
            totalWbsEntries,
            totalActivities,
            totalResources,
            projectsByStatus,
            boqStatusDistribution,
            wbsStatusDistribution,
            activitiesByStatus,
            resourceTypeDistribution,
            recent
        );
    }
}
