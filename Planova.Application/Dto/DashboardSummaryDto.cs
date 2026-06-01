namespace Planova.Application.Dto;

public record DashboardSummaryDto(
    int TotalProjects,
    int TotalClients,
    int TotalContracts,
    Dictionary<string, int> ProjectsByStatus,
    List<RecentActivityItem> RecentActivity
);

public record RecentActivityItem(
    string EntityType,
    string EntityName,
    string Action,
    DateTime Timestamp
);
