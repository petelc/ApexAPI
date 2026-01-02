using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.TaskAggregate;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Tasks.Create;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<TaskId>>
{
    private readonly IRepository<Apex.API.Core.Aggregates.TaskAggregate.Task> _taskRepository;
    private readonly IReadRepository<Project> _projectRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTaskHandler> _logger;

    public CreateTaskHandler(
        IRepository<Apex.API.Core.Aggregates.TaskAggregate.Task> taskRepository,
        IReadRepository<Project> projectRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CreateTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<TaskId>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Verify project exists and belongs to tenant
            var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
            if (project == null)
                return Result<TaskId>.NotFound("Project not found.");

            if (project.TenantId != _tenantContext.CurrentTenantId)
                return Result<TaskId>.Forbidden();

            // Parse priority
            if (!RequestPriority.TryFromName(command.Priority, out var priority))
                return Result<TaskId>.Error($"Invalid priority: {command.Priority}");

            // Create task
            var task = Apex.API.Core.Aggregates.TaskAggregate.Task.Create(
                _tenantContext.CurrentTenantId,
                command.ProjectId,
                command.Title,
                command.Description,
                priority,
                _currentUserService.UserId,
                command.EstimatedHours,
                command.DueDate);

            await _taskRepository.AddAsync(task, cancellationToken);

            _logger.LogInformation("Task created: TaskId={TaskId}, ProjectId={ProjectId}", task.Id, command.ProjectId);

            return Result<TaskId>.Success(task.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<TaskId>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return Result<TaskId>.Error("An error occurred while creating the task.");
        }
    }
}