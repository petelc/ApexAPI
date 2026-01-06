using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.Approve;
/// <summary>
/// Handler for approving a ChangeRequest
/// </summary>
public class ApproveChangeRequestHandler : IRequestHandler<ApproveChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApproveChangeRequestHandler> _logger;

    public ApproveChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<ApproveChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        ApproveChangeRequestCommand command,
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

            // Approve the ChangeRequest (business logic in aggregate)
            changeRequest.Approve(_currentUserService.UserId, command.Notes);

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation(
                "ChangeRequest approved: ChangeRequestId={ChangeRequestId}, ApprovedBy={UserId}",
                command.ChangeRequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot approve ProjectRequest: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving ChangeRequest: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while approving the ChangeRequest.");
        }
    }
}
