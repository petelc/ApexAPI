using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Tasks.DTOs;

namespace Apex.API.UseCases.Tasks.GetById;

/// <summary>
/// Handler for getting a task by ID
/// Returns TaskDto without user information (enriched in Web layer)
/// ✅ ENHANCED: Maps all new fields
/// </summary>
public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    private readonly IReadRepository<Core.Aggregates.TaskAggregate.Task> _repository;
    private readonly ILogger<GetTaskByIdHandler> _logger;

    public GetTaskByIdHandler(
        IReadRepository<Core.Aggregates.TaskAggregate.Task> repository,
        ILogger<GetTaskByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<TaskDto>> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", request.TaskId);
                return Result.NotFound($"Task with ID {request.TaskId} not found");
            }

            // Map to DTO using positional constructor
            var dto = new TaskDto(
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
                AssignedToDepartmentId: task.AssignedToDepartmentId?.Value,  // ✅ NEW
                
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
                
                // User objects (enriched in Web layer)
                CreatedByUser: null,
                AssignedToUser: null,
                StartedByUser: null,      // ✅ NEW
                CompletedByUser: null     // ✅ NEW
            );

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task: {TaskId}", request.TaskId);
            return Result.Error("An error occurred while retrieving the task");
        }
    }
}
