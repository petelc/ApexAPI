using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Get current user's profile information
/// </summary>
public class GetCurrentUserEndpoint : EndpointWithoutRequest
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserEndpoint(
        UserManager<User> userManager,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public override void Configure()
    {
        Get("/users/me");
        // All authenticated users can access their own profile
        
        Description(b => b
            .WithTags("Users")
            .WithSummary("Get current user profile")
            .WithDescription("Returns the profile information for the currently authenticated user."));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        
        if (userId == Guid.Empty)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not authenticated" }, ct);
            return;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString().ToUpperInvariant());

        if (user == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "User not found" }, ct);
            return;
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        var userProfile = new
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
            roles = roles.ToList()
        };

        await HttpContext.Response.WriteAsJsonAsync(userProfile, ct);
    }
}
