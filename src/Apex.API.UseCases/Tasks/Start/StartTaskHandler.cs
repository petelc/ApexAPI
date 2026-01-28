using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.Start;

public class StartTaskHandler : IRequestHandler<StartTaskCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<StartTaskHandler> _logger;

    public StartTaskHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<StartTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(StartTaskCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
                return Result.NotFound("Task not found.");

            if (task.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            // ✅ Get Guid from UserId
            var userId = _currentUserService.UserId!;

            task.Start(userId);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation("Task started: TaskId={TaskId}", command.TaskId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot start task: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting task: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while starting the task.");
        }
    }
}
