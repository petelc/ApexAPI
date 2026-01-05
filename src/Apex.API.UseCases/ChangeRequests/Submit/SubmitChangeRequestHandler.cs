using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.Submit;
/// <summary>
/// Handler for submitting a ChangeRequest for review
/// </summary>
public class SubmitChangeRequestHandler : IRequestHandler<SubmitChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SubmitChangeRequestHandler> _logger;

    public SubmitChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<SubmitChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SubmitChangeRequestCommand command,
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

            // Submit the ChangeRequest (business logic in aggregate)
            changeRequest.Submit();

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation(
                "ChangeRequest submitted: ChangeRequestId={ChangeRequestId}, UserId={UserId}",
                command.ChangeRequestId,
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
            _logger.LogError(ex, "Error submitting ChangeRequest: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while submitting the ChangeRequest.");
        }
    }
}
