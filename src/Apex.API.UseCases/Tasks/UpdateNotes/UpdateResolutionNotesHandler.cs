using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Tasks.UpdateNotes;

/// <summary>
/// Handler for updating resolution notes
/// </summary>
public class UpdateResolutionNotesHandler : IRequestHandler<UpdateResolutionNotesCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateResolutionNotesHandler> _logger;

    public UpdateResolutionNotesHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ILogger<UpdateResolutionNotesHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateResolutionNotesCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", command.TaskId);
                return Result.NotFound("Task not found.");
            }

            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized update attempt: TaskId={TaskId}, TenantId={TenantId}",
                    command.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }

            task.UpdateResolutionNotes(command.Notes);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Resolution notes updated: TaskId={TaskId}",
                command.TaskId);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid notes: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resolution notes: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while updating resolution notes.");
        }
    }
}
