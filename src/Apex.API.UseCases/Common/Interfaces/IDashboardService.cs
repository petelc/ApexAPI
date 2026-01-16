using Apex.API.UseCases.Dashboard.DTOs;

namespace Apex.API.UseCases.Common.Interfaces;

/// <summary>
/// Service for retrieving dashboard statistics
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets complete dashboard statistics
    /// </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change management statistics only
    /// </summary>
    Task<ChangeManagementStatsDto> GetChangeManagementStatsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets project management statistics only
    /// </summary>
    Task<ProjectManagementStatsDto> GetProjectManagementStatsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task management statistics only
    /// </summary>
    Task<TaskManagementStatsDto> GetTaskManagementStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
