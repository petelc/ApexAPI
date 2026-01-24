using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Users.DTOs;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Get users by role (filtered by current tenant)
/// </summary>
public class GetUsersByRoleEndpoint : Endpoint<GetUsersByRoleRequest, List<UserSummaryDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetUsersByRoleEndpoint> _logger;

    public GetUsersByRoleEndpoint(
        UserManager<User> userManager,
        ITenantContext tenantContext,
        ILogger<GetUsersByRoleEndpoint> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/users/by-role/{role}");
        AllowAnonymous(); // TODO: Add auth policy
        
        Description(b => b
            .WithTags("Users")
            .WithSummary("Get users by role (filtered by tenant)")
            .WithDescription("Returns all users with the specified role in the current tenant."));
    }

    public override async Task HandleAsync(GetUsersByRoleRequest req, CancellationToken ct)
    {
        _logger.LogInformation("Getting users with role '{Role}' for tenant {TenantId}", 
            req.Role, _tenantContext.CurrentTenantId);

        // Get all users with the specified role (across all tenants)
        var usersInRole = await _userManager.GetUsersInRoleAsync(req.Role);
        
        _logger.LogInformation("Found {Count} total users with role '{Role}'", usersInRole.Count, req.Role);

        // ✅ CRITICAL: Filter by current tenant
        var tenantUsers = usersInRole
            .Where(u => u.TenantId == _tenantContext.CurrentTenantId)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty
            })
            .ToList();

        _logger.LogInformation("Filtered to {Count} users in current tenant", tenantUsers.Count);

        await HttpContext.Response.WriteAsJsonAsync(tenantUsers, ct);
    }
}

public class GetUsersByRoleRequest
{
    public string Role { get; set; } = string.Empty;
}
