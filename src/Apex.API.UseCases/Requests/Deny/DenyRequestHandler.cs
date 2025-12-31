using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Requests.Deny;

/// <summary>
/// Handler for denying a request
/// </summary>
public class DenyRequestHandler : IRequestHandler<DenyRequestCommand, Result>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DenyRequestHandler> _logger;

    public DenyRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<DenyRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DenyRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if user has permission (TenantAdmin role)
            if (!_currentUserService.IsInRole("TenantAdmin"))
            {
                _logger.LogWarning(
                    "Unauthorized approve attempt: UserId={UserId}, RequestId={RequestId}",
                    _currentUserService.UserId,
                    command.RequestId);

                return Result.Forbidden();
            }

            var request = await _repository.GetByIdAsync(command.RequestId, cancellationToken);

            if (request == null)
            {
                return Result.NotFound("Request not found.");
            }

            // Verify tenant ownership
            if (request.TenantId != _tenantContext.CurrentTenantId)
            {
                return Result.Forbidden();
            }

            // Deny the request (business logic in aggregate)
            request.Deny(_currentUserService.UserId, command.Reason);

            await _repository.UpdateAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request denied: RequestId={RequestId}, DeniedBy={UserId}",
                command.RequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot deny request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error denying request: RequestId={RequestId}", command.RequestId);
            return Result.Error("An error occurred while denying the request.");
        }
    }
}
