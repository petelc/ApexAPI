using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Traxs.SharedKernel;
using Apex.API.Core.Aggregates.RequestAggregate;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Requests.Assign;

/// <summary>
/// Handler for assigning a request to a user
/// </summary>
public class AssignRequestHandler : IRequestHandler<AssignRequestCommand, Result>
{
    private readonly IRepository<Request> _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AssignRequestHandler> _logger;

    public AssignRequestHandler(
        IRepository<Request> repository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        UserManager<User> userManager,
        ILogger<AssignRequestHandler> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AssignRequestCommand command,
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

            // Validate that the assigned user exists and belongs to this tenant
            var assignedUser = await _userManager.FindByIdAsync(command.AssignedToUserId.ToString());

            if (assignedUser == null)
            {
                _logger.LogWarning(
                    "Cannot assign to non-existent user: UserId={UserId}",
                    command.AssignedToUserId);

                return Result.Error("Assigned user not found.");
            }

            // Verify the assigned user belongs to the same tenant (security!)
            if (assignedUser.TenantId != _tenantContext.CurrentTenantId)
            {
                _logger.LogWarning(
                    "Attempted cross-tenant assignment: RequestTenant={RequestTenant}, UserTenant={UserTenant}",
                    _tenantContext.CurrentTenantId,
                    assignedUser.TenantId);

                return Result.Error("Cannot assign to user from different tenant.");
            }

            // Verify assigned user is active
            if (!assignedUser.IsActive)
            {
                return Result.Error("Cannot assign to inactive user.");
            }

            // Assign the request (business logic in aggregate)
            request.AssignTo(
                command.AssignedToUserId,
                _currentUserService.UserId); // Who is doing the assigning

            await _repository.UpdateAsync(request, cancellationToken);

            _logger.LogInformation(
                "Request assigned: RequestId={RequestId}, AssignedTo={AssignedToUserId}, AssignedBy={AssignedByUserId}",
                command.RequestId,
                command.AssignedToUserId,
                _currentUserService.UserId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot assign request: {Message}", ex.Message);
            return Result.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning request: RequestId={RequestId}", command.RequestId);
            return Result.Error("An error occurred while assigning the request.");
        }
    }
}
