using Microsoft.AspNetCore.Identity;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.Infrastructure.Services;

/// <summary>
/// Implementation of authentication service using ASP.NET Core Identity
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationService(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
    {
        // Find user
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new AuthenticationResult
            {
                Succeeded = false,
                ErrorMessage = "Invalid email or password."
            };
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return new AuthenticationResult
            {
                Succeeded = false,
                IsActive = false,
                ErrorMessage = "Your account has been deactivated. Please contact support."
            };
        }

        // Check password
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                return new AuthenticationResult
                {
                    Succeeded = false,
                    IsLockedOut = true,
                    ErrorMessage = "Your account has been locked due to multiple failed login attempts. Please try again later."
                };
            }

            return new AuthenticationResult
            {
                Succeeded = false,
                ErrorMessage = "Invalid email or password."
            };
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Record login
        user.RecordLogin();
        await _userManager.UpdateAsync(user);

        // Return success with user data
        return new AuthenticationResult
        {
            Succeeded = true,
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            TenantId = user.TenantId.Value,
            IsActive = user.IsActive,
            Roles = roles
        };
    }
}
