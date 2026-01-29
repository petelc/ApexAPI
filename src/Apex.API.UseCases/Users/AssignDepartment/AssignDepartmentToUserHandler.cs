using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Users.AssignDepartment;

public class AssignDepartmentToUserHandler : IRequestHandler<AssignDepartmentToUserCommand, Result>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AssignDepartmentToUserHandler> _logger;

    public AssignDepartmentToUserHandler(
        UserManager<User> userManager,
        ITenantContext tenantContext,
        ILogger<AssignDepartmentToUserHandler> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AssignDepartmentToUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());

            if (user == null)
                return Result.NotFound("User not found.");

            // Verify user belongs to current tenant
            if (user.TenantId != _tenantContext.CurrentTenantId)
                return Result.Forbidden();

            // Update department
            user.DepartmentId = command.DepartmentId;
            user.LastModifiedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to assign department: {Errors}", errors);
                return Result.Error(errors);
            }

            _logger.LogInformation(
                "Department assigned: UserId={UserId}, DepartmentId={DepartmentId}",
                command.UserId,
                command.DepartmentId?.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning department to user: UserId={UserId}", command.UserId);
            return Result.Error("An error occurred while assigning the department.");
        }
    }
}
