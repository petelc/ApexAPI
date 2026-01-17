
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.UseCases.Projects.DTOs;
using Apex.API.UseCases.Users.DTOs;
using Apex.API.UseCases.Users.Interfaces;

namespace Apex.API.Web.Extensions;

/// <summary>
/// Mapping extensions for Project - ENHANCED with all aggregate properties
/// </summary>
public static class ProjectMappingExtensions
{
    /// <summary>
    /// Maps Project aggregate to DTO - includes ALL available properties
    /// </summary>
    public static ProjectDto ToDto(this Project request)
    {
        return new ProjectDto
        {
            Id = request.Id.Value,
            Name = request.Name,
            CreatedByUserId = request.CreatedByUserId,
            CreatedByUser = null, // Populated by user lookup
            ProjectManagerUserId = request.ProjectManagerUserId,
            ProjectManager = null // Populated by user lookup
        };
    }

    /// <summary>
    /// Enriches ProjectDto with user information
    /// </summary>
    public static ProjectDto WithUsersInfo(this ProjectDto dto, Dictionary<Guid, UserSummaryDto> userLookup)
    {
        return dto with
        {
            CreatedByUser = userLookup.ContainsKey(dto.CreatedByUserId)
                ? userLookup[dto.CreatedByUserId]
                : null,
            ProjectManager = dto.ProjectManagerUserId.HasValue && userLookup.ContainsKey(dto.ProjectManagerUserId.Value)
                ? userLookup[dto.ProjectManagerUserId.Value]
                : null
        };
    }

    /// <summary>
    /// Maps multiple Projects to DTOs with user information (batch operation)
    /// </summary>
    public static async Task<List<ProjectDto>> ToDtosWithUsersAsync(this IEnumerable<Project> projects, IUserLookupService userLookupService,
    CancellationToken cancellationToken = default)
    {
        var projectList = projects.ToList();

        // Convert to DTOs first
        var dtos = projectList.Select(p => p.ToDto()).ToList();

        // Gather all unique user IDs to look up
        var userIds = new HashSet<Guid>();
        foreach (var dto in dtos)
        {
            userIds.Add(dto.CreatedByUserId);
            if (dto.ProjectManagerUserId.HasValue)
            {
                userIds.Add(dto.ProjectManagerUserId.Value);
            }
        }

        // Fetch user summaries
        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        return dtos.Select(dto => dto.WithUsersInfo(userLookup)).ToList();
    }

    /// <summary>
    /// Maps single Project to DTO with user information
    /// </summary>
    public static async Task<ProjectDto> ToDtoWithUsersAsync(this Project project, IUserLookupService userLookupService,
    CancellationToken cancellationToken = default)
    {
        var dto = project.ToDto();

        // Gather user IDs to look up
        var userIds = new List<Guid> { dto.CreatedByUserId };
        if (dto.ProjectManagerUserId.HasValue)
        {
            userIds.Add(dto.ProjectManagerUserId.Value);
        }
        // Lookup Users
        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        return dto.WithUsersInfo(userLookup);
    }

}