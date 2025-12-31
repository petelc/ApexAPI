using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.ProjectRequests.Cancel;

/// <summary>
/// Handler for canceling a ProjectRequest
/// </summary>
public class CancelProjectRequestHandler : IRequestHandler<CancelProjectRequestCommand, Result>
{
    private readonly IRepository<ProjectRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CancelProjectRequestHandler> _logger;
    public CancelProjectRequestHandler(
        IRepository<ProjectRequest> repository,
        ITenantContext tenantContext,
        ILogger<CancelProjectRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CancelProjectRequestCommand command,  // ✅ FIXED: Changed parameter name from 'ProjectRequest' to 'command'
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ FIXED: Now 'ProjectRequest' is only used for the domain entity
            var ProjectRequest = await _repository.GetByIdAsync(command.ProjectRequestId, cancellationToken);

            if (ProjectRequest == null)
            {
                return Result.NotFound("ProjectRequest not found.");
            }

            // Verify tenant ownership
            if (ProjectRequest.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Forbidden();
            }

            // Cancel the ProjectRequest (business logic in aggregate)
            ProjectRequest.Cancel();

            await _repository.UpdateAsync(ProjectRequest, cancellationToken);

            _logger.LogInformation(
                "ProjectRequest canceled: ProjectRequestId={ProjectRequestId}",
                command.ProjectRequestId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel ProjectRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling ProjectRequest: ProjectRequestId={ProjectRequestId}", command.ProjectRequestId);
            return Result.Error("An error occurred while canceling the ProjectRequest.");
        }
    }
}