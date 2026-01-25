using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Web.Endpoints.Users;

/// <summary>
/// Change current user's password
/// </summary>
public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChangePasswordEndpoint> _logger;

    public ChangePasswordEndpoint(
        UserManager<User> userManager,
        ICurrentUserService currentUserService,
        ILogger<ChangePasswordEndpoint> logger)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users/me/change-password");
        // All authenticated users can change their own password
        
        Description(b => b
            .WithTags("Users")
            .WithSummary("Change password")
            .WithDescription("Changes the password for the currently authenticated user."));
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
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

        // Validate passwords match
        if (req.NewPassword != req.ConfirmNewPassword)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { "New password and confirmation do not match" }
            }, ct);
            return;
        }

        var result = await _userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation("Password changed successfully: UserId={UserId}", userId);
            
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                message = "Password changed successfully"
            }, ct);
        }
        else
        {
            _logger.LogWarning("Failed to change password: UserId={UserId}, Errors={Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => e.Description)
            }, ct);
        }
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
