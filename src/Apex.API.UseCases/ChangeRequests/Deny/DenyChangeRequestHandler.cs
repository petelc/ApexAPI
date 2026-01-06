using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.Deny;
/// <summary>
/// Handler for denying a ChangeRequest
/// </summary>
public class DenyChangeRequestHandler : IRequestHandler<DenyChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DenyChangeRequestHandler> _logger;

    public DenyChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<DenyChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DenyChangeRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user has permission (TenantAdmin role)
            if (!_currentUserService.IsInRole("TenantAdmin"))
            {
                _logger.LogWarning(
                    "Unauthorized approve attempt: UserId={UserId}, ChangeRequestId={ChangeRequestId}",
                    _currentUserService.UserId,
                    command.ChangeRequestId);

                return Result.Forbidden();
            }

            var changeRequest = await _repository.GetByIdAsync(command.ChangeRequestId, cancellationToken);

            if (changeRequest == null)
            {
                return Result.NotFound("ChangeRequest not found.");
            }

            // Verify tenant ownership
            if (changeRequest.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Forbidden();
            }

            // Deny the ChangeRequest (business logic in aggregate)
            changeRequest.Deny(_currentUserService.UserId, command.Reason);

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation(
                "ChangeRequest denied: ChangeRequestId={ChangeRequestId}, DeniedBy={UserId}",
                command.ChangeRequestId,
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
            _logger.LogError(ex, "Error denying ChangeRequest: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while denying the ChangeRequest.");
        }
    }
}
