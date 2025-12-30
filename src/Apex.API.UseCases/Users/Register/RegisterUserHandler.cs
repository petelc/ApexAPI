using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Users.Register;

/// <summary>
/// Handles user registration within a tenant
/// </summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserId>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        UserManager<User> userManager,
        ITenantContext tenantContext,
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<UserId>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Registering user: Email={Email}, Tenant={TenantId}",
                request.Email,
                _tenantContext.CurrentTenantId); // ✅ FIXED

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User already exists: {Email}", request.Email);
                return Result<UserId>.Error("A user with this email already exists.");
            }

            // Create user entity
            var user = User.Create(
                _tenantContext.CurrentTenantId, // ✅ FIXED
                request.Email,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.TimeZone);

            // Create user with password
            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {Errors}", errors);
                return Result<UserId>.Error(errors);
            }

            // Assign default role
            var roleResult = await _userManager.AddToRoleAsync(user, Role.SystemRoles.User);

            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign role to user: {Email}", request.Email);
                // Continue anyway - user is created
            }

            _logger.LogInformation(
                "User registered successfully: UserId={UserId}, Email={Email}",
                user.Id,
                user.Email);

            return Result<UserId>.Success(UserId.From(user.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error registering user: Email={Email}",
                request.Email);

            return Result<UserId>.Error("An unexpected error occurred while registering the user.");
        }
    }
}