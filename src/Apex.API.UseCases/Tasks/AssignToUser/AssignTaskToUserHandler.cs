using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.AssignToUser;

public class AssignTaskToUserHandler : IRequestHandler<AssignTaskToUserCommand, Result>
{
    private readonly IRepository<Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AssignTaskToUserHandler> _logger;

    public AssignTaskToUserHandler(
        IRepository<Core.Aggregates.TaskAggregate.Task> taskRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        ILogger<AssignTaskToUserHandler> logger)
    {
        _taskRepository = taskRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AssignTaskToUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

            if (task == null)
                return Result.NotFound("Task not found.");

            if (task.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            // Verify user exists and belongs to tenant
            var user = await _userManager.FindByIdAsync(command.AssignedToUserId.ToString());

            if (user == null)
                return Result.Error("User not found.");

            if (user.TenantId != _tenantContext.CurrentTenantId)
                return Result.Error("User does not belong to this tenant.");

            // Assign to user
            task.AssignToUser(
                command.AssignedToUserId,
                user.DepartmentId,
                _currentUserService.UserId);

            await _taskRepository.UpdateAsync(task, cancellationToken);

            _logger.LogInformation(
                "Task assigned to user: TaskId={TaskId}, UserId={UserId}",
                command.TaskId,
                command.AssignedToUserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot assign task: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task to user: TaskId={TaskId}", command.TaskId);
            return Result.Error("An error occurred while assigning the task.");
        }
    }
}