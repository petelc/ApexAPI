using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Requests.Complete;

/// <summary>
/// Handler for completing a request
/// </summary>
public class CompleteRequestHandler : IRequestHandler<CompleteRequestCommand, Result>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompleteRequestHandler> _logger;
    public CompleteRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CompleteRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        CompleteRequestCommand command,  // ✅ FIXED: Changed parameter name from 'request' to 'command'
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

            // Complete the request (business logic in aggregate)
            request.Complete(_currentUserService.UserId);

            await _repository.UpdateAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request completed: RequestId={RequestId}",
                command.RequestId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot complete request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing request: RequestId={RequestId}", command.RequestId);
            return Result.Error("An error occurred while completing the request.");
        }
    }
}