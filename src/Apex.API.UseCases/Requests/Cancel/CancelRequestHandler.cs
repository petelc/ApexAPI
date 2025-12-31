using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Requests.Cancel;

/// <summary>
/// Handler for canceling a request
/// </summary>
public class CancelRequestHandler : IRequestHandler<CancelRequestCommand, Result>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CancelRequestHandler> _logger;

    public CancelRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ILogger<CancelRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CancelRequestCommand command,  // ✅ FIXED: Changed parameter name from 'request' to 'command'
        CancellationToken cancellationToken)
    {
        try
        {
            // ✅ FIXED: Now 'request' is only used for the domain entity
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

            // Cancel the request (business logic in aggregate)
            request.Cancel();

            await _repository.UpdateAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request canceled: RequestId={RequestId}",
                command.RequestId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling request: RequestId={RequestId}", command.RequestId);
            return Result.Error("An error occurred while canceling the request.");
        }
    }
}