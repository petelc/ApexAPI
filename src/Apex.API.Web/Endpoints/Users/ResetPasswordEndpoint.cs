using Apex.API.Core.Aggregates.UserAggregate;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Apex.API.Web.Endpoints.Auth;

/// <summary>
/// Request DTO for password reset
/// </summary>
public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Resets user password using reset token
/// </summary>
public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ResetPasswordEndpoint> _logger;

    public ResetPasswordEndpoint(
        UserManager<User> userManager,
        ILogger<ResetPasswordEndpoint> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users/reset-password");
        AllowAnonymous();
        Description(b => b
            .WithTags("Authentication")
            .WithSummary("Reset password")
            .WithDescription("Resets the user's password using a valid reset token"));
    }

    public override async Task HandleAsync(ResetPasswordRequest req, CancellationToken ct)
    {
        // Validate passwords match
        if (req.NewPassword != req.ConfirmPassword)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Passwords do not match"
            }, ct);
            return;
        }

        // Validate password strength (basic check)
        if (req.NewPassword.Length < 8)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Password must be at least 8 characters long"
            }, ct);
            return;
        }

        try
        {
            // Find user by email
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email, ct);

            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent email: {Email}", req.Email);
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid password reset request"
                }, ct);
                return;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Password reset attempted for inactive user: {Email}", req.Email);
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Invalid password reset request"
                }, ct);
                return;
            }

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Password reset failed for user {Email}. Errors: {Errors}",
                    req.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                // Check if token is invalid/expired
                var tokenError = result.Errors.FirstOrDefault(e =>
                    e.Code.Contains("InvalidToken", StringComparison.OrdinalIgnoreCase));

                if (tokenError != null)
                {
                    HttpContext.Response.StatusCode = 400;
                    await HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Password reset link is invalid or has expired. Please request a new one."
                    }, ct);
                    return;
                }

                // Other validation errors
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Password reset failed",
                    details = result.Errors.Select(e => e.Description).ToList()
                }, ct);
                return;
            }

            _logger.LogInformation("Password successfully reset for user: {Email}", user.Email);

            // Update last modified date
            user.LastModifiedDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                message = "Password has been reset successfully. You can now log in with your new password."
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", req.Email);
            HttpContext.Response.StatusCode = 500;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "An error occurred while resetting your password. Please try again."
            }, ct);
        }
    }
}
