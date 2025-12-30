using MediatR;
using Microsoft.Extensions.Logging;
using Ardalis.Result;
using Apex.API.UseCases.Common.Interfaces;

namespace Apex.API.UseCases.Users.Login;

/// <summary>
/// Handles user login and JWT token generation
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IAuthenticationService authService,
        IJwtTokenService jwtTokenService,
        ILogger<LoginHandler> logger)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Login attempt: Email={Email}", request.Email);

            // Authenticate user
            var authResult = await _authService.AuthenticateAsync(request.Email, request.Password);

            if (!authResult.Succeeded)
            {
                _logger.LogWarning(
                    "Login failed: {Reason} - Email={Email}",
                    authResult.ErrorMessage,
                    request.Email);

                return Result<LoginResponse>.Error(authResult.ErrorMessage ?? "Login failed.");
            }

            // Generate tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(
                authResult.UserId!.Value,
                authResult.Email!,
                authResult.FullName!,
                authResult.FirstName!,
                authResult.LastName!,
                authResult.TenantId!.Value,
                authResult.Roles);
            
            var refreshToken = _jwtTokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1);

            // Save refresh token
            await _jwtTokenService.SaveRefreshToken(
                authResult.UserId.Value,
                refreshToken,
                DateTime.UtcNow.AddDays(7));

            _logger.LogInformation(
                "Login successful: UserId={UserId}, Email={Email}",
                authResult.UserId,
                authResult.Email);

            var response = new LoginResponse(
                accessToken,
                refreshToken,
                expiresAt,
                new UserInfo(
                    authResult.UserId.Value,
                    authResult.Email!,
                    authResult.FirstName!,
                    authResult.LastName!,
                    authResult.FullName!,
                    authResult.TenantId.Value,
                    authResult.Roles.ToArray()));

            return Result<LoginResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during login: Email={Email}",
                request.Email);

            return Result<LoginResponse>.Error("An unexpected error occurred during login.");
        }
    }
}
