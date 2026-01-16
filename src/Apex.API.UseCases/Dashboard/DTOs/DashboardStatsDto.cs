namespace Apex.API.UseCases.Dashboard.DTOs;

/// <summary>
/// Complete dashboard statistics
/// </summary>
public sealed record DashboardStatsDto
{
    public ChangeManagementStatsDto ChangeManagement { get; init; } = new();
    public ProjectManagementStatsDto ProjectManagement { get; init; } = new();
    public TaskManagementStatsDto TaskManagement { get; init; } = new();
    public RecentActivityDto RecentActivity { get; init; } = new();
}

/// <summary>
/// Change Management statistics
/// </summary>
public sealed record ChangeManagementStatsDto
{
    public int TotalChanges { get; init; }
    public int DraftChanges { get; init; }
    public int PendingApproval { get; init; }
    public int Approved { get; init; }
    public int InProgress { get; init; }
    public int Completed { get; init; }
    public int Failed { get; init; }
    public decimal SuccessRate { get; init; }
    public int ScheduledToday { get; init; }
}

/// <summary>
/// Project Management statistics
/// </summary>
public sealed record ProjectManagementStatsDto
{
    public int TotalProjects { get; init; }
    public int PendingRequests { get; init; }
    public int ActiveProjects { get; init; }
    public int OnHoldProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int OverdueProjects { get; init; }
    public decimal CompletionRate { get; init; }
}

/// <summary>
/// Task Management statistics
/// </summary>
public sealed record TaskManagementStatsDto
{
    public int TotalTasks { get; init; }
    public int OpenTasks { get; init; }
    public int InProgressTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int OverdueTasks { get; init; }
    public int MyTasks { get; init; }
    public int DueToday { get; init; }
    public decimal CompletionRate { get; init; }
}

/// <summary>
/// Recent activity on the dashboard
/// </summary>
public sealed record RecentActivityDto
{
    public List<RecentChangeDto> RecentChanges { get; init; } = new();
    public List<RecentProjectDto> RecentProjects { get; init; } = new();
    public List<RecentTaskDto> RecentTasks { get; init; } = new();
}

/// <summary>
/// Recent change item
/// </summary>
public sealed record RecentChangeDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
}

/// <summary>
/// Recent project item
/// </summary>
public sealed record RecentProjectDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
}

/// <summary>
/// Recent task item
/// </summary>
public sealed record RecentTaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? DueDate { get; init; }
}
