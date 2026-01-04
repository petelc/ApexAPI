using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Tasks.GetById;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    private readonly IReadRepository<Core.Aggregates.TaskAggregate.Task> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTaskByIdHandler> _logger;
    public GetTaskByIdHandler(
        IReadRepository<Core.Aggregates.TaskAggregate.Task> repository,
        ITenantContext tenantContext,
        ILogger<GetTaskByIdHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TaskDto>> Handle(
        GetTaskByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _repository.GetByIdAsync(query.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: TaskId={TaskId}", query.TaskId);
                return Result<TaskDto>.NotFound("Task not found.");
            }

            // Verify tenant ownership
            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: TaskId={TaskId}, TenantId={TenantId}",
                    query.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result<TaskDto>.Forbidden();
            }

            var dto = new TaskDto(
                task.Id.Value,
                task.Title,
                task.Description,
                task.Status.Name,
                task.Priority.Name,
                task.AssignedToUserId,
                null, // AssignedToUserName not available on Task entity
                null, // AssignedToDepartmentId not available on Task entity
                null, // AssignedToDepartmentName not available on Task entity
                task.EstimatedHours,
                task.ActualHours,
                //task.CreatedByUserId,
                //task.ProjectManagerUserId,
                task.CreatedDate,
                task.StartedDate,
                task.CompletedDate,
                task.BlockedReason
            );

            return Result<TaskDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task: TaskId={TaskId}", query.TaskId);
            return Result<TaskDto>.Error("An error occurred while retrieving the task.");
        }
    }
}