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

        var byStatus = await _context.Projects
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, ct);

        var recent = await _context.Projects
            .OrderByDescending(p => p.UpdatedAt)
            .Take(5)
            .Select(p => new RecentActivityItem("Project", p.Name, "Updated", p.UpdatedAt))
            .ToListAsync(ct);

        return new DashboardSummaryDto(
            totalProjects,
            totalClients,
            totalContracts,
            byStatus,
            recent
        );
    }
}
