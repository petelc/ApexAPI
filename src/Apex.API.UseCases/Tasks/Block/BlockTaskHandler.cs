using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.Block;

public class BlockTaskHandler : IRequestHandler<BlockTaskCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BlockTaskHandler> _logger;

    public BlockTaskHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<BlockTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(BlockTaskCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
                return Result.NotFound("Task not found.");

            if (task.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            task.Block(command.Reason, _currentUserService.UserId);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation("Task blocked: TaskId={TaskId}, Reason={Reason}", command.TaskId, command.Reason);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking task: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while blocking the task.");
        }
    }
}
