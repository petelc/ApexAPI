using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.Web.Endpoints.Admin;

/// <summary>
/// Update user (Admin only)
/// </summary>
public class UpdateUserEndpoint : Endpoint<UpdateUserRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateUserEndpoint> _logger;

    public UpdateUserEndpoint(
        UserManager<User> userManager,
        ITenantContext tenantContext,
        ILogger<UpdateUserEndpoint> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/admin/users/{userId}");
        Roles("TenantAdmin");
        
        Description(b => b
            .WithTags("Admin")
            .WithSummary("Update user")
            .WithDescription("Updates user information (admin only)."));
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
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

        // Update user properties
        if (!string.IsNullOrWhiteSpace(req.FirstName))
            user.FirstName = req.FirstName;
            
        if (!string.IsNullOrWhiteSpace(req.LastName))
            user.LastName = req.LastName;
            
        if (req.PhoneNumber != null)
            user.PhoneNumber = req.PhoneNumber;
            
        if (!string.IsNullOrWhiteSpace(req.TimeZone))
            user.TimeZone = req.TimeZone;

        // Admin can activate/deactivate users
        if (req.IsActive.HasValue)
            user.IsActive = req.IsActive.Value;

        // Note: Email changes require more careful handling in production
        if (!string.IsNullOrWhiteSpace(req.Email) && req.Email != user.Email)
        {
            user.Email = req.Email;
            user.NormalizedEmail = req.Email.ToUpperInvariant();
            user.UserName = req.Email;
            user.NormalizedUserName = req.Email.ToUpperInvariant();
        }

        user.LastModifiedDate = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("User updated by admin: UserId={UserId}", userId);
            
            // Return updated user
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
                roles = roles.ToList()
            };

            await HttpContext.Response.WriteAsJsonAsync(userDetails, ct);
        }
        else
        {
            _logger.LogWarning("Failed to update user: UserId={UserId}, Errors={Errors}", 
                userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => e.Description)
            }, ct);
        }
    }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public bool? IsActive { get; set; }
}
