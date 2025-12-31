using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ProjectRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ProjectRequests.Deny;

/// <summary>
/// Handler for denying a ProjectRequest
/// </summary>
public class DenyProjectRequestHandler : IRequestHandler<DenyProjectRequestCommand, Result>
{
    private readonly IRepository<ProjectRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DenyProjectRequestHandler> _logger;

    public DenyProjectRequestHandler(
        IRepository<ProjectRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<DenyProjectRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DenyProjectRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user has permission (TenantAdmin role)
            if (!_currentUserService.IsInRole("TenantAdmin"))
            {
                _logger.LogWarning(
                    "Unauthorized approve attempt: UserId={UserId}, ProjectRequestId={ProjectRequestId}",
                    _currentUserService.UserId,
                    command.ProjectRequestId);

                return Result.Forbidden();
            }

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

            // Deny the ProjectRequest (business logic in aggregate)
            projectRequest.Deny(_currentUserService.UserId, command.Reason);

            await _repository.UpdateAsync(projectRequest, cancellationToken);

            _logger.LogInformation(
                "ProjectRequest denied: ProjectRequestId={ProjectRequestId}, DeniedBy={UserId}",
                command.ProjectRequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot deny ProjectRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error denying ProjectRequest: ProjectRequestId={ProjectRequestId}", command.ProjectRequestId);
            return Result.Error("An error occurred while denying the ProjectRequest.");
        }
    }
}
