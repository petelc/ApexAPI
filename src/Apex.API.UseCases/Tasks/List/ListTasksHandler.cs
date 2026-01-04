using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;

namespace Apex.API.UseCases.Tasks.List;

public class ListTasksHandler : IRequestHandler<ListTasksQuery, Result<PagedResult<TaskDto>>>
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

    public async Task<Result<PagedResult<TaskDto>>> Handle(
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
                .Where(t => t.ProjectId == query.ProjectId)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            var totalCount = projectTasks.Count;

            // Apply pagination and map to DTO
            var pagedTasks = projectTasks
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(t => new TaskDto(
                    Id: t.Id.Value,  // TaskId value object → Guid
                    Title: t.Title,
                    Description: t.Description,
                    Status: t.Status.ToString(),
                    Priority: t.Priority.ToString(),
                    AssignedToUserId: t.AssignedToUserId,  // Already Guid?
                    AssignedToUserName: null,  // Not stored - would need lookup
                    AssignedToDepartmentId: t.AssignedToDepartmentId?.Value,  // DepartmentId? → Guid?
                    AssignedToDepartmentName: null,  // Not stored - would need lookup
                    EstimatedHours: t.EstimatedHours,  // decimal? - keep nullable
                    ActualHours: t.ActualHours,
                    CreatedDate: t.CreatedDate,
                    StartedDate: t.StartedDate,
                    CompletedDate: t.CompletedDate,
                    BlockedReason: t.BlockedReason
                ))
                .ToList();

            var result = new PagedResult<TaskDto>(
                pagedTasks,
                totalCount,
                query.PageNumber,
                query.PageSize
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
