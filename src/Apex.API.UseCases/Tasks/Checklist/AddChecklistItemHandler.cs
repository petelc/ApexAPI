using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tasks.Checklist;

/// <summary>
/// Handler for adding a checklist item
/// ✅ CLEAN ARCHITECTURE: Works through Task aggregate, no Infrastructure dependency
/// </summary>
public class AddChecklistItemHandler : IRequestHandler<AddChecklistItemCommand, Result<TaskChecklistItemId>>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AddChecklistItemHandler> _logger;

    public AddChecklistItemHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<AddChecklistItemHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<TaskChecklistItemId>> Handle(
        AddChecklistItemCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get task with checklist items (use specification if needed)
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", command.TaskId);
                return Result<TaskChecklistItemId>.NotFound("Task not found.");
            }

            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized checklist add attempt: TaskId={TaskId}, TenantId={TenantId}",
                    command.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result<TaskChecklistItemId>.Forbidden();
            }

            // ✅ Add checklist item through aggregate
            var item = task.AddChecklistItem(command.Description, command.Order);

            // Save task (will cascade save checklist items)
            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Checklist item added: TaskId={TaskId}, ItemId={ItemId}",
                command.TaskId,
                item.Id);

            return Result<TaskChecklistItemId>.Success(item.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid checklist item: {Message}", ex.Message);
            return Result<TaskChecklistItemId>.Error(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot add checklist item: {Message}", ex.Message);
            return Result<TaskChecklistItemId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding checklist item: TaskId={TaskId}", command.TaskId);
            return Result<TaskChecklistItemId>.Error("An error occurred while adding the checklist item.");
        }
    }
}
