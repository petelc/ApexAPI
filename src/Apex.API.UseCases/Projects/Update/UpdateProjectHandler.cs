using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Projects.Update;

/// <summary>
/// Handler for updating project details
/// </summary>
public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, Result>
{
    private readonly IRepository<Project> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateProjectHandler> _logger;

    public UpdateProjectHandler(
        IRepository<Project> repository,
        ITenantContext tenantContext,
        ILogger<UpdateProjectHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);

            if (project == null)
            {
                _logger.LogWarning("Project not found: ProjectId={ProjectId}", command.ProjectId);
                return Result.NotFound("Project not found.");
            }

            // Verify tenant ownership
            if (project.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt: ProjectId={ProjectId}, TenantId={TenantId}",
                    command.ProjectId,
                    _tenantContext.CurrentTenantId);
                return Result.Forbidden();
            }
            

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                project.UpdateName(command.Name);
            }

            // Update description if provided
            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                project.UpdateDescription(command.Description);
            }

            // Update budget if provided
            if (command.Budget.HasValue)
            {
                project.UpdateBudget(command.Budget.Value);
            }

            // Update dates if provided
            if (command.StartDate.HasValue || command.EndDate.HasValue)
            {
                var startDate = command.StartDate ?? project.StartDate;
                var endDate = command.EndDate ?? project.EndDate;
                
                if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                {
                    return Result.Error("Start date cannot be after end date.");
                }

                if (command.StartDate.HasValue)
                    project.UpdateStartDate(command.StartDate);
                    
                if (command.EndDate.HasValue)
                    project.UpdateEndDate(command.EndDate);
            }

            // Update priority if provided
            if (!string.IsNullOrWhiteSpace(command.Priority))
            {
                if (!RequestPriority.TryFromName(command.Priority, out var priority))
                {
                    return Result.Error($"Invalid priority: {command.Priority}. Valid values: Low, Medium, High, Urgent");
                }
                project.UpdatePriority(priority);
            }

            await _repository.UpdateAsync(project, cancellationToken);

            _logger.LogInformation(
                "Project updated successfully: ProjectId={ProjectId}, Name={Name}",
                project.Id,
                project.Name);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating project: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating project: ProjectId={ProjectId}", command.ProjectId);
            return Result.Error("An unexpected error occurred while updating the project.");
        }
    }
}
