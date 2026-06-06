namespace Planova.Application.Dto;

public record DashboardSummaryDto(
    int TotalProjects,
    int TotalClients,
    int TotalContracts,
    int TotalBoqs,
    int TotalWbsEntries,
    int TotalActivities,
    int TotalResources,
    Dictionary<string, int> ProjectsByStatus,
    Dictionary<string, int> BoqStatusDistribution,
    Dictionary<string, int> WbsStatusDistribution,
    Dictionary<string, int> ActivitiesByStatus,
    Dictionary<string, int> ResourceTypeDistribution,
    List<RecentActivityItem> RecentActivity
);

public record RecentActivityItem(
    string EntityType,
    string EntityName,
    string Action,
    DateTime Timestamp
);
