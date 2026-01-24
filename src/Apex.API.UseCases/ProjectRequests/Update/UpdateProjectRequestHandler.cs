using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ProjectRequests.Update;

/// <summary>
/// Handler for updating a ProjectRequest
/// </summary>
public class UpdateProjectRequestHandler : IRequestHandler<UpdateProjectRequestCommand, Result>
{
    private readonly IRepository<ProjectRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateProjectRequestHandler> _logger;

    public UpdateProjectRequestHandler(
        IRepository<ProjectRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<UpdateProjectRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateProjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var projectRequest = await _repository.GetByIdAsync(command.ProjectRequestId, cancellationToken);

            if (projectRequest == null)
            {
                return Result.NotFound("ProjectRequest not found.");
            }

            // Verify tenant ownership
            if (projectRequest.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Forbidden();
            }

            // Only the creator can update their own ProjectRequest (optional - you can remove this check)
            if (projectRequest.CreatedByUserId != _currentUserService.UserId)
            {
                _logger.LogWarning(
                    "User attempted to update another user's ProjectRequest: ProjectRequestId={ProjectRequestId}, UserId={UserId}, CreatedBy={CreatedBy}",
                    command.ProjectRequestId,
                    _currentUserService.UserId,
                    projectRequest.CreatedByUserId);

                return Result.Error("You can only update your own requests.");
            }

            // Parse priority if provided
            RequestPriority priority = projectRequest.Priority; // Keep current priority if not specified
            if (!string.IsNullOrWhiteSpace(command.Priority))
            {
                if (!RequestPriority.TryFromName(command.Priority, out var parsedPriority))
                {
                    return Result.Error($"Invalid priority: {command.Priority}. Valid values: Low, Medium, High, Urgent");
                }
                priority = parsedPriority;
            }

            // Update the ProjectRequest (business logic in aggregate)
            projectRequest.Update(
                command.Title,
                command.Description,
                command.BusinessJustification,
                priority,
                command.DueDate,
                command.EstimatedBudget,
                command.ProposedStartDate,
                command.ProposedEndDate);

            await _repository.UpdateAsync(projectRequest, cancellationToken);

            _logger.LogInformation(
                "ProjectRequest updated: ProjectRequestId={ProjectRequestId}, UpdatedBy={UserId}",
                command.ProjectRequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot update ProjectRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating ProjectRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjectRequest: ProjectRequestId={ProjectRequestId}", command.ProjectRequestId);
            return Result.Error("An error occurred while updating the ProjectRequest.");
        }
    }
}