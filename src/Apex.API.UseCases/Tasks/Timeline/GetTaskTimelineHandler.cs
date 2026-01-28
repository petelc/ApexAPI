using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Tasks.Timeline;

/// <summary>
/// Handler for getting task timeline/activity log
/// ✅ CLEAN ARCHITECTURE: Works through Task aggregate, no Infrastructure dependency
/// </summary>
public class GetTaskTimelineHandler : IRequestHandler<GetTaskTimelineQuery, Result<List<TaskActivityDto>>>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetTaskTimelineHandler> _logger;

    public GetTaskTimelineHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<GetTaskTimelineHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<TaskActivityDto>>> Handle(
        GetTaskTimelineQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get task with activity logs
            var task = await _taskRepository.GetByIdAsync(query.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", query.TaskId);
                return Result<List<TaskActivityDto>>.NotFound("Task not found.");
            }

            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized timeline access: TaskId={TaskId}, TenantId={TenantId}",
                    query.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result<List<TaskActivityDto>>.Forbidden();
            }

            // ✅ Get activity logs from aggregate
            var taskActivities = task.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new TaskActivityDto(
                    a.Id.Value,
                    a.ActivityType.Name,
                    a.Description,
                    a.Details,
                    a.UserId,
                    a.Timestamp,
                    UserName: null,  // Enriched in Web layer
                    UserEmail: null  // Enriched in Web layer
                ))
                .ToList();

            _logger.LogInformation(
                "Retrieved timeline: TaskId={TaskId}, ActivityCount={Count}",
                query.TaskId,
                taskActivities.Count);

            return Result<List<TaskActivityDto>>.Success(taskActivities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving timeline: TaskId={TaskId}", query.TaskId);
            return Result<List<TaskActivityDto>>.Error("An error occurred while retrieving the timeline.");
        }
    }
}
