using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Update;

/// <summary>
/// Handler for updating task details
/// </summary>
public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateTaskHandler> _logger;

    public UpdateTaskHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<UpdateTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateTaskCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the task
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", command.TaskId);
                return Result.NotFound("Task not found.");
            }

            // Verify tenant ownership
            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized update attempt: TaskId={TaskId}, TenantId={TenantId}",
                    command.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }

            // Parse priority
            if (!RequestPriority.TryFromName(command.Priority, out var priority))
            {
                return Result.Error($"Invalid priority: {command.Priority}");
            }

            // Update the task
            task.Update(
                command.Title,
                command.Description,
                priority,
                command.EstimatedHours,
                command.DueDate);

            // Save changes
            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Task updated: TaskId={TaskId}, Title={Title}",
                command.TaskId,
                command.Title);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot update task: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid task data: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while updating the task.");
        }
    }
}
