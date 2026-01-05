using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.Cancel;

/// <summary>
/// Handler for canceling a ChangeRequest
/// </summary>
public class CancelChangeRequestHandler : IRequestHandler<CancelChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CancelChangeRequestHandler> _logger;
    public CancelChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ILogger<CancelChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CancelChangeRequestCommand command,  // ✅ FIXED: Changed parameter name from 'ProjectRequest' to 'command'
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ FIXED: Now 'ChangeRequest' is only used for the domain entity
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

            // Cancel the ChangeRequest (business logic in aggregate)
            changeRequest.Cancel();

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation(
                "ChangeRequest canceled: ChangeRequestId={ChangeRequestId}",
                command.ChangeRequestId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel ChangeRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling ChangeRequest: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while canceling the ChangeRequest.");
        }
    }
}