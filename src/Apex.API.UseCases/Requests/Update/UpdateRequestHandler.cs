using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Requests.Update;

/// <summary>
/// Handler for updating a request
/// </summary>
public class UpdateRequestHandler : IRequestHandler<UpdateRequestCommand, Result>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateRequestHandler> _logger;

    public UpdateRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<UpdateRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdateRequestCommand command,
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

            // Only the creator can update their own request (optional - you can remove this check)
            if (request.CreatedByUserId != _currentUserService.UserId)
            {
                _logger.LogWarning(
                    "User attempted to update another user's request: RequestId={RequestId}, UserId={UserId}, CreatedBy={CreatedBy}",
                    command.RequestId,
                    _currentUserService.UserId,
                    request.CreatedByUserId);

                return Result.Error("You can only update your own requests.");
            }

            // Parse priority if provided
            RequestPriority priority = request.Priority; // Keep current priority if not specified
            if (!string.IsNullOrWhiteSpace(command.Priority))
            {
                if (!RequestPriority.TryFromName(command.Priority, out var parsedPriority))
                {
                    return Result.Error($"Invalid priority: {command.Priority}. Valid values: Low, Medium, High, Urgent");
                }
                priority = parsedPriority;
            }

            // Update the request (business logic in aggregate)
            request.Update(
                command.Title,
                command.Description,
                priority,
                command.DueDate);

            await _repository.UpdateAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request updated: RequestId={RequestId}, UpdatedBy={UserId}",
                command.RequestId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot update request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating request: RequestId={RequestId}", command.RequestId);
            return Result.Error("An error occurred while updating the request.");
        }
    }
}