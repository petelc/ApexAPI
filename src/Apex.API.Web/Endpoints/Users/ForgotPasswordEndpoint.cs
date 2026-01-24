using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.UseCases.Users.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Apex.API.Web.Endpoints.Auth;

/// <summary>
/// Initiates password reset process by sending reset token via email
/// </summary>
public class ForgotPasswordEndpoint : Endpoint<ForgotPasswordRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordEndpoint> _logger;
    private readonly IConfiguration _configuration;

    public ForgotPasswordEndpoint(
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<ForgotPasswordEndpoint> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/users/forgot-password");
        AllowAnonymous();
        Tags("Users");
        Description(b => b
            .WithTags("Users")
            .WithSummary("Initiate password reset")
            .WithDescription("Sends a password reset token to the user's email address"));
    }

    public override async Task HandleAsync(ForgotPasswordRequest req, CancellationToken ct)
    {
        var successMessage = "If the email exists, a password reset link has been sent.";

        try
        {
            // Find user by email
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email, ct);

            // IMPORTANT: Always return success even if user not found (security best practice)
            // This prevents email enumeration attacks
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", req.Email);
                Response = new { message = successMessage };
                HttpContext.Response.StatusCode = 200;
                return;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Password reset requested for inactive user: {Email}", req.Email);
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(new { message = successMessage }, ct);
                return;
            }

            // Ensure user has a security stamp (required for token generation)
            if (string.IsNullOrEmpty(user.SecurityStamp))
            {
                _logger.LogInformation("Updating missing security stamp for user: {Email}", user.Email);
                await _userManager.UpdateSecurityStampAsync(user);
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            _logger.LogInformation("Generated password reset token for user: {Email}. Token length: {TokenLength}",
                user.Email, token?.Length ?? 0);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to generate password reset token for user: {Email}", user.Email);
                HttpContext.Response.StatusCode = 200;
                await HttpContext.Response.WriteAsJsonAsync(new { message = successMessage }, ct);
                return;
            }

            // Create reset URL (this would be your frontend URL)
            var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
            var resetUrl = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email ?? string.Empty)}&token={Uri.EscapeDataString(token)}";

            _logger.LogInformation("Reset URL created: {ResetUrl}", resetUrl);

            // Send email with reset link
            var emailSubject = "Password Reset Request";
            var emailBody = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {user.FirstName},</p>
                <p>We received a request to reset your password. Click the link below to reset your password:</p>
                <p><a href=""{resetUrl}"">Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
            ";

            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                user.FirstName,
                emailSubject,
                emailBody,
                null,
                ct);

            _logger.LogInformation("Password reset email sent to: {Email}", user.Email);

            // Always return same response for security
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new { message = successMessage }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", req.Email);

            // Still return success to prevent information disclosure
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new { message = successMessage }, ct);
        }
    }
}
