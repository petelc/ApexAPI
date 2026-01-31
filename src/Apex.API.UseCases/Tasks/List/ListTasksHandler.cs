using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.List;

/// <summary>
/// Handler for listing tasks for a project
/// ✅ MATCHES: Updated TaskDto with department fields
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
            // Get all tasks
            var allTasks = await _taskRepository.ListAsync(cancellationToken);

            // Filter by project and tenant
            var tasks = allTasks
                .Where(t => t.ProjectId.Value == query.ProjectId &&
                           t.TenantId == _tenantContext.CurrentTenantId)
                .ToList();

            // Map to DTOs
            var taskDtos = tasks.Select(t => new TaskDto(
                Id: t.Id.Value,
                Title: t.Title,
                Description: t.Description,
                Status: t.Status.Name,
                Priority: t.Priority.Name,
                ProjectId: t.ProjectId.Value,

                // User Assignment
                AssignedToUserId: t.AssignedToUserId,

                // ✅ Department Assignment
                AssignedToDepartmentId: t.AssignedToDepartmentId?.Value,
                AssignedToDepartmentName: t.AssignedToDepartmentName,

                // ✅ Notes
                ImplementationNotes: t.ImplementationNotes,
                ResolutionNotes: t.ResolutionNotes,

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
                StartedByUserId: t.StartedByUserId,
                CompletedByUserId: t.CompletedByUserId,

                // User objects (null here, enriched in Web layer)
                CreatedByUser: null,
                AssignedToUser: null,
                StartedByUser: null,
                CompletedByUser: null
            )).ToList();

            // Calculate total pages
            var totalCount = taskDtos.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var response = new ListTasksResponse(
                Tasks: taskDtos,
                TotalCount: totalCount,
                PageNumber: query.PageNumber,
                PageSize: query.PageSize,
                TotalPages: totalPages
            );

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tasks for project: {ProjectId}", query.ProjectId);
            return Result.Error("An error occurred while retrieving tasks.");
        }
    }
}
