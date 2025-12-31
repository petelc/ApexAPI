using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ProjectRequests.Submit;

/// <summary>
/// Handler for submitting a ProjectRequest for review
/// </summary>
public class SubmitProjectRequestHandler : IRequestHandler<SubmitProjectRequestCommand, Result>
{
    private readonly IRepository<ProjectRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SubmitProjectRequestHandler> _logger;

    public SubmitProjectRequestHandler(
        IRepository<ProjectRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<SubmitProjectRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SubmitProjectRequestCommand command,
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

            // Submit the ProjectRequest (business logic in aggregate)
            projectRequest.Submit(_currentUserService.UserId);

            await _repository.UpdateAsync(projectRequest, cancellationToken);

            _logger.LogInformation(
                "ProjectRequest submitted: ProjectRequestId={ProjectRequestId}, UserId={UserId}",
                command.ProjectRequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot submit ProjectRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting ProjectRequest: ProjectRequestId={ProjectRequestId}", command.ProjectRequestId);
            return Result.Error("An error occurred while submitting the ProjectRequest.");
        }
    }
}
