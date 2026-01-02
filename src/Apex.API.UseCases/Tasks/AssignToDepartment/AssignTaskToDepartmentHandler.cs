using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.AssignToDepartment;

public class AssignTaskToDepartmentHandler : IRequestHandler<AssignTaskToDepartmentCommand, Result>
{
    private readonly IRepository<Apex.API.Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AssignTaskToDepartmentHandler> _logger;

    public AssignTaskToDepartmentHandler(
        IRepository<Apex.API.Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<AssignTaskToDepartmentHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AssignTaskToDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the task
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", command.TaskId);
                return Result.NotFound("Task not found.");
            }

            // Verify tenant ownership
            if (task.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized assignment attempt: TaskId={TaskId}, TenantId={TenantId}",
                    command.TaskId,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }

            // Assign to department
            task.AssignToDepartment(command.DepartmentId, _currentUserService.UserId);

            // Save changes
            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Task assigned to department: TaskId={TaskId}, DepartmentId={DepartmentId}",
                command.TaskId,
                command.DepartmentId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot assign task to department: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task to department: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while assigning the task to the department.");
        }
    }
}