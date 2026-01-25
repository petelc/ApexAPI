using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.Admin;

/// <summary>
/// Get user by ID (Admin only)
/// </summary>
public class GetUserByIdEndpoint : EndpointWithoutRequest
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;

    public GetUserByIdEndpoint(
        UserManager<User> userManager,
        ITenantContext tenantContext)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    public override void Configure()
    {
        Get("/admin/users/{userId}");
        Roles("TenantAdmin");
        
        Description(b => b
            .WithTags("Admin")
            .WithSummary("Get user by ID")
            .WithDescription("Returns detailed information about a specific user (admin only)."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        
        var user = await _userManager.FindByIdAsync(userId.ToString().ToUpperInvariant());

        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not found" }, ct);
            return;
        }

        // Verify user belongs to current tenant
        if (user.TenantId != _tenantContext.CurrentTenantId)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not in your tenant" }, ct);
            return;
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        var userDetails = new
        {
            id = user.Id,
            email = user.Email,
            userName = user.UserName,
            fullName = user.FullName,
            firstName = user.FirstName,
            lastName = user.LastName,
            phoneNumber = user.PhoneNumber,
            timeZone = user.TimeZone,
            profileImageUrl = user.ProfileImageUrl,
            isActive = user.IsActive,
            tenantId = user.TenantId.Value,
            departmentId = user.DepartmentId?.Value,
            lastLoginDate = user.LastLoginDate,
            createdDate = user.CreatedDate,
            lastModifiedDate = user.LastModifiedDate,
            roles = roles.ToList()
        };

        await HttpContext.Response.WriteAsJsonAsync(userDetails, ct);
    }
}
