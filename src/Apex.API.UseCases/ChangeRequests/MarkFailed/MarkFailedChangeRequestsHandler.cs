using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.ChangeRequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.ChangeRequests.MarkFailed;

public class MarkFailedChangeRequestHandler : IRequestHandler<MarkFailedChangeRequestCommand, Result>
{
    private readonly IRepository<ChangeRequest> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MarkFailedChangeRequestHandler> _logger;

    public MarkFailedChangeRequestHandler(
        IRepository<ChangeRequest> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<MarkFailedChangeRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(MarkFailedChangeRequestCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var changeRequest = await _repository.GetByIdAsync(command.ChangeRequestId, cancellationToken);
            if (changeRequest == null)
                return Result.NotFound("ChangeRequest not found.");

            if (changeRequest.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            changeRequest.MarkAsFailed(command.Reason);

            await _repository.UpdateAsync(changeRequest, cancellationToken);

            _logger.LogInformation("ChangeRequest marked as failed: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot mark ChangeRequest as failed: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking ChangeRequest as failed: ChangeRequestId={ChangeRequestId}", command.ChangeRequestId);
            return Result.Error("An error occurred while marking the ChangeRequest as failed.");
        }
    }
}