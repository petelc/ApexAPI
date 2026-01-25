using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.Admin;

/// <summary>
/// Assign a role to a user
/// </summary>
public class AssignRoleToUserEndpoint : Endpoint<AssignRoleRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;

    public AssignRoleToUserEndpoint(
        UserManager<User> userManager,
        ITenantContext tenantContext)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    public override void Configure()
    {
        Post("/admin/users/{userId}/roles");
        Roles("TenantAdmin");

        Description(b => b
            .WithTags("Admin")
            .WithSummary("Assign role to user")
            .WithDescription("Assigns a role to a user in the current tenant."));
    }

    public override async Task HandleAsync(AssignRoleRequest req, CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not found" }, ct);
            return;
        }

        // Verify user belongs to current tenant
        if (user.TenantId.Value != _tenantContext.CurrentTenantId.Value)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not in your tenant" }, ct);
            return;
        }

        // Check if user already has the role
        if (await _userManager.IsInRoleAsync(user, req.RoleName))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = $"User already has role '{req.RoleName}'" }, ct);
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, req.RoleName);

        if (result.Succeeded)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                message = $"Role '{req.RoleName}' assigned to user {user.Email}"
            }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => e.Description)
            }, ct);
        }
    }
}

public class AssignRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>
/// Remove a role from a user
/// </summary>
public class RemoveRoleFromUserEndpoint : EndpointWithoutRequest
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;

    public RemoveRoleFromUserEndpoint(
        UserManager<User> userManager,
        ITenantContext tenantContext)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    public override void Configure()
    {
        Delete("/admin/users/{userId}/roles/{roleName}");
        Roles("TenantAdmin");

        Description(b => b
            .WithTags("Admin")
            .WithSummary("Remove role from user")
            .WithDescription("Removes a role from a user in the current tenant."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        var roleName = Route<string>("roleName");

        if (string.IsNullOrEmpty(roleName))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Role name is required" }, ct);
            return;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not found" }, ct);
            return;
        }

        // Verify user belongs to current tenant
        if (user.TenantId.Value != _tenantContext.CurrentTenantId.Value)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not in your tenant" }, ct);
            return;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                message = $"Role '{roleName}' removed from user {user.Email}"
            }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => e.Description)
            }, ct);
        }
    }
}
