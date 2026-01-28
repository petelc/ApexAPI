using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.Checklist;

/// <summary>
/// Handler for toggling checklist item completion
/// ✅ CLEAN ARCHITECTURE: Works through Task aggregate, no Infrastructure dependency
/// </summary>
public class ToggleChecklistItemHandler : IRequestHandler<ToggleChecklistItemCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToggleChecklistItemHandler> _logger;

    public ToggleChecklistItemHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<ToggleChecklistItemHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ToggleChecklistItemCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Get all tasks and find the one containing this checklist item
            // Alternative: Add a specification to find task by checklist item ID
            var allTasks = await _taskRepository.ListAsync(cancellationToken);
            
            var task = allTasks.FirstOrDefault(t => 
                t.ChecklistItems.Any(i => i.Id == command.ItemId));

            if (task == null)
            {
                _logger.LogWarning("Task not found for checklist item: {ItemId}", command.ItemId);
                return Result.NotFound("Checklist item not found.");
            }

            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized checklist toggle attempt: TaskId={TaskId}, TenantId={TenantId}",
                    task.Id,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }

            // ✅ Get item from aggregate
            var item = task.GetChecklistItem(command.ItemId);
            
            if (item == null)
            {
                return Result.NotFound("Checklist item not found.");
            }

            // Toggle completion
            item.Toggle(_currentUserService.UserId);

            // Save task (will cascade save checklist items)
            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Checklist item toggled: ItemId={ItemId}, IsCompleted={IsCompleted}",
                command.ItemId,
                item.IsCompleted);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot toggle checklist item: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling checklist item: ItemId={ItemId}", command.ItemId);
            return Result.Error("An error occurred while toggling the checklist item.");
        }
    }
}
