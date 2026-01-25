using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;
using Apex.API.Core.ValueObjects;
using DomainUser = Apex.API.Core.Aggregates.UserAggregate.User;

namespace Apex.API.Web.Endpoints.Admin;

/// <summary>
/// Create a new user (Admin only)
/// </summary>
public class CreateUserEndpoint : Endpoint<CreateUserRequest>
{
    private readonly UserManager<DomainUser> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateUserEndpoint> _logger;

    public CreateUserEndpoint(
        UserManager<DomainUser> userManager,
        ITenantContext tenantContext,
        ILogger<CreateUserEndpoint> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/admin/users");
        Roles("TenantAdmin");

        Description(b => b
            .WithTags("Admin")
            .WithSummary("Create new user")
            .WithDescription("Creates a new user in the current tenant (admin only)."));
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        // Validate passwords match
        if (req.Password != req.ConfirmPassword)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { "Password and confirmation do not match" }
            }, ct);
            return;
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(req.Email);
        if (existingUser != null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { "A user with this email already exists" }
            }, ct);
            return;
        }

        // Create user entity using factory method
        var user = DomainUser.Create(
            _tenantContext.CurrentTenantId,
            req.Email,
            req.FirstName,
            req.LastName,
            req.PhoneNumber,
            req.TimeZone ?? "America/New_York"
        );

        // Set additional properties for admin-created users
        user.EmailConfirmed = true;
        user.IsActive = req.IsActive ?? true;

        var result = await _userManager.CreateAsync(user, req.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created by admin: UserId={UserId}, Email={Email}",
                user.Id, user.Email);

            // Assign roles if provided
            if (req.Roles != null && req.Roles.Any())
            {
                foreach (var role in req.Roles)
                {
                    try
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to assign role {Role} to user {UserId}",
                            role, user.Id);
                    }
                }
            }

            // Get assigned roles
            var assignedRoles = await _userManager.GetRolesAsync(user);

            // Return created user
            var userResponse = new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                fullName = user.FullName,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                timeZone = user.TimeZone,
                isActive = user.IsActive,
                tenantId = user.TenantId.Value,
                createdDate = user.CreatedDate,
                roles = assignedRoles.ToList()
            };

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(userResponse, ct);
        }
        else
        {
            _logger.LogWarning("Failed to create user: Email={Email}, Errors={Errors}",
                req.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => e.Description)
            }, ct);
        }
    }
}

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public List<string>? Roles { get; set; }
}
