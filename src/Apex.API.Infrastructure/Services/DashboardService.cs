using Apex.API.UseCases.Dashboard.DTOs;
using Apex.API.UseCases.Common.Interfaces;
using Apex.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Apex.API.Infrastructure.Services;

/// <summary>
/// Dashboard service implementation with caching
/// FIXED: Sequential queries to avoid DbContext concurrency issues
/// FIXED: Smart Enum handling for EF Core translation
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ApexDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public DashboardService(
        ApexDbContext context,
        IMemoryCache cache,
        ILogger<DashboardService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"dashboard_stats_{userId}";

        if (_cache.TryGetValue<DashboardStatsDto>(cacheKey, out var cachedStats) && cachedStats != null)
        {
            return cachedStats;
        }

        // ✅ FIXED: Execute queries SEQUENTIALLY to avoid DbContext concurrency issues
        var changeStats = await GetChangeManagementStatsAsync(cancellationToken);
        var projectStats = await GetProjectManagementStatsAsync(cancellationToken);
        var taskStats = await GetTaskManagementStatsAsync(userId, cancellationToken);
        var recentActivity = await GetRecentActivityAsync(cancellationToken);

        var stats = new DashboardStatsDto
        {
            ChangeManagement = changeStats,
            ProjectManagement = projectStats,
            TaskManagement = taskStats,
            RecentActivity = recentActivity
        };

        _cache.Set(cacheKey, stats, CacheDuration);
        return stats;
    }

    public async Task<ChangeManagementStatsDto> GetChangeManagementStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Get all changes for calculations
        var changes = await _context.ChangeRequests
            .AsNoTracking()
            .Select(c => new
            {
                c.Status,
                c.ScheduledStartDate
            })
            .ToListAsync(cancellationToken);

        var totalChanges = changes.Count;
        var draftChanges = changes.Count(c => c.Status.Value == 0); // Draft
        var pendingApproval = changes.Count(c => c.Status.Value == 1); // Submitted
        var approved = changes.Count(c => c.Status.Value == 2); // Approved
        var inProgress = changes.Count(c => c.Status.Value == 4); // InProgress
        var completed = changes.Count(c => c.Status.Value == 5); // Completed
        var failed = changes.Count(c => c.Status.Value == 6); // Failed

        var scheduledToday = changes.Count(c =>
            c.ScheduledStartDate.HasValue &&
            c.ScheduledStartDate.Value.Date == today);

        // Calculate success rate
        var totalFinished = completed + failed;
        var successRate = totalFinished > 0
            ? (decimal)completed / totalFinished * 100
            : 0;

        return new ChangeManagementStatsDto
        {
            TotalChanges = totalChanges,
            DraftChanges = draftChanges,
            PendingApproval = pendingApproval,
            Approved = approved,
            InProgress = inProgress,
            Completed = completed,
            Failed = failed,
            SuccessRate = Math.Round(successRate, 1),
            ScheduledToday = scheduledToday
        };
    }

    public async Task<ProjectManagementStatsDto> GetProjectManagementStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // ✅ FIXED: Get project request stats - load to memory first to avoid Smart Enum translation issues
        var allProjectRequests = await _context.ProjectRequests
            .AsNoTracking()
            .Select(pr => new { pr.Status })
            .ToListAsync(cancellationToken);

        var pendingRequests = allProjectRequests.Count(pr => pr.Status.Value == 0 || pr.Status.Value == 1); // Draft or Pending

        // Get project stats - load to memory first
        var projects = await _context.Projects
            .AsNoTracking()
            .Select(p => new
            {
                p.Status,
                p.EndDate
            })
            .ToListAsync(cancellationToken);

        var totalProjects = projects.Count;
        var activeProjects = projects.Count(p => p.Status.Value == 1); // Active
        var onHoldProjects = projects.Count(p => p.Status.Value == 2); // OnHold
        var completedProjects = projects.Count(p => p.Status.Value == 3); // Completed

        // Overdue = active projects with end date in the past
        var overdueProjects = projects.Count(p =>
            p.Status.Value == 1 &&
            p.EndDate.HasValue &&
            p.EndDate.Value < now);

        // Calculate completion rate
        var totalFinished = completedProjects + projects.Count(p => p.Status.Value == 4); // Completed + Cancelled
        var completionRate = totalProjects > 0
            ? (decimal)completedProjects / totalProjects * 100
            : 0;

        return new ProjectManagementStatsDto
        {
            TotalProjects = totalProjects,
            PendingRequests = pendingRequests,
            ActiveProjects = activeProjects,
            OnHoldProjects = onHoldProjects,
            CompletedProjects = completedProjects,
            OverdueProjects = overdueProjects,
            CompletionRate = Math.Round(completionRate, 1)
        };
    }

    public async Task<TaskManagementStatsDto> GetTaskManagementStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Get all tasks for calculations - load to memory first
        var tasks = await _context.Tasks
            .AsNoTracking()
            .Select(t => new
            {
                t.Status,
                t.DueDate,
                t.AssignedToUserId
            })
            .ToListAsync(cancellationToken);

        var totalTasks = tasks.Count;
        var openTasks = tasks.Count(t => t.Status.Value == 0); // Open
        var inProgressTasks = tasks.Count(t => t.Status.Value == 1); // InProgress
        var completedTasks = tasks.Count(t => t.Status.Value == 2); // Completed

        // Overdue = open or in-progress tasks with due date in the past
        var overdueTasks = tasks.Count(t =>
            (t.Status.Value == 0 || t.Status.Value == 1) &&
            t.DueDate.HasValue &&
            t.DueDate.Value.Date < today);

        // My tasks (assigned to current user)
        var myTasks = tasks.Count(t => t.AssignedToUserId == userId);

        // Due today
        var dueToday = tasks.Count(t =>
            (t.Status.Value == 0 || t.Status.Value == 1) &&
            t.DueDate.HasValue &&
            t.DueDate.Value.Date == today);

        // Calculate completion rate
        var completionRate = totalTasks > 0
            ? (decimal)completedTasks / totalTasks * 100
            : 0;

        return new TaskManagementStatsDto
        {
            TotalTasks = totalTasks,
            OpenTasks = openTasks,
            InProgressTasks = inProgressTasks,
            CompletedTasks = completedTasks,
            OverdueTasks = overdueTasks,
            MyTasks = myTasks,
            DueToday = dueToday,
            CompletionRate = Math.Round(completionRate, 1)
        };
    }

    private async Task<RecentActivityDto> GetRecentActivityAsync(
        CancellationToken cancellationToken = default)
    {
        // Get recent changes (last 5)
        var recentChanges = await _context.ChangeRequests
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedDate)
            .Take(5)
            .Select(c => new RecentChangeDto
            {
                Id = c.Id.Value,
                Title = c.Title,
                Status = c.Status.Name,
                CreatedDate = c.CreatedDate
            })
            .ToListAsync(cancellationToken);

        // Get recent projects (last 5)
        var recentProjects = await _context.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedDate)
            .Take(5)
            .Select(p => new RecentProjectDto
            {
                Id = p.Id.Value,
                Name = p.Name,
                Status = p.Status.Name,
                CreatedDate = p.CreatedDate
            })
            .ToListAsync(cancellationToken);

        // Get recent tasks (last 5)
        var recentTasks = await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedDate)
            .Take(5)
            .Select(t => new RecentTaskDto
            {
                Id = t.Id.Value,
                Title = t.Title,
                Status = t.Status.Name,
                DueDate = t.DueDate
            })
            .ToListAsync(cancellationToken);

        return new RecentActivityDto
        {
            RecentChanges = recentChanges,
            RecentProjects = recentProjects,
            RecentTasks = recentTasks
        };
    }
}
