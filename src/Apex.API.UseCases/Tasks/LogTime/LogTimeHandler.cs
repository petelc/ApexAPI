using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.LogTime;

public class LogTimeHandler : IRequestHandler<LogTimeCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogTimeHandler> _logger;

    public LogTimeHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<LogTimeHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(LogTimeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
                return Result.NotFound("Task not found.");

            if (task.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            task.LogTime(command.Hours);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Time logged: TaskId={TaskId}, Hours={Hours}, Total={Total}",
                command.TaskId,
                command.Hours,
                task.ActualHours);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot log time: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid hours: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging time: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while logging time.");
        }
    }
}
