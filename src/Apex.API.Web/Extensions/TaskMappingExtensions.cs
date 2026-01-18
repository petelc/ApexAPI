using Apex.API.UseCases.Users.DTOs;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.Web.Extensions;

/// <summary>
/// Mapping extensions for Task aggregate to DTO
/// Works with positional TaskDto record
/// </summary>
public static class TaskMappingExtensions
{
    /// <summary>
    /// Maps Task aggregate to DTO (without user information)
    /// Uses positional constructor for TaskDto
    /// </summary>
    public static TaskDto ToDto(this Core.Aggregates.TaskAggregate.Task task)
    {
        return new TaskDto(
            Id: task.Id.Value,
            ProjectId: task.ProjectId.Value,
            Title: task.Title,
            Description: task.Description,
            Status: task.Status.Name,
            Priority: task.Priority.Name,
            AssignedToUserId: task.AssignedToUserId,
            EstimatedHours: task.EstimatedHours,
            ActualHours: task.ActualHours,
            DueDate: task.DueDate,
            CreatedDate: task.CreatedDate,
            StartedDate: task.StartedDate,
            CompletedDate: task.CompletedDate,
            LastModifiedDate: task.LastModifiedDate,
            BlockedReason: task.BlockedReason,
            BlockedDate: task.BlockedDate,
            CreatedByUserId: task.CreatedByUserId,
            CreatedByUser: null,  // Populated in Web layer
            AssignedToUser: null    // Populated in Web layer
        );
    }

    /// <summary>
    /// Enriches TaskDto with user information
    /// Uses 'with' expression for immutable record
    /// </summary>
    public static TaskDto WithUserInfo(
        this TaskDto dto,
        Dictionary<Guid, UserSummaryDto> userLookup)
    {
        return dto with
        {
            CreatedByUser = userLookup.GetValueOrDefault(dto.CreatedByUserId),

            AssignedToUser = dto.AssignedToUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.AssignedToUserId.Value)
                : null

        };
    }

    /// <summary>
    /// Maps multiple Tasks to DTOs with user information (batch operation)
    /// </summary>
    public static async Task<List<TaskDto>> ToDtosWithUsersAsync(
        this IEnumerable<Core.Aggregates.TaskAggregate.Task> tasks,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken)
    {
        var taskList = tasks.ToList();
        if (!taskList.Any())
        {
            return new List<TaskDto>();
        }

        var dtos = taskList.Select(t => t.ToDto()).ToList();

        var userIds = new HashSet<Guid>();
        foreach (var dto in dtos)
        {
            userIds.Add(dto.CreatedByUserId);
            if (dto.AssignedToUserId.HasValue)
            {
                userIds.Add(dto.AssignedToUserId.Value);
            }
        }

        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        return dtos
            .Select(dto => dto.WithUserInfo(userLookup))
            .ToList();
    }

    public static async Task<TaskDto> ToDtoWithUsersAsync(
        this Core.Aggregates.TaskAggregate.Task task,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken)
    {
        var dto = task.ToDto();

        var userIds = new List<Guid> { dto.CreatedByUserId };
        if (dto.AssignedToUserId.HasValue)
        {
            userIds.Add(dto.AssignedToUserId.Value);
        }

        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        return dto.WithUserInfo(userLookup);
    }
}