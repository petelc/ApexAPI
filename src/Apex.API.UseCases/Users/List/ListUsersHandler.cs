using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Users.List;

/// <summary>
/// Handler for listing users in the current tenant
/// </summary>
public class ListUsersHandler : IRequestHandler<ListUsersQuery, Result<List<UserDto>>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListUsersHandler> _logger;

    public ListUsersHandler(
        UserManager<User> userManager,
        ITenantContext tenantContext,
        ILogger<ListUsersHandler> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<List<UserDto>>> Handle(
        ListUsersQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get all users for current tenant
            var users = _userManager.Users
                .Where(u => u.TenantId == _tenantContext.CurrentTenantId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                userDtos.Add(new UserDto(
                    user.Id,
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    user.FullName,
                    user.IsActive,
                    roles));
            }

            _logger.LogInformation(
                "Listed users: Count={Count}, TenantId={TenantId}",
                userDtos.Count,
                _tenantContext.CurrentTenantId);

            return Result<List<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            return Result<List<UserDto>>.Error("An error occurred while listing users.");
        }
    }
}
