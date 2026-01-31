using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.GetById;

/// <summary>
/// Handler for getting a single task by ID
/// ✅ MATCHES: Updated TaskDto with department fields
/// </summary>
public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTaskByIdHandler> _logger;

    public GetTaskByIdHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<GetTaskByIdHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TaskDto>> Handle(
        GetTaskByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(query.TaskId, cancellationToken);

            if (task == null)
            {
                return Result.NotFound("Task not found.");
            }

            // Verify tenant ownership
            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Forbidden();
            }

            // Map to DTO
            var dto = new TaskDto(
                Id: task.Id.Value,
                Title: task.Title,
                Description: task.Description,
                Status: task.Status.Name,
                Priority: task.Priority.Name,
                ProjectId: task.ProjectId.Value,

                // User Assignment
                AssignedToUserId: task.AssignedToUserId,

                // ✅ Department Assignment
                AssignedToDepartmentId: task.AssignedToDepartmentId?.Value,
                AssignedToDepartmentName: task.AssignedToDepartmentName,

                // ✅ Notes
                ImplementationNotes: task.ImplementationNotes,
                ResolutionNotes: task.ResolutionNotes,

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
                StartedByUserId: task.StartedByUserId,
                CompletedByUserId: task.CompletedByUserId,

                // User objects (null here, enriched in Web layer)
                CreatedByUser: null,
                AssignedToUser: null,
                StartedByUser: null,
                CompletedByUser: null
            );

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task by ID: {TaskId}", query.TaskId);
            return Result.Error("An error occurred while retrieving the task.");
        }
    }
}
