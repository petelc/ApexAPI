using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Update current user's profile information
/// </summary>
public class UpdateCurrentUserEndpoint : Endpoint<UpdateCurrentUserRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateCurrentUserEndpoint> _logger;

    public UpdateCurrentUserEndpoint(
        UserManager<User> userManager,
        ICurrentUserService currentUserService,
        ILogger<UpdateCurrentUserEndpoint> logger)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/users/me");
        // All authenticated users can update their own profile
        
        Description(b => b
            .WithTags("Users")
            .WithSummary("Update current user profile")
            .WithDescription("Updates the profile information for the currently authenticated user."));
    }

    public override async Task HandleAsync(UpdateCurrentUserRequest req, CancellationToken ct)
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

        // Update user properties
        if (!string.IsNullOrWhiteSpace(req.FirstName))
            user.FirstName = req.FirstName;
            
        if (!string.IsNullOrWhiteSpace(req.LastName))
            user.LastName = req.LastName;
            
        if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
            user.PhoneNumber = req.PhoneNumber;
            
        if (!string.IsNullOrWhiteSpace(req.TimeZone))
            user.TimeZone = req.TimeZone;

        // Note: Email changes require email confirmation in production
        // For now, we'll allow direct email updates
        if (!string.IsNullOrWhiteSpace(req.Email) && req.Email != user.Email)
        {
            user.Email = req.Email;
            user.NormalizedEmail = req.Email.ToUpperInvariant();
            user.UserName = req.Email; // Assuming username = email
            user.NormalizedUserName = req.Email.ToUpperInvariant();
        }

        user.LastModifiedDate = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User profile updated: UserId={UserId}", userId);
            
            // Return updated profile
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
                roles = roles.ToList()
            };

            await HttpContext.Response.WriteAsJsonAsync(userProfile, ct);
        }
        else
        {
            _logger.LogWarning("Failed to update user profile: UserId={UserId}, Errors={Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => e.Description)
            }, ct);
        }
    }
}

public class UpdateCurrentUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
}
