using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.Claim;

public class ClaimTaskHandler : IRequestHandler<ClaimTaskCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ClaimTaskHandler> _logger;

    public ClaimTaskHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<ClaimTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(ClaimTaskCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
                return Result.NotFound("Task not found.");

            if (task.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            // ✅ SIMPLIFIED: Just pass user ID - no department required
            var userId = _currentUserService.UserId!;
            task.ClaimTask(userId);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Task claimed: TaskId={TaskId}, UserId={UserId}",
                command.TaskId,
                userId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot claim task: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming task: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while claiming the task.");
        }
    }
}
