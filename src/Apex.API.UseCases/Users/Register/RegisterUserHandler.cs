using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Core.Interfaces;

namespace Apex.API.UseCases.Users.Register;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
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

    public async Task<Result<Guid>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Registering user: Email={Email}, Tenant={TenantId}",
                command.Email,
                _tenantContext.CurrentTenantId);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(command.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User already exists: Email={Email}", command.Email);
                return Result<Guid>.Error("A user with this email already exists.");
            }

            // Create user entity
            var user = User.Create(
                _tenantContext.CurrentTenantId, // ✅ FIXED
                command.Email,
                command.FirstName,
                command.LastName,
                command.PhoneNumber,
                command.TimeZone);
            var createResult = await _userManager.CreateAsync(user, command.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create user: {Errors}", errors);
                return Result<Guid>.Error(errors);
            }

            // Assign default "User" role
            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign User role to {Email}", command.Email);
            }

            _logger.LogInformation(
                "User registered successfully: UserId={UserId}, Email={Email}",
                user.Id,
                command.Email);

            return Result<Guid>.Success(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: Email={Email}", command.Email);
            return Result<Guid>.Error("An error occurred while registering the user.");
        }
    }
}