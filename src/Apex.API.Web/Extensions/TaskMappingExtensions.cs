using Apex.API.UseCases.Users.DTOs;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Users.Interfaces;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.Web.Extensions;

/// <summary>
/// Mapping extensions for Task aggregate to DTO
/// ✅ ENHANCED: Maps all new fields (notes, user tracking, department)
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
            
            // ✅ NEW: Notes
            ImplementationNotes: task.ImplementationNotes,
            ResolutionNotes: task.ResolutionNotes,
            
            // Assignment
            AssignedToUserId: task.AssignedToUserId,
            AssignedToDepartmentId: task.AssignedToDepartmentId?.Value,  // ✅ NEW: Extract Guid from value object
            
            // Time Tracking
            EstimatedHours: task.EstimatedHours,
            ActualHours: task.ActualHours,
            
            // Dates
            DueDate: task.DueDate,
            CreatedDate: task.CreatedDate,
            StartedDate: task.StartedDate,
            CompletedDate: task.CompletedDate,
            LastModifiedDate: task.LastModifiedDate,
            
            // Blocking
            BlockedReason: task.BlockedReason,
            BlockedDate: task.BlockedDate,
            
            // User Tracking
            CreatedByUserId: task.CreatedByUserId,
            StartedByUserId: task.StartedByUserId,      // ✅ NEW
            CompletedByUserId: task.CompletedByUserId,  // ✅ NEW
            
            // User objects (populated in Web layer)
            CreatedByUser: null,
            AssignedToUser: null,
            StartedByUser: null,      // ✅ NEW
            CompletedByUser: null     // ✅ NEW
        );
    }

    /// <summary>
    /// Enriches TaskDto with user information
    /// Uses 'with' expression for immutable record
    /// ✅ ENHANCED: Enriches starter and completer users
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
                : null,
            
            // ✅ NEW: Enrich starter
            StartedByUser = dto.StartedByUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.StartedByUserId.Value)
                : null,
            
            // ✅ NEW: Enrich completer
            CompletedByUser = dto.CompletedByUserId.HasValue
                ? userLookup.GetValueOrDefault(dto.CompletedByUserId.Value)
                : null
        };
    }

    /// <summary>
    /// Maps multiple Tasks to DTOs with user information (batch operation)
    /// ✅ ENHANCED: Collects starter and completer user IDs
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
                userIds.Add(dto.AssignedToUserId.Value);
            
            // ✅ NEW: Collect starter and completer IDs
            if (dto.StartedByUserId.HasValue)
                userIds.Add(dto.StartedByUserId.Value);
            
            if (dto.CompletedByUserId.HasValue)
                userIds.Add(dto.CompletedByUserId.Value);
        }

        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        return dtos
            .Select(dto => dto.WithUserInfo(userLookup))
            .ToList();
    }

    /// <summary>
    /// Maps single Task to DTO with user information
    /// ✅ ENHANCED: Includes starter and completer
    /// </summary>
    public static async Task<TaskDto> ToDtoWithUsersAsync(
        this Core.Aggregates.TaskAggregate.Task task,
        IUserLookupService userLookupService,
        CancellationToken cancellationToken)
    {
        var dto = task.ToDto();

        var userIds = new List<Guid> { dto.CreatedByUserId };
        
        if (dto.AssignedToUserId.HasValue)
            userIds.Add(dto.AssignedToUserId.Value);
        
        // ✅ NEW: Add starter and completer
        if (dto.StartedByUserId.HasValue)
            userIds.Add(dto.StartedByUserId.Value);
        
        if (dto.CompletedByUserId.HasValue)
            userIds.Add(dto.CompletedByUserId.Value);

        var userLookup = await userLookupService.GetUserSummariesByIdsAsync(userIds, cancellationToken);

        return dto.WithUserInfo(userLookup);
    }
}
