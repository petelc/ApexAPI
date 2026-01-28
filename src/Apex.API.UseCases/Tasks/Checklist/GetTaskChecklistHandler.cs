using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Tasks.Checklist;

/// <summary>
/// Handler for getting task checklist
/// ✅ CLEAN ARCHITECTURE: Works through Task aggregate, no Infrastructure dependency
/// </summary>
public class GetTaskChecklistHandler : IRequestHandler<GetTaskChecklistQuery, Result<List<TaskChecklistItemDto>>>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTaskChecklistHandler> _logger;

    public GetTaskChecklistHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<GetTaskChecklistHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<TaskChecklistItemDto>>> Handle(
        GetTaskChecklistQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get task with checklist items
            var task = await _taskRepository.GetByIdAsync(query.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", query.TaskId);
                return Result<List<TaskChecklistItemDto>>.NotFound("Task not found.");
            }

            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized checklist access: TaskId={TaskId}, TenantId={TenantId}",
                    query.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result<List<TaskChecklistItemDto>>.Forbidden();
            }

            // ✅ Get checklist items from aggregate
            var taskItems = task.ChecklistItems
                .OrderBy(i => i.Order)
                .Select(i => new TaskChecklistItemDto(
                    i.Id.Value,
                    i.Description,
                    i.IsCompleted,
                    i.Order,
                    i.CompletedByUserId,
                    i.CompletedDate,
                    i.CreatedDate))
                .ToList();

            _logger.LogInformation(
                "Retrieved checklist: TaskId={TaskId}, ItemCount={Count}",
                query.TaskId,
                taskItems.Count);

            return Result<List<TaskChecklistItemDto>>.Success(taskItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checklist: TaskId={TaskId}", query.TaskId);
            return Result<List<TaskChecklistItemDto>>.Error("An error occurred while retrieving the checklist.");
        }
    }
}
