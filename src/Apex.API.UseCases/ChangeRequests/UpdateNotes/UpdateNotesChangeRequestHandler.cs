using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.UpdateNotes;

/// <summary>
/// Handler for updating a ChangeRequest
/// </summary>
public class UpdateNotesChangeRequestHandler : IRequestHandler<UpdateNotesChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateNotesChangeRequestHandler> _logger;

    public UpdateNotesChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<UpdateNotesChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateNotesChangeRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
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

            // Only the creator can update their own ChangeRequest (optional - you can remove this check)
            if (changeRequest.CreatedByUserId != _currentUserService.UserId)
            {
                _logger.LogWarning(
                    "User attempted to update another user's ChangeRequest: ChangeRequestId={ChangeRequestId}, UserId={UserId}, CreatedBy={CreatedBy}",
                    command.ChangeRequestId,
                    _currentUserService.UserId,
                    changeRequest.CreatedByUserId);

                return Result.Error("You can only update your own requests.");
            }

            changeRequest.UpdateImplementationNotes(command.Notes);

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation(
                "ChangeRequest updated: ChangeRequestId={ChangeRequestId}, UpdatedBy={UserId}",
                command.ChangeRequestId,
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
            _logger.LogError(ex, "Error updating ChangeRequest: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while updating the ChangeRequest.");
        }
    }
}