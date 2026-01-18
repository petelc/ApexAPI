using Apex.API.UseCases.Projects.DTOs;
using Apex.API.UseCases.Users.DTOs;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.Core.Aggregates.ProjectAggregate;

namespace Apex.API.Web.Extensions;

/// <summary>
/// Mapping extensions for Project aggregate to DTO
/// Works with positional ProjectDto record
/// </summary>
public static class ProjectMappingExtensions
{
    /// <summary>
    /// Maps Project aggregate to DTO (without user information)
    /// Uses positional constructor for ProjectDto
    /// </summary>
    public static ProjectDto ToDto(this Project project)
    {
        return new ProjectDto(
            Id: project.Id.Value,
            Name: project.Name,
            Description: project.Description,
            Status: project.Status.Name,
            Priority: project.Priority.Name,
            ProjectRequestId: project.ProjectRequestId,
            Budget: project.Budget,
            StartDate: project.StartDate,
            EndDate: project.EndDate,
            ActualStartDate: project.ActualStartDate,
            ActualEndDate: project.ActualEndDate,
            CreatedByUserId: project.CreatedByUserId,
            ProjectManagerUserId: project.ProjectManagerUserId,
            CreatedDate: project.CreatedDate,
            LastModifiedDate: project.LastModifiedDate,
            IsOverdue: project.IsOverdue(),
            DaysUntilDeadline: project.GetDaysUntilDeadline(),
            DurationDays: project.GetDurationDays(),
            CreatedByUser: null,  // Populated in Web layer
            ProjectManagerUser: null  // Populated in Web layer
        );
    }

    /// <summary>
    /// Enriches ProjectDto with user information
    /// Uses 'with' expression for immutable record
    /// </summary>
    public static ProjectDto WithUserInfo(
        this ProjectDto dto,
        Dictionary<Guid, UserSummaryDto> userLookup)
    {
        return dto with
        {
            CreatedByUser = userLookup.GetValueOrDefault(dto.CreatedByUserId),
            
            ProjectManagerUser = dto.ProjectManagerUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.ProjectManagerUserId.Value)
                : null
        };
    }

    /// <summary>
    /// Maps multiple Projects to DTOs with user information (batch operation)
    /// </summary>
    public static async Task<List<ProjectDto>> ToDtosWithUsersAsync(
        this IEnumerable<Project> projects,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken = default)
    {
        var projectsList = projects.ToList();
        if (!projectsList.Any())
            return new List<ProjectDto>();

        // Convert to DTOs
        var dtos = projectsList.Select(p => p.ToDto()).ToList();

        // Collect all user IDs that need lookup
        var userIds = new HashSet<Guid>();
        foreach (var dto in dtos)
        {
            userIds.Add(dto.CreatedByUserId);
            
            if (dto.ProjectManagerUserId.HasValue)
                userIds.Add(dto.ProjectManagerUserId.Value);
        }

        // Batch lookup all users (single DB query)
        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        // Enrich DTOs with user information
        return dtos.Select(dto => dto.WithUserInfo(userLookup)).ToList();
    }

    /// <summary>
    /// Maps single Project to DTO with user information
    /// </summary>
    public static async Task<ProjectDto> ToDtoWithUsersAsync(
        this Project project,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken = default)
    {
        var dto = project.ToDto();

        // Collect user IDs
        var userIds = new List<Guid> { dto.CreatedByUserId };
        
        if (dto.ProjectManagerUserId.HasValue)
            userIds.Add(dto.ProjectManagerUserId.Value);

        // Lookup users
        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        // Enrich with user info
        return dto.WithUserInfo(userLookup);
    }
}
