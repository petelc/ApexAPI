using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Requests.Submit;

/// <summary>
/// Handler for submitting a request for review
/// </summary>
public class SubmitRequestHandler : IRequestHandler<SubmitRequestCommand, Result>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SubmitRequestHandler> _logger;

    public SubmitRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<SubmitRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        SubmitRequestCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
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

            // Submit the request (business logic in aggregate)
            request.Submit(_currentUserService.UserId);

            await _repository.UpdateAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request submitted: RequestId={RequestId}, UserId={UserId}",
                command.RequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot submit request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting request: RequestId={RequestId}", command.RequestId);
            return Result.Error("An error occurred while submitting the request.");
        }
    }
}
