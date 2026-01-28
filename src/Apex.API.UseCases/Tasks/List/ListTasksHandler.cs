using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.List;

/// <summary>
/// Handler for listing tasks
/// ✅ ENHANCED: Maps all new TaskDto fields
/// </summary>
public class ListTasksHandler : IRequestHandler<ListTasksQuery, Result<ListTasksResponse>>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListTasksHandler> _logger;

    public ListTasksHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<ListTasksHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<ListTasksResponse>> Handle(
        ListTasksQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Listing tasks for project: ProjectId={ProjectId}, TenantId={TenantId}",
                query.ProjectId,
                _tenantContext.CurrentTenantId);

            // Get all tasks for the project
            var allTasks = await _taskRepository.ListAsync(cancellationToken);

            // Filter by project ID
            var projectTasks = allTasks
                .Where(t => t.ProjectId.Value == query.ProjectId)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            var totalCount = projectTasks.Count;

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            // Apply pagination and map to DTO
            var pagedTasks = projectTasks
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(t => new TaskDto(
                    Id: t.Id.Value,
                    ProjectId: t.ProjectId.Value,
                    Title: t.Title,
                    Description: t.Description,
                    Status: t.Status.Name,
                    Priority: t.Priority.Name,
                    
                    // ✅ NEW: Notes
                    ImplementationNotes: t.ImplementationNotes,
                    ResolutionNotes: t.ResolutionNotes,
                    
                    // Assignment
                    AssignedToUserId: t.AssignedToUserId,
                    AssignedToDepartmentId: t.AssignedToDepartmentId?.Value,  // ✅ NEW
                    
                    // Time Tracking
                    EstimatedHours: t.EstimatedHours,
                    ActualHours: t.ActualHours,
                    
                    // Dates
                    DueDate: t.DueDate,
                    CreatedDate: t.CreatedDate,
                    StartedDate: t.StartedDate,
                    CompletedDate: t.CompletedDate,
                    LastModifiedDate: t.LastModifiedDate,
                    
                    // Blocking
                    BlockedReason: t.BlockedReason,
                    BlockedDate: t.BlockedDate,
                    
                    // User Tracking
                    CreatedByUserId: t.CreatedByUserId,
                    StartedByUserId: t.StartedByUserId,      // ✅ NEW
                    CompletedByUserId: t.CompletedByUserId,  // ✅ NEW
                    
                    // User objects (enriched in Web layer)
                    CreatedByUser: null,
                    AssignedToUser: null,
                    StartedByUser: null,      // ✅ NEW
                    CompletedByUser: null     // ✅ NEW
                ))
                .ToList();

            var result = new ListTasksResponse(
                pagedTasks,
                totalCount,
                query.PageNumber,
                query.PageSize,
                totalPages
            );

            _logger.LogInformation(
                "Listed tasks: Count={Count}, ProjectId={ProjectId}",
                pagedTasks.Count,
                query.ProjectId);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tasks for project: {ProjectId}", query.ProjectId);
            return Result.Error("An error occurred while retrieving tasks.");
        }
    }
}
